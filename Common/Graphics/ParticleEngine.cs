using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Graphics;

public class ParticleEngine : ILoadable
{
    /// <summary>
    /// Renders over dusts.
    /// </summary>
    public static ParticleRenderer Particles = new ParticleRenderer();
    public static ParticleRenderer ShaderParticles = new ParticleRenderer();

    public static void Clear()
    {
        Particles.Clear();
        ShaderParticles.Clear();
    }

    public void Load(Mod mod)
    {
        On_Main.UpdateParticleSystems += UpdateParticles;
        On_Main.DrawDust += DrawParticles;
    }

    private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
    {
        orig(self);
        ShaderParticles.Update();
        Particles.Update();
    }

    private void DrawParticles(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        ShaderParticles.Settings.AnchorPosition = -Main.screenPosition;
        ShaderParticles.Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        Particles.Settings.AnchorPosition = -Main.screenPosition;
        Particles.Draw(Main.spriteBatch);
        Main.spriteBatch.End();
    }

    public void Unload()
    {

    }
}