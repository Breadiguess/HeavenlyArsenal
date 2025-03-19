using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;

public class AvatarRifleSuperBullet : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public bool hasEmpowerment;
    public int empowerment;

    public override bool PreAI(Projectile projectile)
    {
        int t = Main.projectile[0].type;

        if (hasEmpowerment)
        {

        }
        return base.PreAI(projectile);
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (hasEmpowerment)
        {

        }
    }
}
