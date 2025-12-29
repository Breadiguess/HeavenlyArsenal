using CalamityMod;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    internal class Rogue_BloodSlashes : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.Calamity().stealthStrike = true;
            
        }
    }
}
