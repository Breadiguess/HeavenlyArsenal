using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage
{
    internal class BloodRune_Projectile : ModProjectile
    {
        public struct SpellSnapshot
        {
            int projectileType;
            int damage;
            float knockback;
            Vector2 velocity;
        }
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;

        public ref Player Owner => ref Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.Size = new Vector2(60, 60);

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }
    }
}
