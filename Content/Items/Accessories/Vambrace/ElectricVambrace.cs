using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace;

public class ElectricVambrace : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public override void SetDefaults()
    {
        Item.width = 54;
        Item.height = 56;
        Item.defense = 8;
        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ModContent.RarityType<HotPink>();
        Item.accessory = true;
    }

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<DischargePlayer>().Active = true;
        var modPlayer = player.Calamity();
        modPlayer.transformer = true;
        modPlayer.aSpark = true;

        //modPlayer.DashID = ElectricVambraceDash.ID;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<TheTransformer>()
            .AddIngredient<AmidiasSpark>()
            .AddIngredient<SlagsplitterPauldron>()
            .AddIngredient<LeviathanAmbergris>()
            .AddIngredient<AscendantSpiritEssence>(8)
            .AddTile<CosmicAnvil>()
            .AddCondition(Condition.NearWater)
            .Register();
    }
}

public class ElectricVambracePlayer : ModPlayer
{
    public bool HasReducedDashFirstFrame { get; private set; }

    internal bool Active;

    public override void Load() { }

    public override void PostUpdateMiscEffects()
    {
        if (Active) { }
    }
}