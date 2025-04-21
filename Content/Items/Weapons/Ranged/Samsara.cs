using CalamityMod;
using CalamityMod.Projectiles.Ranged;
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items.SummonItems;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    class Samsara : ModItem
    {
        public override void SetDefaults()
        {
            Item.useAmmo = AmmoID.Dart;
            Item.shoot = ModContent.ProjectileType<PossibilitySeed>();
            Item.shootSpeed = 20f;
            Item.width = 20;
            Item.height = 20;
            Item.useStyle = 5;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 1;
            Item.crit = 20;
            Item.knockBack = 5f;
            Item.value = 10000;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();
            Item.UseSound = SoundID.Item64;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.noMelee = true;

            

            Item.Calamity().devItem = true;

        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 25;

        public override Vector2? HoldoutOffset() => new Vector2(-5, 0);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (type == ProjectileID.Seed || type == ProjectileID.PoisonDart || type == ProjectileID.IchorDart || type == ProjectileID.CrystalDart || type == ProjectileID.CursedDart)
                Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ModContent.ProjectileType<PossibilitySeed>(), damage, knockback, player.whoAmI);
           

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SeedOfWill>(1).
                
                Register();
        }




    }
}
