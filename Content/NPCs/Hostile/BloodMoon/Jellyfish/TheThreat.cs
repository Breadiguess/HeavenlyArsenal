using CalamityMod;
using HeavenlyArsenal.Core.Systems;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using PrimitiveRenderer = CalamityMod.Graphics.Primitives.PrimitiveRenderer;
using PrimitiveSettings = CalamityMod.Graphics.Primitives.PrimitiveSettings;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;

public class TheThreat : ModProjectile
{
    public enum Behavior
    {
        Orbit = 0,

        Concussive = 1
    }

    public Entity Target;

    private int DistanceOffset;

    private int SpiralOffset;

    private float intensity;

    /// <summary>
    ///     the NPC.WhoAmI of the jellyfish that owns this threat ball.
    /// </summary>
    public int ownerIndex
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int varX
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public int varY
    {
        get => (int)Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public Behavior CurrentState
    {
        get => (Behavior)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    public float placeholder => Time / 10.1f * intensity + SpiralOffset;

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(16, 16);
        Projectile.damage = 30;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 180;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 3;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
        SpiralOffset = Main.rand.Next(20, 100);

        DistanceOffset = Main.rand.Next(75, 140);
        intensity = 1;
        varX = Main.rand.Next(0, 3);
        varY = Main.rand.Next(0, 3);
    }

    public override void AI()
    {
        if (Projectile.scale < 0.99)
        {
            Projectile.scale = float.Lerp(Projectile.scale, 1, 0.2f);
        }

        if (ownerIndex != -1)
        {
            var Owner = Main.npc[ownerIndex];
            Projectile.timeLeft++;

            if (CurrentState == Behavior.Orbit)
            {
                Projectile.rotation = (Projectile.Center - Projectile.oldPosition).ToRotation();
                Projectile.Center = Owner.Center + new Vector2(MathF.Sin(placeholder), MathF.Cos(placeholder)) * DistanceOffset;

                if (!Owner.active)
                {
                    Projectile.Kill();
                }
            }
        }

        if (CurrentState == Behavior.Concussive)
        {
            ownerIndex = -1;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Target != null)
            {
                if (Time < 10)
                {
                    Projectile.velocity = Target.Center.AngleFrom(Projectile.Center).ToRotationVector2();
                }
                else
                {
                    Projectile.velocity *= 1.12f;
                }
            }

            // Predict new position for this frame
            var nextPos = Projectile.Center + Projectile.velocity;

            // Perform a raytrace from old position to new position
            var hit = LineAlgorithm.RaycastTo
            (
                (int)(Projectile.oldPosition.X / 16f),
                (int)(Projectile.oldPosition.Y / 16f),
                (int)(nextPos.X / 16f),
                (int)(nextPos.Y / 16f)
            );

            if (hit.HasValue && Projectile.velocity.Length() > 0)
            {
                // Convert back to world coordinates
                var hitWorld = hit.Value.ToVector2() * 16f;

                // Handle impact immediately
                OnRayImpact(hitWorld);

                return; // Stop further logic after impact
            }

            if (Projectile.velocity.Length() <= 0.02f)
            {
                Projectile.Opacity = float.Lerp(Projectile.Opacity, 0, 0.2f);
            }
        }

        Time++;
    }

    private void OnRayImpact(Vector2 hitWorld)
    {
        var SpawnCount = (int)Utils.Remap(Projectile.velocity.Length(), 0, 300, 0, 40);
        //Main.NewText("SpawnCount: " + SpawnCount + ", velocity: " + Projectile.velocity.Length());

        Projectile.Center = hitWorld;

        for (var i = 0; i < SpawnCount; i++)
        {
            Collision.HitTiles(hitWorld, Projectile.velocity, Projectile.width, Projectile.height);

            var d = Dust.NewDustDirect
            (
                hitWorld - new Vector2(8, 8),
                16,
                16,
                DustID.Dirt,
                Main.rand.NextFloat(-3f, 3f),
                Main.rand.NextFloat(-3f, -1f)
            );

            d.scale = Main.rand.NextFloat(1f, 1.8f);
            d.noGravity = false;
        }

        foreach (var player in Main.ActivePlayers)
        {
            if (!player.active)
            {
                continue;
            }

            var distance = Vector2.Distance(player.Center, Projectile.Center);

            // Define min and max range for screenshake effect
            var maxRange = 700f; // beyond this distance, no shake
            var minRange = 150f; // within this distance, maximum shake

            if (distance < maxRange)
            {
                // Normalize strength between 0 (at maxRange) and 1 (at minRange)
                var strength = 1f - MathHelper.Clamp((distance - minRange) / (maxRange - minRange), 0f, 1f);

                strength = MathF.Pow(strength, 2f); // smoother falloff

                var shakeMagnitude = MathHelper.Lerp(1f, 10f, strength);

                if (player.whoAmI == Main.myPlayer)
                {
                    ScreenShakeSystem.StartShakeAtPoint
                    (
                        Projectile.Center,
                        7f * strength,
                        shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2,
                        shakeStrengthDissipationIncrement: 0.7f - strength * 0.1f
                    );
                }
            }
        }

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Mars.MissileExplode with
            {
                PitchVariance = 1,
                Pitch = 0f,
                MaxInstances = 16
            },
            hitWorld
        );

