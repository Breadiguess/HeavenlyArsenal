using System.Collections.Generic;
using CalamityMod.Items;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

[AutoloadEquip(EquipType.Legs)]
public class AwakenedBloodStrides : ModItem, ILocalizedModType
{
    private readonly float MoveSpeedBoost = 0.17f;

    private readonly float DamageBoost = 0.10f;

    private readonly int CritBoost = 7;

    public override string LocalizationCategory => "Items.Armor.AwakenedBloodArmor";

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
        Item.defense = 43;
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
    }

    public override void SetStaticDefaults()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
        }
    }

    public override void UpdateEquip(Player player)
    {
        player.moveSpeed += MoveSpeedBoost;
        player.GetDamage<GenericDamageClass>() += DamageBoost;
        player.GetCritChance<GenericDamageClass>() += CritBoost;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var player = Main.LocalPlayer;
        var bodySlot = player.armor[10].type == Item.type ? 10 : -1;

        var isInVanitySlot = false;

        if (player.armor[12].type == Item.type)
        {
            isInVanitySlot = true;
        }

        if (isInVanitySlot)
        {
            return;
        }

        var text =
            $"+{MoveSpeedBoost * 100:F0}% movement speed\n" +
            $"+{DamageBoost * 100:F0}% all damage\n" +
            $"+{CritBoost}% crit chance";

        // create and add it
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
            .AddIngredient<OmegaBlueTentacles>()
            .AddIngredient<BloodflareCuisses>()
            .AddCondition(conditions: Condition.BloodMoon)
            .AddIngredient<YharonSoulFragment>(15)
            .AddIngredient<PenumbralMembrane>(4)
            .AddTile<CosmicAnvil>()
            .Register();
    }
}