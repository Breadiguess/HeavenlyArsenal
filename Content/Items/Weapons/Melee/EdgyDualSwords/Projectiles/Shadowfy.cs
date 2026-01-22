using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles
{
    public class ShadowfyDash : BaseSwordProjectile
    {
        private bool hasMoved;

        protected override void HandleMovement()
        {
            if (!hasMoved)
            {
                Vector2 dashDir =
                    (Owner.Center - Main.MouseWorld).SafeNormalize(Vector2.UnitX);

                Owner.velocity = dashDir * Attack.DashSpeed;
                Owner.immuneTime = Attack.GrantsIFrames ? 12 : 0;

                hasMoved = true;
            }

            Projectile.Center = Owner.Center;
        }

        public override bool? CanDamage()
            => false; // This dash itself doesn't hit
    }

}
