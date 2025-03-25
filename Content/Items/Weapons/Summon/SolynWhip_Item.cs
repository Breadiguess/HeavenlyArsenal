using HeavenlyArsenal.Content.Projectiles.Weapons.Summon;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Textures;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon
{
    public class SolynWhip_Item : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            // This method quickly sets the whip's properties.
            // Mouse over to see its parameters.
            Item.DefaultToWhip(ModContent.ProjectileType<SolynWhip_Projectile>(), 2040, 2, 4);
            Item.damage = 40000;
            Item.autoReuse = true;
            Item.shootSpeed = 4;
            Item.rare = ItemRarityID.Green;

            Item.channel = true;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            //CreateRecipe()
            //    .AddIngredient<ExampleItem>()
            //    .AddTile<Tiles.Furniture.ExampleWorkbench>()
            //    .Register();
        }

        // Makes the whip receive melee prefixes
        public override bool MeleePrefix()
        {
            return true;
        }
    }
}