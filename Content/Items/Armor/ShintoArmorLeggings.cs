using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Legs value here will result in TML expecting a X_Legs.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Legs)]
	public class ShintoArmorLeggings : ModItem
	{
		public static readonly int MoveSpeedBonus = 5;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBonus);

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 54; // The amount of defense the item will give when equipped
		}
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.moveSpeed += 0.5f;
            player.runAcceleration *= 1.2f;
            player.maxRunSpeed *= 1.2f;
            player.accRunSpeed *= 0.5f;
            player.runSlowdown *= 2f;
            var modPlayer = player.Calamity();
            modPlayer.shadowSpeed = true;
            player.moveSpeed += 0.3f;
        }
        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes() {

            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient<DemonshadeGreaves>()
                .AddIngredient(ItemID.NinjaPants)
                .AddIngredient(ItemID.CrystalNinjaLeggings)
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunPants").Type)
                .AddIngredient<StatisVoidSash>()
                .AddTile<DraedonsForge>()
                .Register();
            }
        }
	}
}
