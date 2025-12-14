using CalamityMod.Items.Accessories;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Projectiles.Misc;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Misc;

internal class ChaliceOfFun : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.rare = ModContent.RarityType<HotPink>();
        Item.width = 40;
        Item.height = 32;
        Item.useTime = 40;
        Item.reuseDelay = 40;
        Item.DamageType = DamageClass.Melee;
        Item.useAnimation = 1;
        Item.useTurn = true;
        Item.channel = true;
        Item.knockBack = 3;
        //Item.autoReuse = true;
        Item.ChangePlayerDirectionOnShoot = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<AncientCoin>(3)
            .AddIngredient<ChaliceOfTheBloodGod>()
            .AddTile<DraedonsForge>()
            .Register();
    }

    public override void HoldItem(Player player)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<ChaliceOfFunHoldout>()] <= 0)
        {
            if (player.altFunctionUse == 0)
            {
                Projectile.NewProjectileDirect
                    (player.GetSource_ItemUse(Item), player.position, player.velocity, ModContent.ProjectileType<ChaliceOfFunHoldout>(), Item.damage, Item.knockBack, player.whoAmI);
            }
        }
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return true;
    }
}