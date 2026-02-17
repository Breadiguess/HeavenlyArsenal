using CalamityMod.Items.Accessories;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Projectiles.Misc;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Misc;

public class ChaliceOfFunItem : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.ChangePlayerDirectionOnShoot = true;
        Item.noUseGraphic = true;
        Item.useTurn = true;
        Item.channel = true;
        
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 3f;
        
        Item.noMelee = true;
        
        Item.width = 40;
        Item.height = 32;
        
        Item.useTime = 40;
        Item.reuseDelay = 40;
        Item.useAnimation = 1;
        
        Item.rare = ModContent.RarityType<HotPink>();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<AncientCoin>(3)
            .AddIngredient<ChaliceOfTheBloodGod>()
            .AddTile<DraedonsForge>()
            .Register();
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
    
    public override void HoldItem(Player player)
    {
        base.HoldItem(player);
        
        if (player.ownedProjectileCounts[ModContent.ProjectileType<ChaliceOfFunProjectile>()] > 0 || player.altFunctionUse != ItemAlternativeFunctionID.None)
        {
            return;
        }

        Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.position, player.velocity, ModContent.ProjectileType<ChaliceOfFunProjectile>(), Item.damage, Item.knockBack, player.whoAmI);
    }
}