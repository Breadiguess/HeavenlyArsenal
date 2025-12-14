using CalamityMod;

namespace HeavenlyArsenal.Core.Systems;

internal class LineAlgorithm
{
    public static bool RaytraceTo(int x0, int y0, int x1, int y1, bool ignoreHalfTiles = false)
    {
        //Bresenham's algorithm
        var horizontalDistance = Math.Abs(x1 - x0); //Delta X
        var verticalDistance = Math.Abs(y1 - y0); //Delta Y
        var horizontalIncrement = x1 > x0 ? 1 : -1; //S1
        var verticalIncrement = y1 > y0 ? 1 : -1; //S2

        var x = x0;
        var y = y0;
        var E = horizontalDistance - verticalDistance;

        while (true)
        {
            if (Main.tile[x, y].IsTileSolid() && (!ignoreHalfTiles || !Main.tile[x, y].IsHalfBlock))
            {
                return false;
            }

            if (x == x1 && y == y1)
            {
                return true;
            }

            var E2 = E * 2;

            if (E2 >= -verticalDistance)
            {
                if (x == x1)
                {
                    return true;
                }

                E -= verticalDistance;
                x += horizontalIncrement;
            }

            if (E2 <= horizontalDistance)
            {
                if (y == y1)
                {
                    return true;
                }

                E += horizontalDistance;
                y += verticalIncrement;
            }
        }
    }

    public static Point? RaycastTo(Vector2 start, Vector2 end, bool ignoreHalfTiles = false, bool debug = false)
    {
        // Convert world coordinates → tile coordinates
        var x0 = (int)(start.X / 16f);
        var y0 = (int)(start.Y / 16f);
        var x1 = (int)(end.X / 16f);
        var y1 = (int)(end.Y / 16f);

        return RaycastTo(x0, y0, x1, y1, ignoreHalfTiles, debug);
    }

    public static Point? RaycastTo(int x0, int y0, int x1, int y1, bool ignoreHalfTiles = false, bool spawnDebugDust = false)
    {
        // Clamp the start and end points to prevent out-of-range crashes.
        x0 = Utils.Clamp(x0, 0, Main.maxTilesX - 1);
        y0 = Utils.Clamp(y0, 0, Main.maxTilesY - 1);
        x1 = Utils.Clamp(x1, 0, Main.maxTilesX - 1);
        y1 = Utils.Clamp(y1, 0, Main.maxTilesY - 1);

        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x1 > x0 ? 1 : -1;
        var sy = y1 > y0 ? 1 : -1;

        var x = x0;
        var y = y0;
        var err = dx - dy;

        while (true)
        {
            // Out-of-bounds check (prevent broken tiles)
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
            {
                break;
            }

            if (spawnDebugDust)
            {
                // World coordinates of this tile’s center
                var basePos = new Vector2(x * 16 + 8, y * 16 + 8);

                // Spawn multiple dust points per tile to increase sampling resolution
                const int density = 4; // increase if needed

                for (var i = 0; i < density; i++)
                {
                    // Evenly spread dust across 16px tile area horizontally and vertically
                    var lerpX = i / (float)(density - 1);
                    var lerpY = i / (float)(density - 1);

                    var dustPos = basePos + new Vector2(lerpX, lerpY);
                    Dust.NewDustPerfect(dustPos, DustID.Cloud, Vector2.Zero, 0, Color.White);
                }
            }

            var tile = Main.tile[x, y];

            if (tile != null &&
                tile.HasTile &&
                tile.IsTileSolid() &&
                (!ignoreHalfTiles || !tile.IsHalfBlock))
            {
                // Return the first solid tile hit
                return new Point(x, y);
            }

            if (x == x1 && y == y1)
            {
                break;
            }

            var e2 = err * 2;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        // No tile hit
        return null;
    }
}