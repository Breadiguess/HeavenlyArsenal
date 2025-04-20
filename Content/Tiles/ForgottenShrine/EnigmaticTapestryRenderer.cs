using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class EnigmaticTapestryRenderer : ModSystem
{
    /// <summary>
    /// The render target in which tapestries are rendered into before being pixelated.
    /// </summary>
    public static InstancedRequestableTarget TapestryTarget
    {
        get;
        private set;
    }

    public override void OnModLoad() => Main.ContentThatNeedsRenderTargets.Add(TapestryTarget = new InstancedRequestableTarget());

    public override void PostDrawTiles()
    {
        List<TEEnigmaticTapestry> placedTapestries = [.. TileEntity.ByID.Values.Where(te => te is TEEnigmaticTapestry).Select(te => te as TEEnigmaticTapestry)];
        if (placedTapestries.Count <= 0)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        foreach (TEEnigmaticTapestry ofuda in placedTapestries)
            ofuda.Render();
        Main.spriteBatch.End();
    }
}
