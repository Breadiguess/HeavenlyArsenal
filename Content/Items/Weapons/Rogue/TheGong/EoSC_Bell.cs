using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.TheGong
{
    internal class EoSC_Bell : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
    }
}
