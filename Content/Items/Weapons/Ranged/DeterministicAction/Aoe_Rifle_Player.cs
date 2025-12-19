using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_Player : ModPlayer
    {

        //hitting shots with rifle builds authority
        // spend authority to ???

        // WOUND THE WORLD, TEAR DOWN THEIR PRECIOUS STARS AND SNUFF THE LIGHT FROM THEIR EYES
        public const int MAX_AUTHORITY_TIMER = 60 * 7;
        public bool Active
        {
            get => Player.HeldItem.type == ModContent.ItemType<Aoe_Rifle_Item>();
        }
        /// <summary>
        /// Resource
        /// </summary>
        public int Authority { get; set; }

        /// <summary>
        /// timer for when authority starts to diminish
        /// </summary>
        public int AuthorityTimer { get; set; }

        public override void PostUpdateMiscEffects()
        {
            if (AuthorityTimer > 0)
                AuthorityTimer--;
            if(AuthorityTimer <= 0 && Authority > 0)
            {
                Authority--;
                AuthorityTimer = MAX_AUTHORITY_TIMER;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(proj.type == ModContent.ProjectileType<Aoe_Rifle_Laser>() && Active)
            {
                Authority++;
                AuthorityTimer = MAX_AUTHORITY_TIMER;
            }
            
        }
    }
}
