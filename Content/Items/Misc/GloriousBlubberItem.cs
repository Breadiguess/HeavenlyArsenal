using HeavenlyArsenal.Content.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Misc
{
    internal class GloriousBlubberItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<GloriousBlubberPlaced>();
            Item.width = 12;
            Item.height = 12;
            Item.rare = ItemRarityID.Purple;
        }
    }
}
