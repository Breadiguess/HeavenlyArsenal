using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    [AutoloadEquip(EquipType.Neck)]
    public class LeechScarfItem : ModItem
    {
        public const string WearingAccessory = "WearingLeechScarf";

        public override string LocalizationCategory => "Items.Accessories";
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            
            Item.accessory = true;
            
            Item.width = 26;
            Item.height = 42;
            
            Item.defense = 7;
            
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var calamityPlayer = player.Calamity();
            
            calamityPlayer.bloodyWormTooth = true;
            
            player.endurance += 0.12f;
            
            player.GetDamage(DamageClass.Melee) += 0.12f;
            player.GetDamage(ModContent.GetInstance<TrueMeleeDamageClass>()) += 0.12f;
            player.GetDamage(ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>()) += 0.12f;

            player.GetModPlayer<LeechScarfPlayer>().Active = true;
        }

        public override void AddRecipes()
        {
            base.AddRecipes();
            
            CreateRecipe()
                .AddIngredient<BloodyWormScarf>()
                .AddIngredient<UmbralLeechDrop>(8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
