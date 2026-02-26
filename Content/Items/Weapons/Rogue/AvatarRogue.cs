using CalamityMod;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue
{
    internal class AvatarRogue : ModItem
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.shoot = ModContent.ProjectileType<AvatarRogue_Projectile>();
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = 30;
            Item.useAnimation = 30;
        }

        public override bool CanShoot(Player player) => player.ownedProjectileCounts[Item.shoot] < 7;


    }
}