        Projectile.velocity = Vector2.Zero;
        Projectile.damage = 0;
        Projectile.tileCollide = false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (CurrentState == Behavior.Concussive)
        {
            var start = Projectile.Center.ToWorldCoordinates();
            var end = Projectile.Center + oldVelocity / 2;

            // Projectile.Center = LineAlgorithm.RaycastTo((int)start.X, (int)start.Y, (int)end.X, (int)end.Y).GetValueOrDefault().ToVector2();
            Projectile.Center = end;
            Projectile.damage = 0;
            Projectile.velocity *= 0;
            var SpawnPos = Collision.TileCollision(Projectile.Center, oldVelocity, 30, 20).ToWorldCoordinates();
            Dust impact;

            for (var i = 0; i < 40; i++)
            {
                impact = Dust.NewDustDirect(SpawnPos, 10, 10, DustID.Dirt);
            }
        }

        return false;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        var projName = Lang.GetProjectileName(Projectile.type).Value;
        var val = Main.rand.Next(0, 3);
        var text = NetworkText.FromKey($"Mods.{Mod.Name}.PlayerDeathMessages.TheThreat{val}", target.name, projName);

        modifiers = new Player.HurtModifiers
        {
            DamageSource = PlayerDeathReason.ByCustomReason(text)
        };
    }
    private Color TrailColor(float trailLengthInterpolant, Vector2 vertexPosition)
    {
        var t = MathHelper.Clamp(trailLengthInterpolant, 0f, 1f);
        var crimson = new Color(220, 20, 76);
        var brightness = MathHelper.SmoothStep(1f, 0.6f, t);

        // Interpolate between transparent and crimson
        var baseColor = Color.Lerp(Color.Transparent, crimson, 1f - t);

        var finalColor = baseColor * brightness * Projectile.Opacity;
        finalColor.A = (byte)MathHelper.Clamp(finalColor.A, 0, 255);

        return finalColor;
    }
    private float TrailWidth(float trailLengthInterpolant, Vector2 vertexPosition)
    {
        var widthInterpolant = Utils.GetLerpValue(0f, 0.25f, trailLengthInterpolant, true) * Utils.GetLerpValue(1.1f, 0.7f, trailLengthInterpolant, true);

        return MathHelper.SmoothStep(8f, 20f, widthInterpolant);
    }
  

    public override bool PreDraw(ref Color lightColor)
    {
        if (ownerIndex != -1)
        {
            var Owner = Main.npc[ownerIndex];

            if (Owner.active && Owner != null)
            {
                //Utils.DrawLine(Main.spriteBatch, Projectile.Center, Owner.Center, Color.AntiqueWhite, Color.Transparent,  2);
            }
        }

        lightColor *= Projectile.Opacity;
        var glow = AssetDirectory.Textures.BigGlowball.Value;
        var DrawPos = Projectile.Center - Main.screenPosition;

        var tex = ModContent.Request<Texture2D>(Texture).Value;
        Main.spriteBatch.EnterShaderRegion();
        //yes, i'm using the art attack shader. so sue me,
        GameShaders.Misc["CalamityMod:ArtAttack"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/DoubleTrail"));
        GameShaders.Misc["CalamityMod:ArtAttack"].Apply();

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(TrailWidth, TrailColor, shader: GameShaders.Misc["CalamityMod:ArtAttack"]), 180);
        Main.spriteBatch.ExitShaderRegion();
        var squishFactor = Utils.Remap(Projectile.velocity.Length(), 0, 40, 1, 0.5f);

        if (CurrentState == Behavior.Orbit)
        {
            squishFactor = 1;
        }

        var Frame = tex.Frame(3, 3, varX, varY);
        var Squish = new Vector2(1, 1 * squishFactor);

        Main.EntitySpriteDraw
        (
            glow,
            DrawPos,
            null,
            Color.Crimson with
            {
                A = 0
            } *
            squishFactor *
            Projectile.Opacity,
            0,
            glow.Size() * 0.5f,
            0.05f * Projectile.scale,
            0
        );

        Main.EntitySpriteDraw(tex, DrawPos, Frame, lightColor, Projectile.rotation, Frame.Size() / 2, Squish * Projectile.scale, 0);

        //Utils.DrawBorderString(Main.spriteBatch, Projectile.velocity.Length().ToString(), DrawPos, Color.AntiqueWhite);
        return false; // base.PreDraw(ref lightColor);
    }

    
}