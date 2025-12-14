using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;
using Luminance.Common.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;

public class FakeFlowerTileRender : FakeFlowerRender
{
    public override int ItemID => ModContent.ItemType<FakeFlowerDebugItem>();

    public override int TileID => ModContent.TileType<FakeFlowerTile>();

    public override void UpdatePoint(Point p) { }

    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {
        // Draw the flower.
        var drawPosition = new Vector2((i + 0.5f) * 16f, j * 16f + 18f) - Main.screenPosition;
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/PlantForm").Value;
        var scale = new Vector2(MathF.Pow(growthInterpolantModified, 1.7f), growthInterpolantModified);

        var frame = texture.Frame(1, 2);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(default, BlendState.Opaque, SamplerState.PointClamp, default, default);
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }
}