using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
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
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 48;
            Item.rare = ModContent.RarityType<PureGreen>();
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

            // build a single combined tooltip line
            string text =
                $"+{LifeBoost} max life\n" +
                $"+{DamageBoost * 100:F0}% to all damage\n" +
                $"+{CritBoost}% crit chance";

            // create and add it
            TooltipLine line = new TooltipLine(Mod, "AwakenedBloodStrides", text)
            {
                OverrideColor = new Color(200, 50, 50)  // optional
            };
            tooltips.Add(line);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<OmegaBlueChestplate>().
                AddIngredient<BloodflareBodyArmor>().AddCondition(conditions: Condition.BloodMoon).
                AddIngredient<YharonSoulFragment>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
