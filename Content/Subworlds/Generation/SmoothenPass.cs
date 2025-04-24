using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class SmoothenPass : GenPass
{
    /// <summary>
    /// The set of all points that should not be smoothened by this pass.
    /// </summary>
    public static HashSet<Point> PointsToNotSmoothen
    {
        get;
        private set;
    } = new HashSet<Point>(512);

    public SmoothenPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Smoothing the world.";

        for (int x = 5; x < Main.maxTilesX - 5; x++)
        {
            for (int y = 5; y < Main.maxTilesY - 5; y++)
            {
                Point p = new Point(x, y);
                Tile t = Main.tile[p];
                if (t.HasTile && t.LiquidAmount <= 0 && !PointsToNotSmoothen.Contains(p))
                {
                    Tile.SmoothSlope(x, y, false);
                    WorldGen.TileFrame(x, y);
                }
            }
        }
        PointsToNotSmoothen.Clear();
    }
}
