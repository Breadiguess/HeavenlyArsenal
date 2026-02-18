using System.Collections.Generic;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria.Audio;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.AvatarSpear;

public class LonginusRift : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];

    public ref float OpenScale => ref Projectile.localAI[0];

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 80;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
    }

    public override void AI()
    {
        Projectile.velocity *= 0.94f;
        Projectile.velocity = Projectile.velocity.RotatedBy(-0.05f * Projectile.direction);
        Projectile.rotation = Projectile.velocity.X * 0.08f;

        OpenScale = MathHelper.Lerp(OpenScale, 1f, 0.1f);

        if (Main.rand.NextBool(10 + Projectile.timeLeft))
        {
            var particle = HeatLightning.pool.RequestParticle();

            particle.Prepare
            (
                Projectile.Center + Main.rand.NextVector2Circular(20, 20) * Projectile.scale,
                Main.rand.NextVector2Circular(5, 5),
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.Next(5, 20),
                Main.rand.NextFloat(0.3f, 1.5f) * OpenScale
            );

            ParticleEngine.Particles.Add(particle);
        }
    }

    public override void OnKill(int timeLeft)
    {
        var metaball = ModContent.GetInstance<BloodMetaball>();

        for (var i = 0; i < 30; i++)
        {
            var bloodSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(5, 5) * Projectile.scale;
            var bloodVelocity = Main.rand.NextVector2Circular(12f, 12f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(30f, 40f), Main.rand.NextFloat());

            if (i % 18 == 0)
            {
                var particle = HeatLightning.pool.RequestParticle();

                particle.Prepare
                (
                    Projectile.Center + Main.rand.NextVector2Circular(10, 10) * Projectile.scale,
                    Main.rand.NextVector2Circular(15, 15),
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.Next(5, 20),
                    Main.rand.NextFloat(0.3f, 1.5f) * OpenScale
                );

                ParticleEngine.Particles.Add(particle);
            }
        }

        //BleedingBurstParticle bombParticle = BleedingBurstParticle.pool.RequestParticle();
        //Color randomColor = Color.Lerp(Color.Black, Color.DarkRed, Main.rand.NextFloat());
        //bombParticle.Prepare(Projectile.Center + Main.rand.NextVector2Circular(35, 35), Main.rand.NextVector2Circular(4, 4), Main.rand.NextFloat(-1f, 1f), randomColor, Main.rand.NextFloat(0.5f, 1f));
        //ParticleEngine.ShaderParticles.Add(bombParticle);

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Avatar.PortalHandReach with
            {
                MaxInstances = 0,
                Volume = 0.5f,
                Pitch = 0.7f,
                PitchVariance = 0.3f
            },
            Projectile.Center
        );

        NPC targetNPC = null;

        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.CanBeChasedBy(this) && npc.Distance(Projectile.Center) < 1000f)
            {
                targetNPC = npc;

                break;
            }
        }

        if (targetNPC != null)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                ScreenShakeSystem.StartShakeAtPoint
                (
                    Projectile.Center,
                    2f,
                    shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2,
                    shakeStrengthDissipationIncrement: 0.2f
                );
            }

            var velocity = Main.rand.NextVector2Circular(20, 20);

            var spear = Projectile.NewProjectileDirect
                (Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AntishadowLonginus>(), Projectile.damage, 1f, Projectile.owner);

            spear.ai[1] = targetNPC.whoAmI + 1;
            spear.scale *= Main.rand.NextFloat(0.9f, 1.3f);
        }
    }

    public override bool? CanCutTiles()
    {
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var glow = AssetDirectory.Textures.BigGlowball.Value;
        var flare = TextureAssets.Extra[ExtrasID.SharpTears].Value;

        Main.EntitySpriteDraw
        (
            glow,
            Projectile.Center - Main.screenPosition,
            null,
            Color.DarkRed with
            {
                A = 0
            },
            0,
            glow.Size() / 2,
            0.15f * Projectile.scale,
            0
        );

        Main.EntitySpriteDraw
        (
            flare,
            Projectile.Center - Main.screenPosition,
            flare.Frame(),
            Color.Red with
            {
                A = 30
            },
            MathHelper.PiOver2,
            flare.Size() / 2,
            new Vector2(0.3f, 3f) * Projectile.scale,
            0
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        var edgeColor = new Color(1f, 0.06f, 0.06f);
        var timeOffset = Projectile.identity * 2.5552343f;

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", -Projectile.timeLeft / 350f + timeOffset);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.42f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 2f);
        riftShader.TrySetParameter("vanishInterpolant", 0f);
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.15f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        var closeTime = MathF.Cbrt(Utils.GetLerpValue(0, 40, Projectile.timeLeft, true));
        var drawScale = (Projectile.scale + MathF.Sin(Projectile.timeLeft / 30f) * 0.2f) * OpenScale * closeTime;

        Main.spriteBatch.Draw
            (innerRiftTexture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + MathHelper.PiOver2, innerRiftTexture.Size() * 0.5f, 0.4f * drawScale, 0, 0);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        return false;
    }
}