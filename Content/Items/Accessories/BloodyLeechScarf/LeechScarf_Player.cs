using CalamityMod;
using CalamityMod.Cooldowns;
using HeavenlyArsenal.Common.Ui.Cooldowns;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    internal class LeechScarf_Player : ModPlayer
    {
        public bool Active { get; set; }

        public readonly int Type = ModContent.ProjectileType<LeechScarf_TendrilProjectile>();

        #region tendrilStruct
        public IReadOnlyList<Tendril> Tendrils => TendrilList;

        public const int MAX_TENDRIL_COOLDOWN = 60 * 12;
        public const int MAX_TENDRILS = 3;
        public readonly int BaseDamage = 200;
        public List<Tendril> TendrilList = new List<Tendril>(MAX_TENDRILS);
        public struct Tendril
        {
            /// <summary>
            /// the projectile associated with this tendril
            /// </summary>
            ///<remarks>
            /// If this is null but the slot is supposed to be working, just wipe this struct and create a new one.
            /// </remarks>
            public LeechScarf_TendrilProjectile proj;

            /// <summary>
            /// whether this slot is active and not on cooldown.
            /// </summary>
            /// <remarks> 
            /// while cooldown > 0, this is always false.
            /// </remarks>
            public bool Active;

            /// <summary>
            /// the associated slot that this tendril is associated with
            /// </summary>
            /// <remarks>
            /// Its important to assign these properly, as this will be used for a cooldown later on.
            /// We'll also want to be able to look up the tendril via slot, just in case.
            /// </remarks>
            public int Slot;

            /// <summary>
            /// the cooldown of this tendril, after it expires.
            /// </summary>
            /// <remarks>
            /// Counts down until zero.
            /// </remarks>
            public int Cooldown;

            /// <summary>
            /// tracks each tendril's individual hit cooldown, so hitting a worm won't instantly refill all tendrils.
            /// </summary>
            public int HitCooldown;

            public Tendril(Projectile proj, int slot)
            {
                this.proj = proj != null
                ? proj.As<LeechScarf_TendrilProjectile>()
                : null;
                this.Slot = slot;
            }
        }
        public float GetSlotCompletion(LeechScarf_Player.Tendril t)
        {
            if (t.Active)
                return 1f;

            if (t.Cooldown <= 0)
                return 1f;

            return 1f - t.Cooldown / (float)MAX_TENDRIL_COOLDOWN;
        }


        /// <summary>
        /// that looks through each tendril and updates it accordingly based on the status of the tendril.
        /// for example, subtracting from cooldown until it reaches zero, then marking the slot as active and spawning a new projecitle to fill it.
        /// </summary>
        private void UpdateTendrils()
        {
            // Ensure we always have MAX_TENDRILS logical slots
            while (TendrilList.Count < MAX_TENDRILS)
                TendrilList.Add(new Tendril(null, TendrilList.Count));

            for (int i = 0; i < TendrilList.Count; i++)
            {
                Tendril t = TendrilList[i];

                if (t.HitCooldown > 0)
                {
                    t.HitCooldown--;
                }
                // Cooldown handling
                if (t.Cooldown > 0)
                {
                    Main.NewText($"{t.Slot}, Cooldown: {t.Cooldown}");
                    t.Cooldown--;
                    
                    
                }
                if (t.Cooldown <= 0)
                    t.Active = true;

                // If slot is active but projectile is missing or dead, respawn it
                if (t.Active)
                {
                    if (t.proj == null || !t.proj.Projectile.active)
                    {
                        Projectile p = Projectile.NewProjectileDirect(
                            Player.GetSource_FromThis(),
                            Player.Center,
                            Vector2.Zero,
                            Type,
                            BaseDamage,
                            0f,
                            Player.whoAmI
                        );


                        SoundEngine.PlaySound(GennedAssets.Sounds.Common.LargeBloodSpill with { Pitch = -1 }, Player.Center).WithVolumeBoost(1f);
                        {
                            t.proj = p.As<LeechScarf_TendrilProjectile>();
                            p.As<LeechScarf_TendrilProjectile>().Slot = t.Slot; 
                        }
                    }
                }

                TendrilList[i] = t;
            }
        }


        public void KillTendril(int slot)
        {
            Tendril t = TendrilList[slot];

            SoundEngine.PlaySound(GennedAssets.Sounds.Common.MediumBloodSpill with { PitchVariance = 0.2f }, Player.Center).WithVolumeBoost(2);
            if (t.Active && t.proj != null && t.proj.Projectile.active)
            {
                // Consume this tendril
                t.Active = false;
                t.Cooldown = 60*12; 

                t.proj.Projectile.Kill();
                t.proj = null;

                TendrilList[slot] = t;
            }

        }
        #endregion





        public override void PostUpdateMiscEffects()
        {
            if (!Active)
                return;

            UpdateTendrils();

            if (!Player.Calamity().cooldowns.ContainsKey(LeechScarfCooldown.ID))
            {
                Player.AddCooldown(LeechScarfCooldown.ID, MAX_TENDRIL_COOLDOWN);
            }

           
        }


        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Player.Distance(target.Center) < 50)
                return;

            for (int i = 0; i < MAX_TENDRILS; i++)
            {
                var t = TendrilList[i];
                if (!t.Active && t.HitCooldown <=0)
                {
                    t.Cooldown -= 50;
                    t.HitCooldown = 30;
                }
                TendrilList[i] = t;
            }

        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {

            //If too far or is a leech scarf, don't reduce cooldown
            if (Player.Distance(target.Center) < 50 || proj.type == ModContent.ProjectileType<LeechScarf_TendrilProjectile>())
                return;

            for (int i = 0; i < MAX_TENDRILS; i++)
            {
                var t = TendrilList[i];
                if (!t.Active && t.HitCooldown <= 0)
                {
                    t.Cooldown -= 50;
                }
                TendrilList[i] = t;
            }

        }
      
        public override void ResetEffects()
        {
            if (!Active)
            {
                TendrilList.Clear();
            }
            Active = false;
        }
    }


}
