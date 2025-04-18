using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineBackground : Background
{
    private static readonly Vector2[] lanternPathOffsets = new Vector2[67];

    private static readonly Asset<Texture2D> skyColorGradient = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/ShrineSkyColor");

    private static readonly Asset<Texture2D> scarletMoon = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/TheScarletMoon");

    private static Vector2 MoonPosition => WotGUtils.ViewportSize * new Vector2(0.67f, 0.15f);

    public override float Priority => 1f;

    protected override Background CreateTemplateEntity() => new ForgottenShrineBackground();

    public override void Load()
    {
        for (int i = 0; i < lanternPathOffsets.Length; i++)
        {
            float completionRatio = i / (float)(lanternPathOffsets.Length - 1f);
            float angle = MathHelper.TwoPi * completionRatio * 3f;
            float radius = MathF.Exp(angle * 0.11f) * MathF.Sqrt(angle) * 74f;
            Vector2 offset = Vector2.UnitY.RotatedBy(angle) * radius;

            lanternPathOffsets[i] = offset;
        }
    }

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        RenderGradient();
        RenderMoon();
        RenderLanternBackglowPath();
    }

    private static void ResetSpriteBatch()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen, null, Matrix.Identity);
    }

    private static void RenderGradient()
    {
        SetSpriteSortMode(SpriteSortMode.Immediate, Matrix.Identity);

        ManagedShader gradientShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineSkyGradientShader");
        gradientShader.TrySetParameter("gradientSteepness", 1.5f);
        gradientShader.TrySetParameter("gradientYOffset", Main.screenPosition.Y / Main.maxTilesY / 16f - 0.2f);
        gradientShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        gradientShader.SetTexture(skyColorGradient.Value, 2, SamplerState.LinearClamp);
        gradientShader.Apply();

        Texture2D pixel = MiscTexturesRegistry.Pixel.Value;
        Vector2 screenArea = WotGUtils.ViewportSize;
        Vector2 textureArea = screenArea / pixel.Size();
        Main.spriteBatch.Draw(pixel, screenArea * 0.5f, null, Color.Black, 0f, pixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    private static void RenderMoon()
    {
        ResetSpriteBatch();

        Texture2D moon = scarletMoon.Value;
        Main.spriteBatch.Draw(moon, MoonPosition, null, Color.White, 0f, moon.Size() * 0.5f, 0.8f, 0, 0f);
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

        base.Update();
    }
}
