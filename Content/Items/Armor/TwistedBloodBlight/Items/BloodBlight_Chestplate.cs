using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Armor.BaseArmor;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Body)]
    public class BloodBlight_Chestplate : BaseArmorItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.defense = 22;
        }

        protected override void ApplyEquipStats(Player player)
        {
            Stats.AddDamage(player, 0.10f,
        locOverride: "Mods.HeavenlyArsenal.Armor.BloodBlight.BloodBlight_Helmet.Crit");
            Stats.AddCrit(player, 6, color: Color.Crimson);
            Stats.AddMoveSpeed(player, -0.15f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<OmegaBlueChestplate>()
                .AddIngredient<BloodflareBodyArmor>()
                .AddCondition(Condition.BloodMoon)
                .AddIngredient<UmbralLeechDrop>(5)
                .AddIngredient<ShellFragment>(7)
                .AddIngredient<YharonSoulFragment>(20)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
}
