using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SacredGrass : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileMerge[TileID.Dirt][Type] = true;
        Main.tileMerge[Type][TileID.Dirt] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        RegisterItemDrop(ItemID.DirtBlock);

        AddMapEntry(new Color(94, 51, 128));

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Dirt;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;
    }

    public override void NumDust(int i, int j, bool fail, ref int Type)
    {
        Type = fail ? 1 : 3;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        Tile t = Main.tile[i, j];

        int horizontalChoiceX = (i + j * 13) % 3;
        bool isLeftEdge = !Framing.GetTileSafely(i - 1, j).HasTile;
        bool isRightEdge = !Framing.GetTileSafely(i + 1, j).HasTile;
        int distanceToLiquid = -1;
        for (int dy = 1; dy <= 2; dy++)
        {
            Tile below = Framing.GetTileSafely(i, j + dy);
            if (!below.HasTile && below.LiquidAmount >= 128)
            {
                distanceToLiquid = dy;
                break;
            }
        }

        if (distanceToLiquid != -1)
        {
            t.TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
            t.TileFrameY = (short)(108 - (distanceToLiquid - 1) * 18);
        }
        else if (isLeftEdge)
        {
            t.TileFrameX = 0;
            t.TileFrameY = 54;
        }
        else if (isRightEdge)
        {
            t.TileFrameX = 18;
            t.TileFrameY = 54;
        }
        else
        {
            Tile above = Framing.GetTileSafely(i, j - 1);
            bool grassAbove = above.HasTile && above.TileType == Type;
            t.TileFrameX = (short)(horizontalChoiceX * 18 + 18);
            t.TileFrameY = 0;
            if (grassAbove)
            {
                bool dirtLayer = j % 3 != 0;
                t.TileFrameY = (short)(dirtLayer ? 36 : 18);
            }
        }

        return false;
    }
}
