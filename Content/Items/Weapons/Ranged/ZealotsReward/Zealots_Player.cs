using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_Player : ModPlayer
    {
        public bool CanUseAltFire = true;

        public float UseSpeedMulti;

        public int hitCount = 0;

        public int AltFireCooldown;
        public const int MAXCOOLDOWN = 60 * 7;
        public override void UpdateBadLifeRegen()
        {
            if (AltFireCooldown > 0)
                AltFireCooldown--;

                CanUseAltFire = AltFireCooldown <= 0;
        }


        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(proj.type != ModContent.ProjectileType<Zealots_BoltProj>())
            {
                return;
            }

            
        }

        public bool Active => Player.HeldItem.type == ModContent.ItemType<Zealots_Item>();
        public override void PostUpdateMiscEffects()
        {
            if (Active)
            {
                UseSpeedMulti = float.Lerp(UseSpeedMulti, 1, 0.065f);
            }
        }

    }
}
