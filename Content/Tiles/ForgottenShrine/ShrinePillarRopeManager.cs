using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Common.Utilities;
using System.Linq;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrinePillarRopeManager : WorldOrientedTileObjectManager<ShrinePillarRopeData>
{
    /// <summary>
    /// Registers a new rope into the set of ropes maintained by the world.
    /// </summary>
    public override void Register(ShrinePillarRopeData rope)
    {
        bool ropeAlreadyExists = TileObjects.Any(r => (r.Start == rope.Start && r.End == rope.End) ||
                                                      (r.Start == rope.End && r.End == rope.Start));
        if (ropeAlreadyExists)
            return;

        base.Register(rope);
    }

    public override void PostDrawTiles()
    {
        if (TileObjects.Count <= 0)
            return;

        Main.spriteBatch.ResetToDefault(false);
        foreach (ShrinePillarRopeData rope in TileObjects)
            rope.Render();

        Main.spriteBatch.End();
    }
}
