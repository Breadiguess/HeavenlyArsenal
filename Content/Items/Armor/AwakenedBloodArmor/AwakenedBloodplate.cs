using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

[AutoloadEquip(EquipType.Body)]
public class AwakenedBloodplate : ModItem, ILocalizedModType
{
    private readonly float DamageBoost = 0.12f;

    private readonly int CritBoost = 8;

    private readonly int LifeBoost = 245;

    public override string LocalizationCategory => "Items.Armor.AwakenedBloodArmor";

    public override void Load() { }

    public override void SetStaticDefaults()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

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
        // Add this tooltip before vanilla armor tooltips.
        // Do not add tooltip if the item is in vanity slot.
        var player = Main.LocalPlayer;
        var bodySlot = player.armor[10].type == Item.type ? 10 : -1;
        var isInVanitySlot = false;

        if (player.armor[11].type == Item.type)
        {
            isInVanitySlot = true;
        }

        if (isInVanitySlot)
        {
            return;
        }

        var text =
            $"+{LifeBoost} max life\n" +
            $"+{DamageBoost * 100:F0}% to all damage\n" +
            $"+{CritBoost}% crit chance";

        var line = new TooltipLine(Mod, "AwakenedBloodPlate", text);

        // Insert before vanilla armor tooltips (which have Mod == "Terraria" and Name == "Tooltip#")
        var insertIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));

        if (insertIndex == -1)
        {
            tooltips.Add(line);
        }
        else
        {
            tooltips.Insert(insertIndex, line);
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<OmegaBlueChestplate>()
            .AddIngredient<BloodflareBodyArmor>()
            .AddCondition(conditions: Condition.BloodMoon)
            .AddIngredient<UmbralLeechDropItem>(5)
            .AddIngredient<ShellFragmentItem>(7)
            .AddIngredient<YharonSoulFragment>(20)
            .AddTile<CosmicAnvil>()
            .Register();
    }
}