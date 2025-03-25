using NoxusBoss.Content.Rarities;
using HeavenlyArsenal.Content.Projectiles;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using NoxusBoss.Assets.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI.Chat;
using System.Collections.Generic;
using NoxusBoss.Content.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using NoxusBoss.Content.Tiles;
using CalamityMod;
using static NoxusBoss.Assets.GennedAssets.Sounds;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;


namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    internal class AvatarRifle : ModItem
    {
        public const int ShootDelay = 32;

        public const int BulletsPerShot = 1;

        public static int RPM = 20;

        public const int CycleTimeDelay = 40;
        public const int CycleTime = 120;

        public const int ReloadTime = 360;
        

        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/HeavenlyGaleFire");

        public static readonly SoundStyle LightningStrikeSound = new("CalamityMod/Sounds/Custom/HeavenlyGaleLightningStrike");

        //public static int AmmoType = 
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();

            Item.damage = 9600;
            Item.DamageType = DamageClass.Ranged;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 45;
            Item.reuseDelay = 45;


            
            Item.useAmmo = AmmoID.Bullet;

            //Item.consumeAmmoOnFirstShotOnly = true;
            Item.useAnimation = 5;

            
            Item.noUseGraphic = true;

            Item.useTurn = true;
            Item.channel = true;


            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
           
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            //Item.shoot = ModContent.ProjectileType<FusionRifle_Projectile>();
            //Item.shoot = ProjectileID<t>
            Item.shoot = AmmoID.Bullet;
            Item.ChangePlayerDirectionOnShoot = true;
            Item.crit = 87;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
        }


        public override bool CanShoot(Player player)
        {
            return false;
        }

        private bool AvatarRifle_Out = false;
        public override void HoldItem(Player player)
        {
            if (!AvatarRifle_Out)
            {
                // Check if there is already a projectile of this type owned by the player
                bool projectileExists = false;
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<AvatarRifle_Holdout>())
                    {
                        projectileExists = true;
                        break;
                    }
                }

                if (!projectileExists)
                {
                    // Spawn the projectile
                    Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item), // Source of the projectile
                        player.Center.X,               // X coordinate of the spawn location
                        player.Center.Y,               // Y coordinate of the spawn location
                        0f, 0f,                        // Velocity (set to 0 for stationary)
                        ModContent.ProjectileType<AvatarRifle_Holdout>(), // Type of the projectile
                        Item.damage,                   // Damage of the projectile
                        Item.knockBack,                // Knockback of the projectile
                        player.whoAmI                  // Owner of the projectile
                    );
                    AvatarRifle_Out = true; // Set the flag to true to prevent further spawns
                }
            }
        }


        public override void UpdateInventory(Player player)
        {
            // Reset the flag if the item is not being held
            if (player.HeldItem.type != Item.type)
            {
                AvatarRifle_Out = false;
            }
        }
    }
}
