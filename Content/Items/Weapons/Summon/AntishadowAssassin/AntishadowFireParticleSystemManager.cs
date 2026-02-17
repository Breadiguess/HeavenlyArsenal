using Luminance.Core.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.FastParticleSystems;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

[Autoload(Side = ModSide.Client)]
public class AntishadowFireParticleSystemManager : ModSystem
{
    /// <summary>
    ///     The particle system used to render the antishadow fire behind projectiles.
    /// </summary>
    public static Dictionary<int, FireParticleSystem> BackParticleSystem { get; } = new(Main.maxPlayers);

    /// <summary>
    ///     The particle system used to render the antishadow fire.
    /// </summary>
    public static Dictionary<int, FireParticleSystem> ParticleSystem { get; } = new(Main.maxPlayers);

    private static int particleLifetime => 34;

    private static void PrepareShader()
    {
        if (Main.dedServ)
        {
            return;
        }

        var world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        var projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        var overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowFireParticleDissolveShader");
        overlayShader.TrySetParameter("pixelationLevel", 3000f);
        overlayShader.TrySetParameter("turbulence", 0.023f);
        overlayShader.TrySetParameter("screenPosition", Main.screenPosition);
        overlayShader.TrySetParameter("uWorldViewProjection", world * projection);
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
        var growthRate = 0.02f;
        particle.Size.X *= 1f + growthRate * 0.85f;
        particle.Size.Y *= 1f + growthRate;

        particle.Velocity *= 0.7f;
        particle.Rotation = particle.Velocity.ToRotation() + MathHelper.PiOver2;

        if (particle.Time >= particleLifetime + 15)
        {
            particle.Active = false;
        }
    }

    public override void PreUpdateEntities()
    {
        if (Main.dedServ)
        {
            return;
        }

        foreach (var system in BackParticleSystem.Values)
        {
            system.UpdateAll();
        }

        foreach (var system in ParticleSystem.Values)
        {
            system.UpdateAll();
        }
    }

    /// <summary>
    ///     Creates a new fire. particle
    /// </summary>
    public static void CreateNew(int playerIndex, bool behindProjectiles, Vector2 spawnPosition, Vector2 velocity, Vector2 size, Color color)
    {
        if (Main.dedServ)
        {
            return;
        }

        var maxParticles = 512;
        FireParticleSystem system;

        if (behindProjectiles)
        {
            if (BackParticleSystem.TryGetValue(playerIndex, out var s))
            {
                system = s;
            }
            else
            {
                system = BackParticleSystem[playerIndex] = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, particleLifetime, maxParticles, PrepareShader, UpdateParticle);
            }
        }
        else
        {
            if (ParticleSystem.TryGetValue(playerIndex, out var s))
            {
                system = s;
            }
            else
            {
                system = ParticleSystem[playerIndex] = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, particleLifetime, maxParticles, PrepareShader, UpdateParticle);
            }
        }

        system.CreateNew(spawnPosition, velocity, size, color);
    }
}