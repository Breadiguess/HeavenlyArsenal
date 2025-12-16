using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace HeavenlyArsenal.Content.Items.Materials;

public class AuricCatalystItem : ModItem
{
    public override string LocalizationCategory => "Items.Misc.Materials";

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.maxStack = 1;

        Item.width = 92;
        Item.height = 34;

        // TODO: We may want to adjust the price.
        Item.value = 9999999;

        Item.rare = ItemRarityID.LightPurple;
    }

    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
            .AddTile<CosmicAnvil>()
            .AddIngredient<AuricBar>(5)
            .Register();
    }
}