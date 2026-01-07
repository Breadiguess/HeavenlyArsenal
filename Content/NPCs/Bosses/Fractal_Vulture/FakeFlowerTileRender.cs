using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.LightingMask;

namespace HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;

public class FakeFlowerTileRender : FakeFlowerRender
{
    public override int ItemID => ModContent.ItemType<FakeFlowerDebugItem>();

    public override int TileID => ModContent.TileType<FakeFlowerTile>();

    public override void UpdatePoint(Point p) { }

    public float Opacity = 1;
    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {
        if (voidVulture.Myself is not null)
        {
            Opacity = float.Lerp(Opacity, -1, 0.01f);
        }
        else
            Opacity = 1;

        if (Opacity <= 0)
            return;

        // Draw the flower.
        var drawPosition = new Vector2((i + 0.5f) * 16f, j * 16f + 20f) - Main.screenPosition;
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/PlantForm").Value;
        var scale = new Vector2(MathF.Pow(growthInterpolantModified, 1.7f), growthInterpolantModified);

        var frame = texture.Frame(1, 2);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        var worldAnchor = GetAnchorWorldPosition(new Point(i, j));
        bool playerNearby = Main.LocalPlayer.WithinRange(worldAnchor, 300f);

        Color outlineColor = playerNearby
            ? new Color(255, 255, 47) * Opacity
            : Color.Transparent;


        var idealTarget = texture;
        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BaseGenesisOverlayShader");
        overlayShader.TrySetParameter("pixelationFactor", Vector2.One * 0.01f / idealTarget.Size());
        overlayShader.TrySetParameter("textureSize0", idealTarget.Size());
        overlayShader.TrySetParameter("lightInfluenceFactor", 1);
        overlayShader.TrySetParameter("morphToGenesisInterpolant", 1);
        overlayShader.TrySetParameter("screenArea", Main.ScreenSize.ToVector2());
        overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        overlayShader.TrySetParameter("outlineColor", outlineColor.ToVector4());
        overlayShader.SetTexture(texture, 1, SamplerState.PointClamp);
        overlayShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 2, SamplerState.LinearWrap);
        overlayShader.SetTexture(LightingMaskTargetManager.LightTarget, 3, SamplerState.LinearWrap);
        overlayShader.Apply();

        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * Opacity, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }
}