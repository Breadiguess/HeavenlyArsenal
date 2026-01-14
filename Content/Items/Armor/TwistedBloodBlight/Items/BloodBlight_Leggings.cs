using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Armor.BaseArmor;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Legs)]
    public class BloodBlight_Leggings : BaseArmorItem
    {

        public override string LocalizationCategory => "Items.Armor.BloodBlight";

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.defense = 15;
        }

        protected override void ApplyEquipStats(Player player)
        {
            Stats.AddDamage(player, 0.12f);
            Stats.AddCrit(player, 4);
            Stats.AddMoveSpeed(player, -0.07f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<OmegaBlueTentacles>()
                .AddIngredient<BloodflareCuisses>()
                .AddCondition(Condition.BloodMoon)
                .AddIngredient<YharonSoulFragment>(15)
                .AddIngredient<PenumbralMembrane>(4)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
}
