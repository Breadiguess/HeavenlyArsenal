using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.LightingMask;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrinePillarManager : WorldOrientedTileObjectManager<ShrinePillarData>
{
    public override void OnModLoad() => On_Main.DoDraw_Tiles_Solid += RenderPillars;

    private void RenderPillars(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        if (TileObjects.Count >= 1)
        {
            ManagedShader lightShader = ShaderManager.GetShader("HeavenlyArsenal.LightingShader");
            lightShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            lightShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
            lightShader.SetTexture(LightingMaskTargetManager.LightTarget, 1, SamplerState.LinearClamp);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, lightShader.Shader.Value, Main.GameViewMatrix.TransformationMatrix);

            foreach (ShrinePillarData lily in TileObjects)
                lily.Render();
            Main.spriteBatch.End();
        }

        orig(self);
    }
}
