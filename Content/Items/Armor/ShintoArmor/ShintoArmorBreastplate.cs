using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials;
using HeavenlyArsenal.Utilities.Extensions;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

[AutoloadEquip(EquipType.Body)]
public class ShintoArmorBreastplate : ModItem
{
    // public new string LocalizationCategory => "Items.Armor";
    // public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease, MaxLifeIncrease);

    // gonna keep it real chief, this chestplate code is a mess lmao.

    public override string LocalizationCategory => "Items.Armor.ShintoArmor";

    public override void SetStaticDefaults()
    {
        var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 22;
        Item.value = Item.sellPrice(gold: 4445);
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.defense = 63;
        Item.lifeRegen += 3;
    }

    public override void UpdateEquip(Player player)
    {
        var modPlayer = player.Calamity();

        player.statLifeMax2 += MaxLifeIncrease;
        player.statManaMax2 += MaxManaIncrease;
        player.GetDamage<GenericDamageClass>() += 0.15f;
        player.GetCritChance<GenericDamageClass>() += 18;
        player.GetAttackSpeed<GenericDamageClass>() += 0.25f;
        player.GetModPlayer<ShintoArmorPlayer>().ChestplateEquipped = true;
    }

    public override void UpdateVanity(Player player)
    {
        player.GetModPlayer<ShintoArmorPlayer>().ChestplateEquipped = true;
    }

    public override void Load()
    {
        EquipLoader.AddEquipTexture(Mod, Texture + "_Waist", EquipType.Waist, this);
    }

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        if (player.body == Item.bodySlot)
        {
            player.waist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);
            player.cWaist = player.cBody;
        }
    }

    public override void AddRecipes()
    {
        var recipe =
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient(ModContent.ItemType<StatigelArmor>());

        RecipeExtensions.TryAddIngredient(recipe, "CalamityHunt", "ShogunChestplate");

        recipe.AddIngredient<TheSponge>()
            .AddTile<DraedonsForge>()
            .AddCustomShimmerResult(ModContent.ItemType<ShintoArmorWings>());

        recipe.Register();
    }

    #region static values

    public static int BarrierCooldown = 18 * 60;

    public static int ShieldDurabilityMax = 300;

    public static int ShieldRechargeDelay = 725;

    public static int ShieldRechargeRate = 20;

    public static int TotalShieldRechargeTime = 575;

    public static Texture2D NoiseTex = GennedAssets.Textures.Noise.TurbulentNoise;

    public static Texture2D GFB = GennedAssets.Textures.Extra.Ogscule;

    private static readonly int MaxManaIncrease = 200;

    private static readonly int MaxLifeIncrease = 300;

    #endregion
}