using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{

    [AutoloadEquip(EquipType.Head)]

    public class AwakenedBloodHelm : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Armor.AwakenedBloodArmor";
        internal static string TentacleEntitySourceContext => "SetBonus_HeavenlyArsenal_BloodArmor";

        

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodHelmDefense_Head", EquipType.Head, name: "AwakenedBloodHelmDefense");
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodHelmOffense_Head", EquipType.Head, name: "AwakenedBloodHelmOffense");

            }
        }
        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            var equipSlotHead = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodHelmDefense", EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        }
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 49; //85
            
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<AwakenedBloodplate>() && legs.type == ModContent.ItemType<AwakenedBloodStrides>();


        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadowSubtle = false;
        }
        
        public override void UpdateArmorSet(Player player)
        {
            //player.setBonus = this.GetLocalizedValue("SetBonus") + "\n";
            player.setBonus = Language.GetOrRegister(("Items.Armor.AwakenedBloodArmor.AwakenedBloodHelm.SetBonus")).Value;
            player.setBonus = this.GetLocalizedValue("SetBonus");
            var modPlayer = player.Calamity();
            player.GetAttackSpeed<MeleeDamageClass>() += 0.18f;
            modPlayer.bloodflareSet = true;
            modPlayer.bloodflareMelee = true;
            modPlayer.abyssalDivingSuitPlates = true;
            modPlayer.abyssalAmulet = true;
            modPlayer.reaverRegen = true;
            player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped = true;

            player.crimsonRegen = true;
            player.aggro += 900;
        }

        private float DamageBoost = 0.1f;
        private int CritBoost = 5;
        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<MeleeDamageClass>() += CritBoost;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add this tooltip before vanilla armor tooltips. Do not add tooltip if the item is in vanity slot.
            Player player = Main.LocalPlayer;
            int bodySlot = player.armor[10].type == Item.type ? 10 : -1;
            // In Terraria, armor[10..13] are vanity slots for body, legs, head, etc.
            // Body vanity slot is armor[10]
            bool isInVanitySlot = false;

            if (player.armor[10].type == Item.type)
            {
                isInVanitySlot = true;
            }
            if (isInVanitySlot)
                return;
            // build a single combined tooltip line
            string text =
                $"+{DamageBoost * 100:F0}% to all damage\n" +
                $"+{CritBoost}% crit chance";

            // create and add it
            TooltipLine line = new TooltipLine(Mod, "AwakenedBloodHelm", text)
            {
                OverrideColor = new Color(200, 50, 50)  // optional
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
                     AddIngredient<OmegaBlueHelmet>().
                     AddRecipeGroup("HeavenlyArsenal:BloodflareHelmets", 1).
                     AddIngredient<YharonSoulFragment>(15).
                     //AddIngredient<UmbralLeechDrop>(3).
                     AddCondition(conditions: Condition.BloodMoon).
                     AddTile<CosmicAnvil>().
                     Register();

          
        }
    }


    public class bloodRecipeGroup : ModSystem
    {
        public static LocalizedText BloodflareHelmetGroupText;

        public override void Load()
        {
            BloodflareHelmetGroupText = Language.GetOrRegister(Mod.GetLocalizationKey("Items.Armor.BloodflareHelmetGroup"));
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {BloodflareHelmetGroupText.Value}",
                ModContent.ItemType<BloodflareHeadMagic>(),
                ModContent.ItemType<BloodflareHeadMelee>(),
                ModContent.ItemType<BloodflareHeadRanged>(),
                ModContent.ItemType<BloodflareHeadRogue>(),
                ModContent.ItemType<BloodflareHeadSummon>());
            RecipeGroup.RegisterGroup("HeavenlyArsenal:BloodflareHelmets", group);
        }
    }

}


   

