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
        Player player = Main.player[projectile.owner];
        modifiers.Knockback *= LumUtils.InverseLerp(1000, 0, player.Distance(NPC.Center));
        //Main.NewText(modifiers.Knockback.Multiplicative);


    }
    Vector2 EndFiringPos;
    float ReturnOffsetY = 30f;
    float HoverDistance = 200f;
    float MaxTrackSpeed = 18f;
    int TelegraphTime = 20;
    int ShootInterval = 60;

    float RotationVelocity;
    float RotationDamping = 0.92f;

    enum CoreState
    {
        Inactive,
        Deployed,
        Attacking,
        Returning
    }

    CoreState State = CoreState.Inactive;
    int StateTimer;
    bool PreparingToShoot;
    float TelegraphInterp;
    public voidVulture.Behavior CurrentBehavior =>
          Body != null ? Body.currentState : default;

    public int BodyTime =>
        Body != null ? Body.Time : 0;


    public override bool CheckActive()
    {
        //STOP IT
        return true;
    }
    public override void PostAI()
    {

        NPC.DiscourageDespawn(int.MaxValue);
    }
    public override void AI()
    {
        if (Body == null || !Body.NPC.active)
        {
            NPC.active = false;
            return;
        }

        UpdateCord();

        switch (State)
        {
            case CoreState.Inactive:
                EnterDeployed();
                break;

            case CoreState.Deployed:
                UpdateDeployed();
                break;

            case CoreState.Attacking:
                UpdateAttacking();
                break;

            case CoreState.Returning:
                UpdateReturning();
                break;
        }

        StateTimer++;
    }

    void EnterDeployed()
    {
        State = CoreState.Deployed;
        StateTimer = 0;
        NPC.velocity = Vector2.Zero;
    }

    void EnterAttacking()
    {
        State = CoreState.Attacking;
        StateTimer = 0;
        PreparingToShoot = false;
        TelegraphInterp = 0f;
    }

    void EnterReturning()
    {
        EndFiringPos = NPC.Center;
        State = CoreState.Returning;
        StateTimer = 0;
        PreparingToShoot = false;
    }

    void UpdateDeployed()
    {
        if (CurrentBehavior == voidVulture.Behavior.EjectCoreAndStalk)
            EnterAttacking();
    }

    void UpdateAttacking()
    {
        Player target = Body.currentTarget as Player;
        if (target == null || !target.active)
            return;

        NPC.knockBackResist = 0.7f;

        // Gentle pull
        float pullScale = LumUtils.InverseLerp(200, 600, NPC.Distance(target.Center));
        SuckNearbyPlayersGently(2000f, 0.5f * pullScale);

        // Tracking movement
        Vector2 desiredVel;
        float dist = NPC.Distance(target.Center);

        if (dist < HoverDistance)
            desiredVel = NPC.DirectionFrom(target.Center) * 12f;
        else
            desiredVel = NPC.DirectionTo(target.Center) * MaxTrackSpeed;

        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel, 0.1f);

        // Telegraph / shoot cycle
        if (StateTimer % ShootInterval == 0)
            PreparingToShoot = true;

        if (PreparingToShoot)
            HandleShooting();

        if (CurrentBehavior != voidVulture.Behavior.EjectCoreAndStalk && !PreparingToShoot)
            EnterReturning();

        NPC.rotation += RotationVelocity;
        RotationVelocity *= RotationDamping;
    }

    void UpdateReturning()
    {
        Vector2 returnPos = Body.NPC.Center + new Vector2(0f, ReturnOffsetY);

        NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.4f);
        float thing = 1 - LumUtils.InverseLerp(0, 4000, NPC.Distance(Body.NPC.Center));

        NPC.Center = Vector2.Lerp(EndFiringPos, returnPos, thing);
        NPC.knockBackResist = 0;

        if (NPC.Distance(returnPos) < 10f)
        {
            Body.CoreDeployed = false;
            NPC.active = false;
        }
    }

    void HandleShooting()
    {
        TelegraphInterp = float.Lerp(TelegraphInterp, 1f, 0.2f);

        if (StateTimer % ShootInterval == TelegraphTime)
        {
            FireCoreBlasts();
            PreparingToShoot = false;
            TelegraphInterp = 0f;
        }
    }

    void FireCoreBlasts()
    {
        int count = Body.HasSecondPhaseTriggered ? 4 : 3;

        // ⬇ ROTATION IMPULSE
        RotationVelocity += Main.rand.NextFloat(-0.12f, 0.12f);

        SoundEngine.PlaySound(
            GennedAssets.Sounds.Avatar.DeadStarBurst
            with
            { Pitch = -1.5f, PitchVariance = 0.8f },
            NPC.Center).WithVolumeBoost(1.6f);

        for (int i = 0; i < count; i++)
        {
            Vector2 vel = FindShootVelocity(i, count, NPC) * 4f;
            Projectile p = Projectile.NewProjectileDirect(
                NPC.GetSource_FromThis(),
                NPC.Center,
                vel,
                ModContent.ProjectileType<CoreBlast>(),
                (int)(Body.NPC.defDamage / 3.5f),
                0f);

            p.As<CoreBlast>().OwnerIndex = NPC.whoAmI;
            p.As<CoreBlast>().index = i;
        }
    }

    void SuckNearbyPlayersGently(float radius = 900f, float pullStrength = 0.35f)
    {
        Vector2 center = NPC.Center;

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
    public static Vector2 FindShootVelocity(int i, int max, NPC npc)
    {
        return new Vector2(10f, 0f)
            .RotatedBy(i / (float)max * MathHelper.TwoPi + npc.rotation)
            * npc.scale;
    }
    void UpdateCord()
    {
        Cord ??= new Rope(NPC.Center, Body.NPC.Center, 100, 4, Vector2.Zero);

        Cord.segments[0].position = NPC.Center;
        Cord.segments[^1].position = Body.NPC.Center;
        Cord.Update();

        NPC.realLife = Body.NPC.whoAmI;
    }
    void renderUmbilical()
    {
        if (NPC.IsABestiaryIconDummy || Cord == null)
            return;
        for (int i = 0; i < Cord.segments.Length - 1; i++)
        {
            Color a = Color.White.MultiplyRGB(Color.Lerp(Color.White, Color.Transparent, i / (float)Cord.segments.Length));
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


            for (int i = 0; i < coreblastCount; i++)
            {
                Vector2 Vel = FindShootVelocity(i, coreblastCount, NPC) * 200 * TelegraphInterp;
                Utils.DrawLine(spriteBatch, NPC.Center, NPC.Center + Vel, Color.AntiqueWhite * TelegraphInterp, Color.Transparent, 4 * TelegraphInterp);
            }
        }
        Texture2D debug = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
        float thing = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 3f)) + 1.3f;
        Main.EntitySpriteDraw(debug, NPC.Center - screenPos, null, Color.AntiqueWhite with { A = 0 }, 0, debug.Size() / 2, 0.1f * NPC.scale * thing, 0);

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