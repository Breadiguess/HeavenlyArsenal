using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

[AutoloadEquip(EquipType.Head)]

public class AwakenedBloodHelm : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor";

    internal static string TentacleEntitySourceContext => "SetBonus_HeavenlyArsenal_BloodArmor";


    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodHelmDefense_Head", EquipType.Head, name: "AwakenedBloodHelmDefense");
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
        Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
        Item.defense = 49; //85
        Item.rare = ModContent.RarityType<PureGreen>();
    }
    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<AwakenedBloodplate>() && legs.type == ModContent.ItemType<AwakenedBloodStrides>();
    

    public override void ArmorSetShadows(Player player)
    {
        player.armorEffectDrawShadowSubtle = false;
    }

    public override void UpdateArmorSet(Player player)
    {
        //player.setBonus = this.GetLocalizedValue("SetBonus") + "\n";
        player.setBonus = Language.GetOrRegister(Mod.GetLocalizationKey("Items.Armor.AwakenedBloodHelm.SetBonus")).Value;
        
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

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<MeleeDamageClass>() += 0.1f;
        player.GetCritChance<MeleeDamageClass>() += 5;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<BloodstoneCore>(11).
            AddIngredient<RuinousSoul>(2).
            AddTile(TileID.LunarCraftingStation).
            Register();
    }
}
