using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Legs value here will result in TML expecting a X_Legs.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Legs)]
	public class ShintoArmorLeggings : ModItem
	{
		public static readonly int MoveSpeedBonus = 5;

        //public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBonus);

        public override string LocalizationCategory => "Items.Armor.ShintoArmor";
        public override void SetStaticDefaults()
        {

            if (Main.netMode != NetmodeID.Server)
            {
                var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
                ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
            }
        }

        public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ModContent.RarityType<AvatarRarity>();  // The rarity of the item
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
            player.moveSpeed += 0.3f;

            player.autoJump = true;
            player.jumpSpeedBoost += 1.6f;
            player.noFallDmg = true;
            player.GetModPlayer<ShintoArmorPlayer>().VoidBeltEquipped = true;
            modPlayer.DashID = ShintoArmorDash.ID;
            player.dashType = 0;
            player.spikedBoots = 2;
        }
        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes() {

            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeGreaves>()
                .AddIngredient(ItemID.NinjaPants)
                .AddIngredient(ItemID.CrystalNinjaLeggings)
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunPants").Type)
                .AddIngredient<StatisVoidSash>()
                .AddTile<DraedonsForge>()
                .Register();
            }
            else
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeGreaves>()
                .AddIngredient(ItemID.NinjaPants)
                .AddIngredient(ItemID.CrystalNinjaLeggings)
                .AddIngredient<StatisVoidSash>()
                .AddTile<DraedonsForge>()
                .Register();
            }
        }
	}

  

   
}
