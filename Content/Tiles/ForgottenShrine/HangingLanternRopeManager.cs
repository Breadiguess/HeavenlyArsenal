using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Common.Utilities;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class HangingLanternRopeManager : WorldOrientedTileObjectManager<HangingLanternRopeData>
{
    public override void PostDrawTiles()
    {
        if (TileObjects.Count <= 0)
            return;

        Main.spriteBatch.ResetToDefault(false);
        foreach (HangingLanternRopeData rope in TileObjects)
            rope.Render();

        Main.spriteBatch.End();
    }
}
