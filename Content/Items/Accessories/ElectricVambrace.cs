using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories
{
   
    public class ElectricVambrace : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<HeavenlyArsenalPlayer>().ElectricVambrace = true;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<ScorchedBone>(12).
            AddIngredient<DemonicBoneAsh>(3).
            AddIngredient<EssenceofHavoc>(8).
            AddTile(TileID.Anvils).
            Register();
        }
    }
}