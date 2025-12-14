using CalamityMod;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

internal class OtherworldlyCore : ModNPC
{
    public Rope Cord;

    public voidVulture Body;

    private bool PreparingToShoot;

    private float TelegraphInterp;

    public voidVulture.Behavior CurrentBehavior => Body != null ? Body.currentState : default;

    public int Time => Body != null ? Body.Time : 0;

    //AAAAAAAAAAAAAAAAAAAAAA
    public override bool CheckActive()
    {
        return false;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = NPC.Hitbox;
    }

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.MPAllowedEnemies[Type] = true;

        EmptinessSprayer.NPCsToNotDelete[Type] = true;
        RocheLimitGlobalNPC.ImmuneToLobotomy[Type] = true;
        NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
    }

    public override void OnSpawn(IEntitySource source) { }

    public override void SetDefaults()
    {
        NPC.noGravity = true;
        NPC.lifeMax = 30;
        NPC.defense = 199;
        NPC.damage = 0;
        NPC.Size = new Vector2(100, 100);
        NPC.noTileCollide = true;

        if (Main.netMode != NetmodeID.Server)
        {
            NPCNameFontSystem.RegisterFontForNPCID(Type, DisplayName.Value, Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/WINDLISTENERGRAPHIC", AssetRequestMode.ImmediateLoad).Value);
        }
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.IsMinionOrSentryRelated)
        {
            modifiers.Knockback *= 0.1f;
            modifiers.FinalDamage *= 0.75f;
        }

        var player = Main.player[projectile.owner];
        modifiers.Knockback *= LumUtils.InverseLerp(1000, 0, player.Distance(NPC.Center));
        //Main.NewText(modifiers.Knockback.Multiplicative);
    }

    public override void AI()
    {
        if (Body == null)
        {
            return;
        }

        if (Body != null)
        {
            Cord = new Rope(NPC.Center, Body.NPC.Center, 100, 4, Vector2.Zero);
            NPC.realLife = Body.NPC.whoAmI;
            NPC.active = Body.NPC.active;

            Cord.segments[0].position = NPC.Center;
            Cord.segments[^1].position = Body.NPC.Center;
            Cord.Update();
        }

        if (CurrentBehavior != voidVulture.Behavior.EjectCoreAndStalk || (Time > 500 && !PreparingToShoot))
        {
            foreach (var projectile in Main.ActiveProjectiles)
            {
                if (projectile.type == ModContent.ProjectileType<CoreBlast>())
                {
                    return;
                }
            }

            NPC.knockBackResist = LumUtils.InverseLerp(0, 60, NPC.ai[0]);
            var interpolant = LumUtils.InverseLerp(0, 60, NPC.ai[0]++);
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.4f);
            NPC.Center = Vector2.Lerp(NPC.Center, Body.NPC.Center, interpolant * 0.76f);

            if (NPC.Center.Distance(Body.NPC.Center) < 10f)
            {
                Body.CoreDeployed = false;
                NPC.active = false;
            }

            NPC.scale = LumUtils.InverseLerp(0, 100, NPC.Distance(Body.NPC.Center));
        }
        else
        {
            NPC.knockBackResist = 0.7f;

            if (Time == 41)
            {
                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Avatar.Chirp with
                        {
                            Volume = 4f
                        }
                    )
                    .WithVolumeBoost(2);
            }

            var thing = LumUtils.InverseLerp(200, 600, NPC.Distance(Body.currentTarget.Center));
            SuckNearbyPlayersGently(2000, 0.5f * thing);
            var trackStrength = LumUtils.InverseLerp(0, 500, NPC.Distance(Body.currentTarget.Center)) * 18;

            //Main.NewText(trackStrength);
            if (NPC.Center.Distance(Body.currentTarget.Center) < 200)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionFrom(Body.currentTarget.Center) * 12, 0.1f);
            }
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Body.currentTarget.Center) * trackStrength, 0.1f);
            }

            if (Time % 60 == 0)
            {
                PreparingToShoot = true;
            }

            if (PreparingToShoot)
            {
                TelegraphInterp = float.Lerp(TelegraphInterp, 1, 0.2f);
                NPC.ai[0]++;

                if (NPC.ai[0] == 20)
                {
                    var coreblastCount = !Body.HasSecondPhaseTriggered ? 3 : 4;

                    SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.DeadStarBurst with
                            {
                                Pitch = -0.5f,
                                PitchRange = (-2, 0),
                                PitchVariance = 0.8f
                            },
                            NPC.Center
                        )
                        .WithVolumeBoost(1.6f);

                    for (var i = 0; i < coreblastCount; i++)
                    {
                        var Vel = findShootVels(i, coreblastCount, NPC) * 4;
                        var a = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vel, ModContent.ProjectileType<CoreBlast>(), Body.NPC.defDamage / 3, 0);
                        a.As<CoreBlast>().OwnerIndex = NPC.whoAmI;
                        a.As<CoreBlast>().index = i;
                    }

                    NPC.ai[0] = 0;
                    PreparingToShoot = false;
                }
            }

            if (!PreparingToShoot)
            {
                NPC.rotation += MathHelper.ToRadians(1);
                TelegraphInterp = float.Lerp(TelegraphInterp, 0, 0.2f);
            }
        }
    }

    private void SuckNearbyPlayersGently(float radius = 900f, float pullStrength = 0.35f)
    {
        var center = NPC.Center;

        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];

            if (!p.active || p.dead)
            {
                continue;
            }

            var dist = Vector2.Distance(p.Center, center);

            if (dist > radius)
            {
                continue;
            }

            if (p.grappling[0] != -1)
            {
                continue;
            }

            var dir = (center - p.Center).SafeNormalize(Vector2.Zero);

            var closeness = Utils.GetLerpValue(radius, 0f, dist, true);

            p.velocity += dir * pullStrength * closeness;
            p.mount?.Dismount(p);
        }
    }

    public static Vector2 findShootVels(int i, int max, NPC npc)
    {
        return new Vector2(10, 0).RotatedBy(i / (float)max * MathHelper.TwoPi + npc.rotation) * npc.scale;
    }

    private void renderUmbilical()
    {
        if (NPC.IsABestiaryIconDummy || Cord == null)
        {
            return;
        }

        for (var i = 0; i < Cord.segments.Length - 1; i++)
        {
            var a = Color.White.MultiplyRGB(Color.Lerp(Color.White, Color.Transparent, i / (float)Cord.segments.Length));
            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            // Horizontal thickness (X) tapers from baseWidth to tipWidth
            var width = 0.5f;

            // Vertical stretch based on actual distance to next segment and texture height
            var segmentDistance = Cord.segments[i].position.Distance(Cord.segments[i + 1].position);
            var rot = Cord.segments[i].position.AngleTo(Cord.segments[i + 1].position);
            var lengthFactor = 1.4f;
            lengthFactor = segmentDistance / 1;

            var stretch = new Vector2(width, lengthFactor) * 1.6f;
            var DrawPos = Cord.segments[i].position - Main.screenPosition;

            Main.EntitySpriteDraw(debug, DrawPos, null, a * NPC.Opacity, rot + MathHelper.PiOver2, debug.Size() / 2, stretch, 0);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        renderUmbilical();

        if (PreparingToShoot || TelegraphInterp > 0)
        {
            var coreblastCount = !Body.HasSecondPhaseTriggered ? 3 : 4;

            for (var i = 0; i < coreblastCount; i++)
            {
                var Vel = findShootVels(i, coreblastCount, NPC) * 200 * TelegraphInterp;
                Utils.DrawLine(spriteBatch, NPC.Center, NPC.Center + Vel, Color.AntiqueWhite * TelegraphInterp, Color.Transparent, 4 * TelegraphInterp);
            }
        }

        Texture2D debug = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
        var thing = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 3f)) + 1.3f;

        Main.EntitySpriteDraw
        (
            debug,
            NPC.Center - screenPos,
            null,
            Color.AntiqueWhite with
            {
                A = 0
            },
            0,
            debug.Size() / 2,
            0.1f * NPC.scale * thing,
            0
        );

        Texture2D white = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        Texture2D outline = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
        var core = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/OtherworldlyCore_Anim").Value;
        var Offset = new Vector2(0, 0);

        var frame = core.Frame(1, 4, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 4);
        var DrawPos = NPC.Center - screenPos + Offset;

        var GlowFlip = Color.Lerp(Color.Blue, Color.WhiteSmoke, Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly))) * 0.1f * NPC.Opacity;

        Main.EntitySpriteDraw
        (
            outline,
            DrawPos,
            null,
            Color.White with
            {
                A = 0
            } *
            NPC.Opacity,
            0,
            outline.Size() / 2,
            0.1f,
            0
        );

        Main.EntitySpriteDraw(core, DrawPos, frame, Color.White * NPC.Opacity, 0, frame.Size() / 2, 1, 0);

        Main.EntitySpriteDraw
        (
            Glow,
            DrawPos,
            null,
            GlowFlip with
            {
                A = 0
            },
            0,
            Glow.Size() / 2,
            1,
            0
        );

        return false;
    }
}