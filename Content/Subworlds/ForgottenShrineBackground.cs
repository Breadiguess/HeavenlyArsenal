using HeavenlyArsenal.Common.Graphics;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using NoxusBoss.Core.Graphics.UI.GraphicalUniverseImager;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using TextureAsset = NoxusBoss.Assets.LazyAsset<Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineBackground : Background
{
    /// <summary>
    /// The curve that contains velocities for the spiral.
    /// </summary>
    private static DeCasteljauCurve lanternPositionPath;

    /// <summary>
    /// The curve that contains positions for the spiral.
    /// </summary>
    private static DeCasteljauCurve lanternVelocityPath;

    /// <summary>
    /// The particle system responsible for lanterns in the background.
    /// </summary>
    private static FramedFastParticleSystem lanternSystem;

    /// <summary>
    /// The set of discrete points that compose the spiral.
    /// </summary>
    private static readonly Vector2[] lanternPathOffsets = new Vector2[65];

    private static readonly Asset<Texture2D> skyColorGradient = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/ShrineSkyColor");

    private static readonly Asset<Texture2D> skyLantern = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/SkyLantern");

    private static readonly Asset<Texture2D> scarletMoon = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/TheScarletMoon");

    private static readonly TextureAsset icon = TextureAsset.FromPath("HeavenlyArsenal/Content/Subworlds/BiomeIcon");

    private static readonly Asset<Texture2D>[] backgroundLayers =
    [
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/BackgroundFront"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/BackgroundMid"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/BackgroundBack"),
    ];

    /// <summary>
    /// A 0-1 interpolant which dictates the extent to which the sky gradient shifts.
    /// </summary>
    public static float AltSkyGradientInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The alternate palette to use in accordance with <see cref="AltSkyGradientInterpolant"/>.
    /// </summary>
    public static Color[] AltSkyGradient
    {
        get;
        set;
    } = new Color[4];

    /// <summary>
    /// The standard palette of the background gradient.
    /// </summary>
    public static Color[] StandardPalette =>
    [
        new Color(108, 42, 80),
        new Color(55, 39, 72),
        new Color(29, 26, 47),
        new Color(7, 6, 12),
    ];

    /// <summary>
    /// The position of the moon in the background.
    /// </summary>
    public static Vector2 MoonPosition => WotGUtils.ViewportSize * new Vector2(0.67f, 0.15f);

    public override float Priority => 1f;

    protected override Background CreateTemplateEntity() => new ForgottenShrineBackground();

    public override void Load()
    {
        Vector2[] velocities = new Vector2[lanternPathOffsets.Length];
        for (int i = 0; i < lanternPathOffsets.Length; i++)
        {
            float completionRatio = i / (float)(lanternPathOffsets.Length - 1f);
            float angle = MathHelper.TwoPi * completionRatio * 3f;
            float radius = MathF.Exp(angle * 0.11f) * MathF.Sqrt(angle) * 74f;
            Vector2 offset = Vector2.UnitY.RotatedBy(angle) * radius;

            lanternPathOffsets[i] = offset;

            if (i >= 1)
                velocities[i] = lanternPathOffsets[i] - lanternPathOffsets[i - 1];
        }

        lanternPositionPath = new DeCasteljauCurve(lanternPathOffsets);
        lanternVelocityPath = new DeCasteljauCurve(velocities);

        Main.QueueMainThreadAction(() =>
        {
            lanternSystem = new FramedFastParticleSystem(5, 8192, PrepareLanternParticleRendering, UpdateLanternParticles);
        });

        GraphicalUniverseImagerOption option = new GraphicalUniverseImagerOption("Mods.HeavenlyArsenal.UI.GraphicalUniverseImager.ForgottenShrineBackground", true, icon, RenderGUIPortrait, RenderGUIBackground);
        GraphicalUniverseImagerOptionManager.RegisterNew(option);

        Main.OnPreDraw += RenderMoonToDarknessGlowTarget;
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(lanternSystem.Dispose);
        Main.OnPreDraw -= RenderMoonToDarknessGlowTarget;
    }

    private static void PrepareLanternParticleRendering()
    {
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -400f, 400f);

        Texture2D lantern = skyLantern.Value;
        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BasicPrimitiveOverlayShader");
        overlayShader.TrySetParameter("uWorldViewProjection", projection);
        overlayShader.SetTexture(lantern, 1, SamplerState.LinearClamp);
        overlayShader.Apply();
    }

    private static void UpdateLanternParticles(ref FastParticle particle)
    {
        int lifetime = 200;
        float pathInterpolant = particle.ExtraData;
        if (particle.Time >= lifetime)
            particle.Active = false;

        if (particle.Time / (float)lifetime >= 0.75f || pathInterpolant < 0.12f)
            particle.Size *= 0.93f;

        float spinSpeed = 0.000072f;
        float moveSpeedInterpolant = LumUtils.Saturate(8f / particle.Size.X);
        particle.ExtraData -= moveSpeedInterpolant * spinSpeed;
        particle.Velocity = particle.Velocity.RotatedBy(spinSpeed * -25f);
        particle.Rotation = particle.Velocity.ToRotation() - MathHelper.PiOver2;
    }

    private static void RenderMoonToDarknessGlowTarget(GameTime _)
    {
        ForgottenShrineDarknessSystem.QueueGlowAction(() =>
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            Vector2 scale = Vector2.One * 0.8f;
            Texture2D moon = scarletMoon.Value;
            Main.spriteBatch.Draw(moon, MoonPosition + Main.screenPosition - Main.screenLastPosition, null, Color.White, 0f, moon.Size() * 0.5f, scale, 0, 0f);

            Main.spriteBatch.ResetToDefault();
        });
    }

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        if (Opacity < 1f)
            return;

        if (minDepth < 0f && maxDepth > 0f)
            RenderWrapper(false, true);
    }

    private static void RenderWrapper(bool justMoonAndGradient, bool includeMountains)
    {
        RenderGradient();
        RenderMoon(justMoonAndGradient);

        if (!justMoonAndGradient)
        {
            RenderLanternBackglowPath();

            Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            lanternSystem.RenderAll();

            if (includeMountains)
                RenderMountains(WotGUtils.ViewportSize);
        }
    }

    private static void RenderGUIPortrait(GraphicalUniverseImagerSettings settings)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        RenderWrapper(true, false);
        Main.spriteBatch.End();
    }

    private static void RenderGUIBackground(float minDepth, float maxDepth, GraphicalUniverseImagerSettings settings)
    {
        if (minDepth < 0f && maxDepth > 0f)
            RenderWrapper(false, false);
    }

    private static void ResetSpriteBatch()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen, null, Matrix.Identity);
    }

    private static void RenderParallaxLayer(Vector2 backgroundSize, float parallax, Color color, Texture2D backgroundTexture)
    {
        // Loop the background horizontally.
        float scale = 2.15f;
        float screenYPositionInterpolant = Main.screenPosition.Y / Main.maxTilesY / 16f;
        for (int i = -3; i <= 3; i++)
        {
            // Draw the base background.
            Vector2 layerPosition = new Vector2(backgroundSize.X * 0.5f + backgroundTexture.Width * i * scale, backgroundSize.Y - (int)((screenYPositionInterpolant + 0.5f) * scale * 230f));
            layerPosition.Y += 10f / parallax;
            layerPosition.X -= parallax * Main.screenPosition.X % (backgroundTexture.Width * scale);

            Main.spriteBatch.Draw(backgroundTexture, layerPosition - backgroundTexture.Size() * scale * 0.5f, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    private static void RenderGradient()
    {
        SetSpriteSortMode(SpriteSortMode.Immediate, Matrix.Identity);

        Color[] standardPalette = StandardPalette;
        Vector3[] gradient = new Vector3[standardPalette.Length];
        for (int i = 0; i < gradient.Length; i++)
            gradient[i] = Color.Lerp(standardPalette[i], AltSkyGradient[i], AltSkyGradientInterpolant).ToVector3();

        ManagedShader gradientShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineSkyGradientShader");
        gradientShader.TrySetParameter("gradientSteepness", 1.5f);
        gradientShader.TrySetParameter("gradientYOffset", Main.screenPosition.Y / Main.maxTilesY / 16f - 0.2f);
        gradientShader.TrySetParameter("gradient", gradient);
        gradientShader.TrySetParameter("gradientCount", gradient.Length);
        gradientShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        gradientShader.SetTexture(skyColorGradient.Value, 2, SamplerState.LinearClamp);
        gradientShader.Apply();

        Texture2D pixel = MiscTexturesRegistry.Pixel.Value;
        Vector2 screenArea = WotGUtils.ViewportSize;
        Vector2 textureArea = screenArea / pixel.Size();
        Main.spriteBatch.Draw(pixel, screenArea * 0.5f, null, Color.Black, 0f, pixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    private static void RenderMountains(Vector2 backgroundSize)
    {
        ResetSpriteBatch();
        RenderParallaxLayer(backgroundSize, 0.035f, Color.White, backgroundLayers[2].Value);
        RenderParallaxLayer(backgroundSize, 0.081f, Color.White, backgroundLayers[1].Value);
        RenderParallaxLayer(backgroundSize, 0.133f, Color.White, backgroundLayers[0].Value);
    }

    private static void RenderMoon(bool squishToFitRT)
    {
        ResetSpriteBatch();

        Vector2 scale = Vector2.One * 0.8f;
        if (squishToFitRT)
            scale.X *= 0.7f;

        Texture2D moon = scarletMoon.Value;
        Main.spriteBatch.Draw(moon, MoonPosition, null, Color.White, 0f, moon.Size() * 0.5f, scale, 0, 0f);
    }

    private static void RenderLanternBackglowPath()
    {
        ManagedShader pathShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineBackglowPathShader");
        pathShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);

        float widthFunction(float completionRatio) => MathHelper.Lerp(45f, 397.5f, MathF.Pow(completionRatio, 1.6f));
        Color colorFunction(float completionRatio) => new Color(141, 42, 70) * (1f - completionRatio) * LumUtils.InverseLerp(0.01f, 0.15f, completionRatio);
        PrimitiveSettings settings = new PrimitiveSettings(widthFunction, colorFunction, _ => MoonPosition + Main.screenPosition, Shader: pathShader, UseUnscaledMatrix: true);

        Main.screenWidth = (int)WotGUtils.ViewportSize.X;
        Main.screenHeight = (int)WotGUtils.ViewportSize.Y;
        PrimitiveRenderer.RenderTrail(lanternPathOffsets, settings, 100);
    }

    public override void Update()
    {
        SkyManager.Instance["Ambience"].Deactivate();
        SkyManager.Instance["Party"].Deactivate();

        AltSkyGradientInterpolant = (AltSkyGradientInterpolant * 0.975f).StepTowards(0f, 0.005f);

        for (int i = 0; i < 40; i++)
        {
            float pathInterpolant = Main.rand.NextFloat(0.05f, 1f);
            float size = MathHelper.Lerp(2.5f, 11.5f, MathF.Pow(Main.rand.NextFloat(), 5f)) * Main.rand.NextFloat(0.4f, 1.2f);
            Vector2 spawnPosition = MoonPosition + lanternPositionPath.Evaluate(pathInterpolant) * 1.6f + Main.rand.NextVector2Circular(210f, 210f);
            Vector2 velocity = lanternVelocityPath.Evaluate(pathInterpolant) * -Main.rand.NextFloat(0.007f, 0.03f);
            lanternSystem?.CreateNew(spawnPosition, velocity, Vector2.One * size, new Color(255, Main.rand.Next(40, 150), 33) * 0.75f, pathInterpolant);
        }
        lanternSystem.UpdateAll();

        base.Update();
    }
}
