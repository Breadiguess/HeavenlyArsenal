using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class AwakenedBloodplate : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Armor.AwakenedBloodArmor";

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplateDefense_Body", EquipType.Body, name: "AwakenedBloodplateDefense");
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplateOffense_Body", EquipType.Body, name: "AwakenedBloodplateOffense");

            }
        }
        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            //todo: if the player who has this item doesnt have the full armor set equipped, this should use the defense sprite.
            var equipSlotBody = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodplateDefense", EquipType.Body);
            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        }
      
       
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 48;
           
        }
        private float DamageBoost = 0.12f;
        private int CritBoost = 8;
        private int LifeBoost = 245;
        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            //modPlayer.omegaBlueChestplate = true;
            modPlayer.noLifeRegen = true;
            //modPlayer.omegaBlueSet = true;
            player.statLifeMax2 += LifeBoost;

        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add this tooltip before vanilla armor tooltips. Do not add tooltip if the item is in vanity slot.
            Player player = Main.LocalPlayer;
            int bodySlot = player.armor[10].type == Item.type ? 10 : -1;
            // In Terraria, armor[10..13] are vanity slots for body, legs, head, etc.
            // Body vanity slot is armor[10]
            bool isInVanitySlot = false;
            
            if(player.armor[11].type == Item.type)
            {
                isInVanitySlot = true;
            }
            if (isInVanitySlot)
                return;

            string text =
                $"+{LifeBoost} max life\n" +
                $"+{DamageBoost * 100:F0}% to all damage\n" +
                $"+{CritBoost}% crit chance";

            TooltipLine line = new TooltipLine(Mod, "AwakenedBloodPlate", text)
            {
                OverrideColor = new Color(200, 50, 50)
            };

            // Insert before vanilla armor tooltips (which have Mod == "Terraria" and Name == "Tooltip#")
            int insertIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));
            if (insertIndex == -1)
                tooltips.Add(line);
            else
                tooltips.Insert(insertIndex, line);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OmegaBlueChestplate>().
                AddIngredient<BloodflareBodyArmor>().AddCondition(conditions: Condition.BloodMoon).
                AddIngredient<UmbralLeechDrop>(7).
                AddIngredient<YharonSoulFragment>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
