using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Common.Utilities;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrinePillarManager : WorldOrientedTileObjectManager<ShrinePillarData>
{
    public override void OnModLoad() => On_Main.DoDraw_Tiles_Solid += RenderPillars;

    private void RenderPillars(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        if (tileObjects.Count >= 1)
        {
            Main.spriteBatch.ResetToDefault(false);
            foreach (ShrinePillarData lily in tileObjects)
                lily.Render();
            Main.spriteBatch.End();
        }

        orig(self);
    }
}
