using System.Linq;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Summon;
using HeavenlyArsenal.Content.Projectiles.Weapons;

namespace HeavenlyArsenal.Content.Items
{
    public class avatar_FishingRod : ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/avatar_FishingRod";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.DamageType = DamageClass.Summon;
            Item.damage = 4445;
            Item.knockBack = 3f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.autoReuse = true;

            Item.holdStyle = 0; // Custom hold style
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<avatar_FishingRodProjectile>();
            Item.shootSpeed = 10f;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();
            Item.value = Item.buyPrice(gold: 2);
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Coral, 2)
                .AddTile<GardenFountainTile>()
                .Register();
        }
    }
}
