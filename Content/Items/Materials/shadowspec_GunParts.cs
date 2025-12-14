using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.DataStructures;
//using CalamityMod.Items.Placeables.Furniture.CraftingStations;

namespace HeavenlyArsenal.Content.Items.Materials;

internal class shadowspec_GunParts : ModItem
{
    public override string LocalizationCategory => "Items.Misc.Materials";

    public override void SetStaticDefaults()
    {
        //DisplayName.SetDefault("ITEM NAME");
        //Tooltip.SetDefault("'TOOLTIP.'");
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.AnimatesAsSoul[Item.type] = true;

        ItemID.Sets.ItemNoGravity[Item.type] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 60;
        Item.height = 60;
        Item.maxStack = 1;
        Item.value = 9999999;
        Item.rare = ModContent.RarityType<HotPink>();
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, 5);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddTile<DraedonsForge>()
            .AddIngredient(ModContent.ItemType<ShadowspecBar>(), 4)
            .
            //AddIngredient(ItemType<>).
            Register();
    }
}