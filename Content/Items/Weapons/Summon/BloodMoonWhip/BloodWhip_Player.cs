using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    public class BloodWhip_Player : ModPlayer
    {
        public int MaxBlood = 300;

        public int blood;

        public int DecayTime;

        public override void PostUpdateMiscEffects()
        {
            DecayTime++;

            if (Player.HeldItem.type == ModContent.ItemType<BloodySting_Item>())
            {
                DecayTime = 0;
            }

            if (DecayTime > 70)
            {
                blood--;

                if (DecayTime > 120)
                {
                    blood--;
                }
            }

            blood = Math.Min(blood, MaxBlood);

            if (Math.Sign(blood) < 1)
            {
                blood = 0;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPCWithProj(proj, target, hit, damageDone);

            if (proj.IsMinionOrSentryRelated && Player.HeldItem.type == ModContent.ItemType<BloodySting_Item>())
            {
                var availableSlots = Player.maxMinions - Player.numMinions;
                //Main.NewText($"MaxMinions: {Player.maxMinions}, num : {Player.slotsMinions}");
                var bloodToGain = Math.Max(1, availableSlots);
                // Main.NewText(bloodToGain);
                blood += bloodToGain;
            }
        }
    }
}
