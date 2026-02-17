using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Materials;

public class ShadowspecGunPartsItem : ModItem
{
    public override string LocalizationCategory => "Items.Misc.Materials";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = false;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 8));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.maxStack = 1;

        Item.width = 38;
        Item.height = 38;

        // TODO: We may want to adjust the price.
        Item.value = 9999999;

        Item.rare = ModContent.RarityType<HotPink>();
    }

    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
            .AddTile<DraedonsForge>()
            .AddIngredient<ShadowspecBar>(4)
            .Register();
    }
}