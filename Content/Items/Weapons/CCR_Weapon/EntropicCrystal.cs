using CalamityMod.Particles;
using HeavenlyArsenal.Content.Particles.Metaballs.NoxusGasMetaball;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

internal class EntropicCrystal : ModProjectile
{
    public override void AI()
    {
        HandleState();

        Time++;
    }

    private void HandleState()
    {
        switch (CurrentState)
        {
            case EntropicCrystalState.PreHit:
                HandlePreHit();

                break;
            case EntropicCrystalState.PostHit:
                HandlePostHit();

                break;
            case EntropicCrystalState.Exploding:
                HandlePostHit();
                HandleExplosion();

                break;
            case EntropicCrystalState.DisipateHarmlessly:
                HandleDisipateHarmlessly();

                break;
        }
    }

    /// <summary>
    ///     this is the crystal before hitting anything. its just falling from the sky right now, and isn't
    ///     doing all too much
    /// </summary>
    private void HandlePreHit()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        // Integrate HomeResource to prevent excessive homing by limiting the total amount of "turning" the projectile can do
        var homingRange = 400f;
        var homingStrength = 0.12f;
        NPC closestNPC = null;
        var closestDist = homingRange;

        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var npc = Main.npc[i];

            if (npc.CanBeChasedBy(Projectile))
            {
                var dist = Vector2.Distance(Projectile.Center, npc.Center);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNPC = npc;
                }
            }
        }

        if (closestNPC != null && HomeResource > 0f)
        {
            var desiredVelocity = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
            // Calculate the angle between current and desired velocity
            var angleDiff = Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.Zero), desiredVelocity.SafeNormalize(Vector2.Zero));
            angleDiff = Math.Clamp(angleDiff, -1f, 1f);
            var turnAmount = (float)Math.Acos(angleDiff);
            // Reduce HomeResource by the amount of turning done
            var turnCost = turnAmount * 20f; // Arbitrary scaling factor for resource usage

            if (HomeResource >= turnCost)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
                HomeResource -= turnCost;
            }
            else
            {
                // Not enough resource to turn fully, so only turn partially
                var partialStrength = HomeResource / turnCost * homingStrength;
                //Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, partialStrength);
                HomeResource = 0f;
            }
        }
    }

    /// <summary>
    ///     this is the crystal after hitting something. it embeds itself in the target, and deals DOT
    ///     while waiting for an explosion call.
    /// </summary>
    private void HandlePostHit()
    {
        Projectile.timeLeft = 4;
        // Stick to the hit NPC
        Projectile.velocity = Vector2.Zero;

        if (Projectile.ai[2] == 0)
        {
            if (StuckNPC != null)
            {
                Projectile.ai[2] = StuckNPC.whoAmI + 1;
            }

            Projectile.netUpdate = true;
        }

        var npcIndex = (int)Projectile.ai[2] - 1;

        if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
        {
            var stuckNpc = Main.npc[npcIndex];

            if (stuckNpc.active && !stuckNpc.dontTakeDamage && !stuckNpc.friendly)
            {
                Projectile.Center = StuckNPC.Center + StuckOffset;
            }
            else
            {
                // If NPC is dead or invalid, dissipate harmlessly
                Projectile.ai[1] = (float)EntropicCrystalState.DisipateHarmlessly;
                Projectile.netUpdate = true;
            }
        }

        if (Toxic)
        {
            if (Time == 60 * 3 - 1)
            {
                for (var i = 0; i < 5; i++)
                {
                    var voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

                    HeavySmokeParticle darkGas = new
                    (
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextVector2Circular(1f, 1f),
                        voidColor,
                        11,
                        Projectile.scale * 1.24f,
                        Projectile.Opacity * 0.6f,
                        Main.rand.NextFloat(0.02f),
                        true
                    );

                    GeneralParticleHandler.SpawnParticle(darkGas);
                }
            }

            if (Time > 60 * 3)
            {
                Time = 0;

                if (Main.rand.NextBool(1))
                {
                    var gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                    var angularOffset = -Projectile.rotation;

                    NoxusGasMetaball.CreateParticle
                        (Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
                }

                Main.npc[npcIndex].SimpleStrikeNPC(400, 0);
            }
        }
    }

    /// <summary>
    ///     ended up splitting this into two projectiles. this just creates the portal.
    /// </summary>
    private void HandleExplosion()
    {
        var npc = StuckNPC as NPC;

        if (npc == null)
        {
            return;
        }

        CrystalStorageNPC crystalNPC;
        npc.TryGetGlobalNPC(out crystalNPC);

        if (crystalNPC == null)
        {
            return;
        }

        var index = crystalNPC.storedProjectiles.IndexOf(Projectile);

        if (index < 0)
        {
            return;
        }

        // how many points per ring
        var pointsPerRing = 6;

        // find ring + local position
        var ring = index / pointsPerRing;
        var pos = index % pointsPerRing;

        // rotation logic
        var baseRotation = 0f; // rotate whole formation
        var ringOffset = 0.5f; // stagger rings
        var angle = MathHelper.TwoPi * pos / pointsPerRing + baseRotation + ring * ringOffset;

        // radius logic
        var baseRadius = 100f;
        var radius = baseRadius * (1f + ring * 0.35f);

        // final spawn offset
        var offset = angle.ToRotationVector2() * radius;
        var spawnPos = npc.Center + offset;

        // only spawn once
        var d = Dust.NewDustPerfect(spawnPos, DustID.Cloud);
        d.velocity = Vector2.Zero;
        d.color = Color.AntiqueWhite;

        if (Projectile.localAI[0] != 1f)
        {
            Projectile.localAI[0] = 1f;

            var portal = Projectile.NewProjectile
            (
                Projectile.GetSource_FromThis(),
                spawnPos,
                Vector2.Zero,
                ModContent.ProjectileType<EntropicBlast>(),
                (int)(Owner.HeldItem.damage * 3f),
                0,
                Projectile.owner
            );
        }
    }

    /// <summary>
    /// </summary>
    private void HandleDisipateHarmlessly()
    {
        for (var i = 0; i < 2; i++)
        {
            var voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
            voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

            HeavySmokeParticle darkGas = new
            (
                Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                Main.rand.NextVector2Circular(1f, 1f),
                voidColor,
                11,
                Projectile.scale * 1.24f,
                Projectile.Opacity * 0.6f,
                Main.rand.NextFloat(0.02f),
                true
            );

            GeneralParticleHandler.SpawnParticle(darkGas);
        }

        Projectile.alpha--;

        if (Projectile.alpha >= 0)
        {
            Projectile.Kill();
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.GetGlobalNPC<CrystalStorageNPC>().AttachtoNPC(target, Projectile);

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Common.TwinkleMuffled with
            {
                Pitch = 0.3f,
                PitchVariance = 0.4f
            }
        );

        Projectile.ai[1] = (float)EntropicCrystalState.PostHit;
        Projectile.timeLeft = 180;
        StuckNPC = target;
        StuckOffset = Projectile.Center - target.Center;
        Toxic = true;

        if (!Main.rand.NextBool(ShatterChance))
        {
            target.GetGlobalNPC<NoxusWeaponNPC>().AttachedCrystalCount++;
        }
        else
        {
            Projectile.active = false;
        }

        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var FrameCount = 4;

        var texture = ModContent.Request<Texture2D>(Texture).Value;

        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var drawPos = Projectile.Center - Main.screenPosition;
        var ScaleEffects = new Vector2(0.9f, 1);

        var Frame = new Rectangle(0, (int)CrystalFrame * texture.Height / FrameCount, texture.Width, texture.Height / FrameCount);
        var origin = new Vector2(texture.Width / 2f, texture.Height / FrameCount / 1.3f);

        var wind = AperiodicSin(Main.GlobalTimeWrappedHourly * 0.1f + Projectile.Center.X + Projectile.Center.Y) * 0.333f;

        if (Toxic)
        {
            var thing = MathHelper.SmoothStep(0.8f, 1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.1f));
            ScaleEffects = new Vector2(thing, 1);
        }
        else
        {
            ScaleEffects = new Vector2(0.9f, 1);
            wind = 0f;
        }

        Main.EntitySpriteDraw(texture, drawPos, Frame, lightColor, Projectile.rotation + wind, origin, ScaleEffects, effects);

        //attempt blur  

        if (Projectile.ai[1] == (float)EntropicCrystalState.PreHit)
        {
            //draw several times faded or have a shader that does the same thing
            for (var i = 0; i < 40; i++)
            {
                var blurOffset = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * (i * 2 - 10);
                var fadedColor = lightColor * (1f - i * 0.2f);
                Main.EntitySpriteDraw(texture, drawPos + blurOffset, Frame, fadedColor, Projectile.rotation + wind, origin, ScaleEffects, effects);
                //Utils.DrawBorderString(Main.spriteBatch, blurOffset.ToString(), drawPos + blurOffset - Vector2.UnitY * 10*i, Color.AntiqueWhite);
            }
        }

        //Utils.DrawBorderString(Main.spriteBatch, CrystalFrame.ToString(), drawPos - Vector2.UnitY * 100, Color.AntiqueWhite);
        return false;
    }

    #region setup

    public float ShatterChance;

    public float HomeResource;

    public Entity StuckNPC { get; set; }

    public Vector2 StuckOffset;

    public enum EntropicCrystalState
    {
        PreHit,

        PostHit,

        Exploding,

        DisipateHarmlessly
        //Shatter
    }

    public bool Toxic;

    public ref Player Owner => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    //store in ai[1] to avoid bugs
    public EntropicCrystalState CurrentState => (EntropicCrystalState)Projectile.ai[1];

    //just for fun
    public ref float StoredNPC => ref Projectile.ai[2];

    public ref float CrystalFrame => ref Projectile.localAI[1];

    public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/EntropicCrystal";

    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.width = Projectile.height = 30;

        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = 180;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void SetStaticDefaults() { }

    public override void OnSpawn(IEntitySource source)
    {
        HomeResource = 60;
        CrystalFrame = Main.rand.Next(0, 4);
        ShatterChance = (float)Math.Round(Main.rand.NextFloat(0, 1.000001f), 4);
        var gasSize = Utils.GetLerpValue(-3f, 25f, 10, true) * Projectile.width * 0.68f;
        var angularOffset = (float)Math.Sin(Time / 5f) * 0.77f;

        NoxusGasMetaball.CreateParticle
            (Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
    }

    #endregion
}