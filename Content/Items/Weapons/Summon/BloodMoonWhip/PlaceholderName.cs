using System;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    public class PlaceholderName : ModPlayer
    {
        public int MaxBlood = 300;
        public int blood;
        public int DecayTime;
        public override void PostUpdateMiscEffects()
        {
            DecayTime++;
            if (Player.HeldItem.type == ModContent.ItemType<ViscousWhip_Item>())
                DecayTime = 0;

            
            if (DecayTime > 70)
            {
                blood--;
                if (DecayTime > 120)
                    blood--;
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

            if (proj.IsMinionOrSentryRelated && Player.HeldItem.type == ModContent.ItemType<ViscousWhip_Item>())
            {
                int availableSlots = Player.maxMinions - Player.numMinions;
                //Main.NewText($"MaxMinions: {Player.maxMinions}, num : {Player.slotsMinions}");
                int bloodToGain = Math.Max(1, availableSlots);
               // Main.NewText(bloodToGain);
                blood += bloodToGain;

            }
        }
    }
}
