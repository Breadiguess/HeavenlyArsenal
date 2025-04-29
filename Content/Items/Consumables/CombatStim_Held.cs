using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Misc
{
    class CombatStim_Held : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0]; // time of this projectile
        public ref float HasUsed => ref Projectile.ai[1]; // If the syringe should be held in the hand, or stuck in the player.

        public bool UsedStim => HasUsed != 0; // If the stim has been used or not. false = not used, true = used.
        public ref float InjectionX => ref Projectile.localAI[0];
        public ref float InjectionY => ref Projectile.localAI[1];

        public Vector2 InjectionPosition => new Vector2(InjectionX, InjectionY); //stores the location of the stim on the player so that the injection can be drawn on the player in the correct place.
        public override string Texture => "HeavenlyArsenal/Content/Items/Consumables/CombatStim";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.damage = -1;
            Projectile.velocity = Vector2.Zero;

            Projectile.timeLeft = 600;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void AI()
        {
            //todo: make it stay in the hand until the player uses it
            if (!UsedStim)
            {
                Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

                Projectile.Center = armPosition;
            }
            else
            {
                // Logic for when the stim has been used
            }
            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            
            return base.PreDraw(ref lightColor);
        }
    }
}
