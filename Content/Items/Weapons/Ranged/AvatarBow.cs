using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged.FusionRifleProj;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    class AvatarBow : ModItem
    {

        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public const int ShootDelay = 32;

        public const int ArrowsPerBurst = 10;

        public const int ArrowShootRate = 4;

        public const int ArrowShootTime = ArrowsPerBurst * ArrowShootRate;

        public const int MaxChargeTime = 300;

        public const float ArrowTargetingRange = 1100f;

        public const float MaxChargeDamageBoost = 3.5f;

        public const float LightningDamageFactor = 0.36f;

        public const float ChargeLightningCreationThreshold = 0.8f;

        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/HeavenlyGaleFire");

        public static readonly SoundStyle LightningStrikeSound = new("CalamityMod/Sounds/Custom/HeavenlyGaleLightningStrike");
        public override void SetDefaults()
        {
           Item.DefaultToBow(Item.width, Item.height);
           Item.damage = 100;
           Item.DamageType = DamageClass.Ranged;
           Item.shootSpeed = 20;
           Item.useAmmo = AmmoID.Arrow;
           Item.useAnimation = 4;
           Item.useTime = 10;
           Item.shoot = ModContent.ProjectileType<AvatarBow_Held>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override bool CanShoot(Player player) => false;
        private bool BowOut(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;
      
        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!BowOut(player))
                {
                    Projectile bow = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                    //spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
                }
            }
        }

    }
}
