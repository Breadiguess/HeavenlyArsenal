using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.ArsenalPlayer;
using NoxusBoss.Content.Rarities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Head)]
	public class ShintoArmorHelmetRogue : ModItem
	{
		public static readonly int AdditiveGenericDamageBonus = 20;
        public const float TeleportRange = 2000f;

        // Boosted by Cross Necklace.
        internal static readonly int ShadowVeilIFrames = 80;
        public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults() {
			// If your head equipment should draw hair while drawn, use one of the following:
			//ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false; // Don't draw the head at all. Used by Space Creature Mask
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true; // Draw hair as if a hat was covering the top. Used by Wizards Hat
			// ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses
			// ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(AdditiveGenericDamageBonus);

		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 999); // How many coins the item is worth
            Item.rare = ModContent.RarityType<AvatarRarity>();  // The rarity of the item
            Item.defense = 60; // The amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<ShintoArmorBreastplate>() && legs.type == ModContent.ItemType<ShintoArmorLeggings>();
		}

        // UpdateArmorSet allows you to give set bonuses to the armor.
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetOrRegister(Mod.GetLocalizationKey("SetBonuses.Shogun")).Value;
            player.jumpSpeedBoost += 2f;
            player.GetModPlayer<ShintoArmorPlayer>().SetActive = true;
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.maxMinions += 10;
            var modPlayer = player.Calamity();
            
            modPlayer.rogueStealthMax += 1.5f;
            player.setBonus = this.GetLocalizedValue("SetBonus");
            //player.GetDamage<ThrowingDamageClass>() += 0.05f;
            player.Calamity().wearingRogueArmor = true;
        }

        
		public override void UpdateEquip(Player player)
		{ 
            
            player.Calamity().stealthGenMoving += 0.15f;
            player.Calamity().stealthGenStandstill += 0.15f;
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
			player.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;
		}
        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.SpectralVeilHotKey);


        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes() {


			if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient<DemonshadeHelm>()
                .AddIngredient(ItemID.NinjaHood)
				.AddIngredient(ItemID.CrystalNinjaHelmet)
				.AddIngredient(CalamityHunt.Find<ModItem>("ShogunHelm").Type)
				.AddIngredient<SpectralVeil>()
				.AddTile<DraedonsForge>()
                .Register();
            }
			else
			{
                CreateRecipe()
               .AddIngredient<DemonshadeHelm>()
               .AddIngredient(ItemID.NinjaHood)
               .AddIngredient(ItemID.CrystalNinjaHelmet)
               .AddIngredient<SpectralVeil>()
               .AddTile<DraedonsForge>()
               .Register();
            }
            
		}
	}
}
