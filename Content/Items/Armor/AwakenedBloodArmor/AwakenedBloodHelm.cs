using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

[AutoloadEquip(EquipType.Head)]
public class AwakenedBloodHelm : ModItem, ILocalizedModType
{
    private readonly float DamageBoost = 0.1f;

    private readonly int CritBoost = 5;

    public override string LocalizationCategory => "Items.Armor.AwakenedBloodArmor";

    internal static string TentacleEntitySourceContext => "SetBonus_HeavenlyArsenal_BloodArmor";

    public override void Load() { }

    public override void SetStaticDefaults()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
        Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
        Item.defense = 49;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<AwakenedBloodplate>() && legs.type == ModContent.ItemType<AwakenedBloodStrides>();
    }

    public override void ArmorSetShadows(Player player)
    {
        player.armorEffectDrawShadowSubtle = false;
    }

    public override void UpdateArmorSet(Player player)
    {
        //player.setBonus = this.GetLocalizedValue("SetBonus") + "\n";
        player.setBonus = Language.GetOrRegister("Items.Armor.AwakenedBloodArmor.AwakenedBloodHelm.SetBonus").Value;
        player.setBonus = this.GetLocalizedValue("SetBonus");
        var modPlayer = player.Calamity();
        player.GetAttackSpeed<MeleeDamageClass>() += 0.18f;
        modPlayer.bloodflareSet = true;

        //player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped = true;
        player.GetModPlayer<AwakenedBloodPlayer>().AwakenedBloodSetActive = true;
        player.crimsonRegen = true;
        player.aggro += 900;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<GenericDamageClass>() += DamageBoost;
        player.GetCritChance<MeleeDamageClass>() += CritBoost;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var player = Main.LocalPlayer;
        var bodySlot = player.armor[10].type == Item.type ? 10 : -1;

        var isInVanitySlot = false;

        if (player.armor[10].type == Item.type)
        {
            isInVanitySlot = true;
        }

        if (isInVanitySlot)
        {
            return;
        }

        var text =
            $"+{DamageBoost * 100:F0}% to all damage\n" +
            $"+{CritBoost}% crit chance";

        var line = new TooltipLine(Mod, "AwakenedBloodHelm", text);

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
            .AddIngredient<OmegaBlueHelmet>()
            .AddRecipeGroup("HeavenlyArsenal:BloodflareHelmets")
            .AddIngredient<YharonSoulFragment>(15)
            .AddIngredient<UmbralLeechDrop>(3)
            .AddIngredient<PenumbralMembrane>(2)
            .AddCondition(conditions: Condition.EclipseOrBloodMoon)
            .AddTile<CosmicAnvil>()
            .Register();
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
        var group = new RecipeGroup
        (
            () => $"{Language.GetTextValue("LegacyMisc.37")} {BloodflareHelmetGroupText.Value}",
            ModContent.ItemType<BloodflareHeadMagic>(),
            ModContent.ItemType<BloodflareHeadMelee>(),
            ModContent.ItemType<BloodflareHeadRanged>(),
            ModContent.ItemType<BloodflareHeadRogue>(),
            ModContent.ItemType<BloodflareHeadSummon>()
        );

        RecipeGroup.RegisterGroup("HeavenlyArsenal:BloodflareHelmets", group);
    }
}