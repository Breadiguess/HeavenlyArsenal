using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineBackground : Background
{
    private static readonly Asset<Texture2D> skyColorGradient = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/ShrineSkyColor");

    private static readonly Asset<Texture2D> scarletMoon = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Subworlds/TheScarletMoon");

    private static Vector2 moonPosition => WotGUtils.ViewportSize * new Vector2(0.67f, 0.15f);

    public override float Priority => 1f;

    protected override Background CreateTemplateEntity() => new ForgottenShrineBackground();

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        RenderGradient();
        RenderMoon();
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

        SetSpriteSortMode(SpriteSortMode.Immediate, Matrix.Identity);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen, null, Matrix.Identity);
    }

    private static void RenderMoon()
    {
        Texture2D moon = scarletMoon.Value;
        Main.spriteBatch.Draw(moon, moonPosition, null, Color.White, 0f, moon.Size() * 0.5f, 0.25f, 0, 0f);
    }

    public override void Update()
    {
        SkyManager.Instance["Ambience"].Deactivate();
        SkyManager.Instance["Party"].Deactivate();

        base.Update();
    }
}
