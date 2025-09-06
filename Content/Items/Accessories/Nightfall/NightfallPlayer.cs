using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Cooldowns;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace HeavenlyArsenal.Content.Items.Accessories.Nightfall
{
    internal class NightfallPlayer : ModPlayer
    {

        
        public bool NightfallActive;

        public static int CooldownMax = 4 * 60;
                
        public static int MaxStack = 9;

        public int HitCooldownMax = 20;
        public int HitCooldown = 0;

        /// <summary>
        /// integer representing all the damage you've dealt recently while Nightfall is active
        /// </summary>
        public int DamageBucketTotal;
        /// <summary>
        /// the total amount of damage you can store in the damage bucket before it stops accumulating
        /// </summary>
        public static int DamageBucketMax = 10_000;

        public int CritModifier = 0;


        public override void ResetEffects()
        {

            DamageBucketTotal = 0;
            NightfallActive = false;
            
        }
        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
           
            // Create an interpolant out of damage bucket total / damagebucketmax
            // Increase the crit chance of the player based on that interpolant x 100
            if (NightfallActive && DamageBucketMax > 0)
            {
                
              
                float interpolant = Math.Clamp((float)DamageBucketTotal / DamageBucketMax, 0f, 1f);
                //bruh
                /*
                if (item.crit > 0)
                {

                    crit += (1 + interpolant) * item.crit;
                    CritModifier = (int)crit;
                    Main.NewText($" {CritModifier}");
                }
                */

                if(crit > 0)
                {
                    //todo: factor in item attack speed/whatever to help further balance this crit chance increase;
                    //faster attacking weapons should get less crit chance, and slower weapons should get more crit chance.
                    //
                    crit += (1 + interpolant) * 25;
                    CritModifier = (int)crit;

                    Main.NewText($" {CritModifier}");
                }
            }
        }
        public override void PostUpdateMiscEffects()
        {
            //todo: for each npc, is active, and has a stack (NightfallNPC stack), get its damage bucket
            // add the value inside of the damagebucket total
            
            if(HitCooldown > 0)
            {
                HitCooldown--;
            }

            foreach (NPC npc in Main.ActiveNPCs)
            {
                NightfallNPC a = npc.GetGlobalNPC<NightfallNPC>();
                if (npc.active)
                {
                    if (a.DamageBucketNPC != 0 && a.Stack > 0 && a.StackOwner == Player)
                        DamageBucketTotal += a.DamageBucketNPC;
                    else
                        continue;
                }
            }

            
            if (DamageBucketTotal > DamageBucketMax)
                DamageBucketTotal = DamageBucketMax;
            
                //Main.NewText($"{DamageBucketTotal}");
        }

        public override void OnHitAnything(float x, float y, Entity victim)
        {
            if (!NightfallActive || HitCooldown > 0 || victim is Player ba)
                return;
            
            if(victim is NPC npc)
            {
                NightfallNPC a = npc.GetGlobalNPC<NightfallNPC>();
                if (a.Stack >= MaxStack || a.BurstCooldown > 0)
                {
                    return;
                }
                if(a.StackOwner != Player)
                {
                    a.StackTimer = 0;
                    a.Stack = 0;
                    a.OrbitInterp = 1;
                    a.WindupInterp = 0;
                }
                a.StackOwner = Player;
                a.StackTimer = 300;
                a.Stack++;
                SoundEngine.PlaySound(AssetDirectory.Sounds.Nightfall.Hit with { Pitch = -0.5f + 0.1f * a.Stack, Volume = 0.5f + 0.05f * a.Stack });

            }
            HitCooldown = HitCooldownMax;


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(!NightfallActive)
                base.OnHitNPC(target, hit, damageDone);

            if (target.active && !target.friendly && !target.dontTakeDamage && !target.immortal)
            {
                target.GetGlobalNPC<NightfallNPC>().DamageBucketNPC += damageDone;

                target.GetGlobalNPC<NightfallNPC>().BucketLossTimer = 120;
            }


        }

    }
}
