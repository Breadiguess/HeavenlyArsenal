using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CalamityMod.Rarities;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Assets.Fonts;
using Terraria.UI.Chat;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.Tiles;
//using CalamityMod.Items.Placeables.Furniture.CraftingStations;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace HeavenlyArsenal.Content.Items.Materials
{
    internal class shadowspec_GunParts : ModItem
    {
        public override string LocalizationCategory => "Items.Misc.Materials";
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("ITEM NAME");
            //Tooltip.SetDefault("'TOOLTIP.'");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;

            ItemID.Sets.ItemNoGravity[Item.type] = false;
        }


        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.maxStack = 1;
            Item.value = 9999999;
            Item.rare = ModContent.RarityType<HotPink>();
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, 5);
        }



        public override void AddRecipes()
        {
            CreateRecipe().
                AddTile<DraedonsForge>().
                AddIngredient(ModContent.ItemType<ShadowspecBar>(), 4).
                //AddIngredient(ItemType<>).
                Register();
        }

    }
}

