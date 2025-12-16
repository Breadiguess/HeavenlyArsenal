using HeavenlyArsenal.Content.Tiles;
using Terraria.Enums;

namespace HeavenlyArsenal.Content.Items.Banners;

public class UmbralLeechBannerItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBannerTile>(), (int)EnemyBannerTile.EnemyStyle.UmbralLeech);
        
        Item.width = 10;
        Item.height = 24;
        
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
    }
}