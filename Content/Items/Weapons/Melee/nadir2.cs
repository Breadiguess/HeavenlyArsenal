using CalamityMod.Rarities;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.Nadir2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee
{
    internal class Nadir2 : ModItem
    {
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<HotPink>();

            Item.damage = 10000;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 40;
            Item.reuseDelay = 40;

            ;
            Item.useAnimation = 0;
            Item.noUseGraphic = false;
            Item.useTurn = true;
            Item.channel = true;


            //Item.useStyle = 3;
            Item.knockBack = 6;


            Item.autoReuse = true;


            Item.ChangePlayerDirectionOnShoot = true;
            Item.noMelee = false;
        }


        public const int ShootDelay = 32;

        public const int ArrowsPerBurst = 5;

        public const int AttackRate = 4;

        public const int AttackDuration = 40;

        public const int MaxChargeTime = 120;


        public static readonly SoundStyle ChargeSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_charge3");

        public static readonly SoundStyle FireSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire");

        public static readonly SoundStyle FullyChargedSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_FullyCharged");



        private bool spearOut = false;

        public override void HoldItem(Player player)
        {
            // Check if the spear projectile already exists
            bool projectileExists = false;

            foreach (Projectile projectile in Main.projectile)
            {
                // Check if the projectile is active, matches the spear type, and is owned by the player
                if (projectile.active && projectile.type == ModContent.ProjectileType<nadir2_Holdout>() && projectile.owner == player.whoAmI)
                {
                    projectileExists = true;
                    break; // Stop checking if a matching projectile is found
                }
            }

            if (!spearOut && !projectileExists)
            {
                // Spawn the projectile
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center.X, player.Center.Y, 0f, 0f, ModContent.ProjectileType<nadir2_Holdout>(), Item.damage, Item.knockBack, player.whoAmI);
                spearOut = true; // Set the flag to true to prevent further spawns
            }
        }

        public override void UpdateInventory(Player player)
        {
            // Check if the item is no longer being held or in the inventory
            if (player.HeldItem.type != Item.type)
            {
                spearOut = false;

                // Find the spear projectile and kill it
                foreach (Projectile projectile in Main.projectile)
                {
                    if (projectile.active && projectile.type == ModContent.ProjectileType<nadir2_Holdout>() && projectile.owner == player.whoAmI)
                    {
                        projectile.Kill(); // Despawn the spear projectile
                        break; // Only kill the first matching projectile
                    }
                }
            }
        }

    }
}
