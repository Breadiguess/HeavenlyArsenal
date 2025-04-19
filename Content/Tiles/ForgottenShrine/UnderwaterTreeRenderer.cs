using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class UnderwaterTreeRenderer : ModSystem
{
    public override void PostDrawTiles()
    {
        List<TEUnderwaterTree> trees = TileEntity.ByID.Values.Where(te => te is TEUnderwaterTree).Select(te => te as TEUnderwaterTree).ToList();
        if (trees.Count <= 0)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        ManagedShader postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineUnderwaterVegetationShader");
        postProcessingShader.TrySetParameter("underwaterOpacity", 0.09f);
        postProcessingShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        postProcessingShader.TrySetParameter("screenOffset", (Main.screenPosition - Main.screenLastPosition) / WotGUtils.ViewportSize);
        postProcessingShader.TrySetParameter("targetSize", WotGUtils.ViewportSize);
        postProcessingShader.SetTexture(TileTargetManagers.LiquidTarget, 1);
        postProcessingShader.Apply();

        foreach (TEUnderwaterTree tree in trees)
            tree.Render();

        Main.spriteBatch.End();
    }
}
