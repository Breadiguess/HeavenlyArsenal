using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class PlacedOfudaRenderer : ModSystem
{
    public override void PostDrawTiles()
    {
        List<TEPlacedOfuda> placedOfuda = [.. TileEntity.ByID.Values.Where(te => te is TEPlacedOfuda).Select(te => te as TEPlacedOfuda)];
        if (placedOfuda.Count <= 0)
            return;

        foreach (TEPlacedOfuda ofuda in placedOfuda)
            ofuda.Render();
    }
}
