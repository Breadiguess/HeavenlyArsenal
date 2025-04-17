using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.BackgroundManagement;
using Terraria;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineBackground : Background
{
    public override float Priority => 1f;

    protected override Background CreateTemplateEntity() => new ForgottenShrineBackground();

    public override void Render(Vector2 backgroundSize, float minDepth, float maxDepth)
    {
        SetSpriteSortMode(SpriteSortMode.Immediate, Matrix.Identity);

        ManagedShader fogShader = ShaderManager.GetShader("NoxusBoss.AvatarUniverseFogBackgroundShader");
        fogShader.TrySetParameter("arcCurvature", 2.2f);
        fogShader.TrySetParameter("fogColor", new Vector4(0.04f, 0.04f, 0.054f, 1f) * Opacity);
        fogShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        fogShader.Apply();

        Texture2D pixel = MiscTexturesRegistry.Pixel.Value;
        Vector2 screenArea = WotGUtils.ViewportSize;
        Vector2 textureArea = screenArea / pixel.Size();
        Main.spriteBatch.Draw(pixel, screenArea * 0.5f, null, Color.Black, 0f, pixel.Size() * 0.5f, textureArea, 0, 0f);
    }

    public override void Update()
    {
        base.Update();

        ManagedScreenFilter fogShader = ShaderManager.GetFilter("NoxusBoss.AvatarUniverseRedFogShader");
        fogShader.TrySetParameter("intensity", Opacity);
        fogShader.TrySetParameter("fogDensityExponent", 5.6f);
        fogShader.TrySetParameter("fogColor", new Vector4(1f, 0f, 0.15f, 0f));
        fogShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        fogShader.Activate();
    }
}
