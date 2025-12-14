using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace HeavenlyArsenal.Content.Items.Materials;

public class Auric_Catalyst : ModItem
{
    public override string LocalizationCategory => "Items.Misc.Materials";

    public override void SetStaticDefaults()
    {
        //DisplayName.SetDefault("ITEM NAME");
        //Tooltip.SetDefault("'TOOLTIP.'");
    }

    public override void SetDefaults()
    {
        Item.width = 60;
        Item.height = 60;
        Item.maxStack = 1;
        Item.value = 9999999;
        Item.rare = ItemRarityID.LightPurple;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddTile<CosmicAnvil>()
            .AddIngredient(ModContent.ItemType<AuricBar>(), 5)
            .
            //AddIngredient(ItemType<>).
            Register();
    }
}