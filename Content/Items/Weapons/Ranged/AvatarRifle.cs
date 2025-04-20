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
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged.AvatarRifleProj;
using Terraria.DataStructures;


namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    class AvatarRifle : ModItem
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
            Item.useAnimation = 5;
            Item.noUseGraphic = true;
            Item.useTurn = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
           
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AvatarRifle_Holdout>();
            //Item.shoot = AmmoID.Bullet;
            Item.ChangePlayerDirectionOnShoot = true;
            Item.crit = 87;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

        private bool AvatarRifle_Out(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!AvatarRifle_Out(player))
                {
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);

                }
            }
        }


        public override void UpdateInventory(Player player)
        {

        }
    }
}
