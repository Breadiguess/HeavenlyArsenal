using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

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
            if (!NightfallActive || DamageBucketMax <= 0)
                return;

            float baseCrit = crit;
            float interpolant = Math.Clamp((float)DamageBucketTotal / DamageBucketMax, 0f, 1f);

            // Calculate a speed bias (slower weapons benefit more)
            float speedBias = MathF.Pow(Math.Clamp(item.useTime / 30f, 0.4f, 2.0f), 0.75f);
            // High-damage weapons gain slightly less (to prevent stacking with armor multipliers)
            float dmgBias = 1f / MathF.Pow(Math.Clamp(item.damage / 100f, 0.6f, 2f), 0.5f);

            float baseBonus = 10f;

            float bonus = interpolant * baseBonus * speedBias * dmgBias;

            bonus = 20f * (1f - MathF.Exp(-bonus / 30f));

            crit += bonus;
            CritModifier = (int)crit;
            //Main.NewText($"{item.Name}: base = {baseCrit:F1}, speed = {item.useTime}, damage = {item.damage}, bonus = {bonus:F1}, total = {crit:F1}");

        }

        public override void PostUpdateMiscEffects()
        {
            //todo: for each npc, is active, and has a stack (NightfallNPC stack), get its damage bucket
            // add the value inside of the damagebucket total

            if (HitCooldown > 0)
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

            if (victim is NPC npc)
            {
                NightfallNPC a = npc.GetGlobalNPC<NightfallNPC>();
                if (a.Stack >= MaxStack || a.BurstCooldown > 0)
                {
                    return;
                }
                if (a.StackOwner != Player)
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
            if (!NightfallActive)
                base.OnHitNPC(target, hit, damageDone);

            if (target.active && !target.friendly && !target.dontTakeDamage && !target.immortal)
            {
                target.GetGlobalNPC<NightfallNPC>().DamageBucketNPC += damageDone;

                target.GetGlobalNPC<NightfallNPC>().BucketLossTimer = 120;
            }


        }

    }
}
