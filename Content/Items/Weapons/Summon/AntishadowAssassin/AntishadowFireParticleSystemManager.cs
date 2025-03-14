using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

[Autoload(Side = ModSide.Client)]
public class AntishadowFireParticleSystemManager : ModSystem
{
    /// <summary>
    /// The particle system used to render the antishadow fire behind projectiles.
    /// </summary>
    public static FireParticleSystem BackParticleSystem
    {
        get;
        private set;
    }

    /// <summary>
    /// The particle system used to render the antishadow fire.
    /// </summary>
    public static FireParticleSystem ParticleSystem
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        BackParticleSystem = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, 34, 1024, PrepareShader, UpdateParticle);
        ParticleSystem = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, 34, 1024, PrepareShader, UpdateParticle);
        On_Main.DrawProjectiles += RenderParticlesBehindProjectiles;
        On_Main.DrawDust += RenderParticles;
    }

    private static void PrepareShader()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowFireParticleDissolveShader");
        overlayShader.TrySetParameter("pixelationLevel", 3000f);
        overlayShader.TrySetParameter("turbulence", 0.023f);
        overlayShader.TrySetParameter("screenPosition", Main.screenPosition);
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.TrySetParameter("imageSize", GennedAssets.Textures.Particles.FireParticleA.Value.Size());
        overlayShader.TrySetParameter("initialGlowIntensity", 0.42f);
        overlayShader.TrySetParameter("initialGlowDuration", 0.285f);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleA, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleB, 2, SamplerState.LinearClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 3, SamplerState.LinearWrap);
        overlayShader.Apply();
    }

    private static void UpdateParticle(ref FastParticle particle)
    {
        float growthRate = 0.02f;
        particle.Size.X *= 1f + growthRate * 0.85f;
        particle.Size.Y *= 1f + growthRate;

        particle.Velocity *= 0.7f;
        particle.Rotation = particle.Velocity.ToRotation() + MathHelper.PiOver2;

        if (particle.Time >= ParticleSystem.ParticleLifetime + 15)
            particle.Active = false;
    }

    public override void PreUpdateEntities()
    {
        BackParticleSystem.UpdateAll();
        ParticleSystem.UpdateAll();
    }

    private void RenderParticlesBehindProjectiles(On_Main.orig_DrawProjectiles orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        BackParticleSystem.RenderAll();
        Main.spriteBatch.End();
        orig(self);
    }

    private static void RenderParticles(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        ParticleSystem.RenderAll();
        Main.spriteBatch.End();
    }
}
