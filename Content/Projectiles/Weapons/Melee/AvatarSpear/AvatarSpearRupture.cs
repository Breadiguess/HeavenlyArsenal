using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarSpearRupture : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 100;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults()
    {
    }

    public ref float Time => ref Projectile.ai[0];

    public int Target => (int)(Projectile.ai[1] - 1);

    public override void AI()
    {
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
