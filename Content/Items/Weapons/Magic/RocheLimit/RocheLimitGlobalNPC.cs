using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitGlobalNPC : GlobalNPC
{
    private static int timeSinceLastRTAccess;

    private static Vector2? itemVelocityOverride;

    /// <summary>
    ///     Whether this NPC is currently being shredded by a black hole.
    /// </summary>
    public bool BeingShredded { get; set; }

    /// <summary>
    ///     The amount by which this NPC should be downscaled due to entering a black hole.
    /// </summary>
    public float DownscaleFactor { get; set; } = 1f;

    /// <summary>
    ///     The disintegration render target for NPCs.
    /// </summary>
    public static InstancedRequestableTarget DisintegrationTarget { get; set; }

    /// <summary>
    ///     The set of NPCs that are immune to being lobotomized by the Roche Limit.
    /// </summary>
    public static bool[] ImmuneToLobotomy { get; } = NPCID.Sets.Factory.CreateBoolSet(false);

    public override bool InstancePerEntity => true;

    public override void SetStaticDefaults()
    {
        Main.ContentThatNeedsRenderTargets.Add(DisintegrationTarget = new InstancedRequestableTarget());
        On_Main.DrawNPC += DecreaseTargetScale;
        On_Main.DrawNPCs += ApplyDisintegrationEffect;
        On_Item.NewItem_Inner += UseSpecialVelocity;

        ImmuneToLobotomy[ModContent.NPCType<AvatarRift>()] = true;
        ImmuneToLobotomy[ModContent.NPCType<AvatarOfEmptiness>()] = true;
        ImmuneToLobotomy[ModContent.NPCType<NamelessDeityBoss>()] = true;
    }

    private static int UseSpecialVelocity
    (
        On_Item.orig_NewItem_Inner orig,
        IEntitySource source,
        int X,
        int Y,
        int Width,
        int Height,
        Item itemToClone,
        int Type,
        int Stack,
        bool noBroadcast,
        int pfix,
        bool noGrabDelay,
        bool reverseLookup
    )
    {
        var index = orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noBroadcast, reverseLookup);

        if (index >= 0 && index < Main.maxItems && itemVelocityOverride is not null)
        {
            Main.item[index].velocity = itemVelocityOverride.Value.RotatedByRandom(0.1f);
        }

        return index;
    }

    private static void ApplyDisintegrationEffect(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        var blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        var targetIdentifier = behindTiles.ToInt();

        if (LumUtils.AnyProjectiles(blackHoleID))
        {
            // Not doing this causes one-frame visual bugs in which the old contents of the RT flicker on the screen.
            timeSinceLastRTAccess++;

            DisintegrationTarget.Request
            (
                Main.screenWidth,
                Main.screenHeight,
                targetIdentifier,
                () =>
                {
                    Main.spriteBatch.ResetToDefault(false);
                    orig(self, behindTiles);
                    Main.spriteBatch.End();
                }
            );

            if (RocheLimitBlackHoleRenderer.blackHoleTarget.TryGetTarget(0, out var blackHoleTarget) &&
                blackHoleTarget is not null &&
                DisintegrationTarget.TryGetTarget(targetIdentifier, out var npcTarget) &&
                npcTarget is not null &&
                timeSinceLastRTAccess >= 3)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

                var aspectRatioCorrectionFactor = new Vector2(WotGUtils.ViewportSize.X / WotGUtils.ViewportSize.Y, 1f);
                RocheLimitBlackHoleRenderer.GetBlackHoleData(aspectRatioCorrectionFactor, out var blackHoleRadii, out var blackHolePositions);

                var spaghettificationShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSpaghettificationShader");
                spaghettificationShader.TrySetParameter("sourceRadii", blackHoleRadii);
                spaghettificationShader.TrySetParameter("sourcePositions", blackHolePositions);
                spaghettificationShader.TrySetParameter("aspectRatioCorrectionFactor", aspectRatioCorrectionFactor);
                spaghettificationShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
                spaghettificationShader.TrySetParameter("burnColor", new Vector3(2.4f, 1.13f, 0.04f));
                spaghettificationShader.SetTexture(blackHoleTarget, 1);
                spaghettificationShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2, SamplerState.LinearWrap);
                spaghettificationShader.Apply();

                Main.spriteBatch.Draw(npcTarget, Main.screenLastPosition - Main.screenPosition, Color.White);

                Main.spriteBatch.ResetToDefault();
            }
            else
            {
                orig(self, behindTiles);
            }

            return;
        }

        timeSinceLastRTAccess = 0;

        orig(self, behindTiles);
    }

    private static void DecreaseTargetScale(On_Main.orig_DrawNPC orig, Main self, int index, bool behindTiles)
    {
        var npc = Main.npc[index];

        if (!npc.active || !npc.TryGetGlobalNPC(out RocheLimitGlobalNPC globalNPC))
        {
            return;
        }

        var originalScale = npc.scale;

        try
        {
            npc.scale *= globalNPC.DownscaleFactor;
            orig(self, index, behindTiles);
        }
        finally
        {
            if (npc is not null)
            {
                npc.scale = originalScale;
            }
        }
    }

    // Lobotomize targets that are preoccupied with being fucking shredded to pieces by a black hole.
    // This doesn't apply to deities.
    public override bool PreAI(NPC npc)
    {
        return !BeingShredded || ImmuneToLobotomy[npc.type];
    }

    public override void PostAI(NPC npc)
    {
        var wasShreddedBefore = BeingShredded;
        BeingShredded = false;
        DownscaleFactor = MathHelper.Lerp(DownscaleFactor, 1f, 0.12f);

        if (!npc.CanBeChasedBy() && !wasShreddedBefore)
        {
            return;
        }

        var minDistance = 9999999f;
        var blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        var blackHoles = LumUtils.AllProjectilesByID(blackHoleID);
        Projectile? closestBlackHole = null;

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == blackHoleID)
            {
                var distanceToBlackHole = projectile.Distance(npc.Center);

                if (distanceToBlackHole < minDistance)
                {
                    closestBlackHole = projectile;
                    minDistance = distanceToBlackHole;
                }
            }
        }

        if (closestBlackHole is not null && npc.WithinRange(closestBlackHole.Center, 900f))
        {
            var suctionOrigin = closestBlackHole.Center;

            var suctionInterpolant = closestBlackHole.As<RocheLimitBlackHole>().BlackHoleDiameter / RocheLimitBlackHole.MaxBlackHoleDiameter;
            var suctionAcceleration = suctionInterpolant * 0.09f;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.SafeDirectionTo(suctionOrigin) * suctionInterpolant * 80f, suctionAcceleration);

            if (npc.realLife == -1 && !ImmuneToLobotomy[npc.type])
            {
                var idealDownscaling = EasingCurves.Exp.Evaluate(EasingType.Out, LumUtils.InverseLerp(150f, 700f, npc.Distance(suctionOrigin)));
                DownscaleFactor = MathHelper.Lerp(1f, idealDownscaling, suctionInterpolant);
            }

            // It's time to die.
            var shredDistance = closestBlackHole.As<RocheLimitBlackHole>().BlackHoleDiameter * 0.33f;

            if (npc.WithinRange(suctionOrigin, shredDistance) && suctionInterpolant >= 0.85f)
            {
                BeingShredded = true;

                if (!ImmuneToLobotomy[npc.type])
                {
                    npc.velocity = Vector2.Zero;
                    npc.Center = suctionOrigin;
                }
            }

            // Hits are inputted manually to ensure maximum control over the NPC's death, which needs to be more interesting than just splaying a bunch of gore and loot.
            if (closestBlackHole.Colliding(closestBlackHole.Hitbox, npc.Hitbox) || BeingShredded)
            {
                var damage = closestBlackHole.damage;

                var willDie = npc.life - damage <= 0; // This calculation doesn't care about defense and DR but honestly who cares? filthy liar,then why do they have DR still

                if (willDie)
                {
                    npc.active = false;

                    var fallbackJetDirection = Main.rand.NextVector2Unit().RotateTowards(closestBlackHole.AngleTo(Main.player[closestBlackHole.owner].Center), MathHelper.Pi * 0.4f);
                    var jetDirection = npc.velocity.SafeNormalize(fallbackJetDirection);

                    try
                    {
                        itemVelocityOverride = jetDirection * 65f;
                        npc.NPCLoot();
                    }
                    finally
                    {
                        itemVelocityOverride = null;
                    }

                    closestBlackHole.As<RocheLimitBlackHole>().ReleaseJet(jetDirection);
                }
                else
                {
                    var oldHitSound = npc.HitSound;
                    var oldDeathSound = npc.DeathSound;

                    try
                    {
                        npc.HitSound = null;
                        npc.DeathSound = null;
                        Main.player[closestBlackHole.owner].addDPS(npc.SimpleStrikeNPC(damage, 0));

                        #region kill echdeath

                        var echdeathMod = ModLoader.TryGetMod("ReturnOfEchdeeath", out var mod) ? mod : null;
                        var echdeathType = -1;

                        if (echdeathMod != null)
                        {
                            // Try to get the NPC type "echdeath" from the mod
                            if (echdeathMod.TryFind("Echdeath", out ModNPC echdeathNPC))
                            {
                                echdeathType = echdeathNPC.Type;
                            }
                        }

                        if (BeingShredded && echdeathType != -1 && npc.type == echdeathType)
                        {
                            npc.defense = 0;
                            npc.life -= (int)(npc.lifeMax * 0.1f);
                            npc.takenDamageMultiplier = 5f;

                            if (Main.netMode != NetmodeID.Server)
                            {
                                SoundEngine.PlaySound
                                (
                                    SoundID.Roar with
                                    {
                                        Volume = 1.0f,
                                        PitchVariance = 0.4f
                                    },
                                    npc.Center
                                );
                            }
                        }

                        #endregion
                    }
                    finally
                    {
                        npc.HitSound = oldHitSound;
                        npc.DeathSound = oldDeathSound;
                    }
                }
            }
        }
    }
}