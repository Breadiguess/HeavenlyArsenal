using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitBlackHoleRenderer : ModSystem
{
    /// <summary>
    /// The render target that holds all black holes.
    /// </summary>
    internal static InstancedRequestableTarget blackHoleTarget;

    /// <summary>
    /// The fire particle system used for charging up black holes.
    /// </summary>
    public static FireParticleSystem ParticleSystem
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        ParticleSystem = new FireParticleSystem(GennedAssets.Textures.Particles.FireParticleA, 34, 512, PrepareShader, UpdateParticle);
        Main.ContentThatNeedsRenderTargets.Add(blackHoleTarget = new InstancedRequestableTarget());
        On_Main.DrawProjectiles += RenderBlackHolesWrapper;
    }

    private static void PrepareShader()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Main.instance.GraphicsDevice.BlendState = BlendState.Additive;

        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitFireParticleDissolveShader");
        overlayShader.TrySetParameter("pixelationLevel", 3000f);
        overlayShader.TrySetParameter("turbulence", 0.023f);
        overlayShader.TrySetParameter("screenPosition", Main.screenPosition);
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.TrySetParameter("imageSize", GennedAssets.Textures.Particles.FireParticleA.Value.Size());
        overlayShader.TrySetParameter("initialGlowIntensity", 0.81f);
        overlayShader.TrySetParameter("initialGlowDuration", 0.285f);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleA, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Particles.FireParticleB, 2, SamplerState.LinearClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 3, SamplerState.LinearWrap);
        overlayShader.Apply();
    }

    public override void PostUpdateDusts() => ParticleSystem.UpdateAll();

    private static void UpdateParticle(ref FastParticle particle)
    {
        float growthRate = 0.02f;
        particle.Size.X *= 1f + growthRate * 0.85f;
        particle.Size.Y *= 1f + growthRate;

        particle.Velocity *= 0.7f;
        particle.Rotation = particle.Velocity.ToRotation() + MathHelper.PiOver2;

        if (particle.Time >= 49)
            particle.Active = false;
    }

    private static void RenderIntoTarget()
    {
        ParticleSystem.RenderAll();

        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        foreach (Projectile blackHole in Main.ActiveProjectiles)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (blackHole.type == blackHoleID)
                blackHole.As<RocheLimitBlackHole>().RenderSelf();

            Main.spriteBatch.End();
        }
    }

    private static void RenderBlackHolesWrapper(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);

        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        if (!LumUtils.AnyProjectiles(blackHoleID))
            return;

        blackHoleTarget.Request(Main.screenWidth, Main.screenHeight, 0, RenderIntoTarget);
        if (blackHoleTarget.TryGetTarget(0, out RenderTarget2D target) && target is not null)
        {
            Vector2 aspectRatioCorrectionFactor = new Vector2(WotGUtils.ViewportSize.X / WotGUtils.ViewportSize.Y, 1f);
            GetBlackHoleData(aspectRatioCorrectionFactor, out float[] blackHoleRadii, out Vector2[] blackHolePositions);

            ManagedScreenFilter distortionShader = ShaderManager.GetFilter("HeavenlyArsenal.BlackHoleDistortionShader");
            distortionShader.TrySetParameter("maxLensingAngle", 72.1f);
            distortionShader.TrySetParameter("aspectRatioCorrectionFactor", aspectRatioCorrectionFactor);
            distortionShader.TrySetParameter("sourceRadii", blackHoleRadii);
            distortionShader.TrySetParameter("sourcePositions", blackHolePositions);
            distortionShader.SetTexture(target, 1);
            distortionShader.Activate();
        }
    }

    internal static void GetBlackHoleData(Vector2 aspectRatioCorrectionFactor, out float[] blackHoleRadii, out Vector2[] blackHolePositions)
    {
        int index = 0;
        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        blackHoleRadii = new float[5];
        blackHolePositions = new Vector2[5];
        foreach (Projectile blackHole in Main.ActiveProjectiles)
        {
            if (blackHole.type == blackHoleID)
            {
                if (index < blackHoleRadii.Length - 1)
                {
                    blackHoleRadii[index] = blackHole.As<RocheLimitBlackHole>().DistortionDiameter / WotGUtils.ViewportSize.X * Main.GameViewMatrix.Zoom.X;

                    Vector2 positionCoords = (blackHole.Center - Main.screenLastPosition) / WotGUtils.ViewportSize;
                    blackHolePositions[index] = (positionCoords - Vector2.One * 0.5f) * aspectRatioCorrectionFactor * Main.GameViewMatrix.Zoom + Vector2.One * 0.5f;
                }
                index++;
            }
        }
    }
}
