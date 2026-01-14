using CalamityMod;
using HeavenlyArsenal.Common.Ui.Cooldowns;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using System.IO;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    public sealed class LeechScarfPlayer : ModPlayer
    {
        public bool Active { get; set; }

        public readonly int Type = ModContent.ProjectileType<LeechScarf_TendrilProjectile>();

        #region tendrilStruct
        public IReadOnlyList<Tendril> Tendrils => TendrilList;

        public const string PacketName = "LeechScarf_Player_";
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
        public float GetSlotCompletion(LeechScarfPlayer.Tendril t)
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
                    t.Cooldown--;

                    Main.NewText(t.Cooldown);
                }
                if (t.Cooldown <= 0)
                    t.Active = true;

               
                // If slot is active but projectile is missing or dead, respawn it
                if (t.Active && t.Cooldown <= 0)
                {
                    if (t.proj == null || !t.proj.Projectile.active || t.proj.Type != ModContent.ProjectileType<LeechScarf_TendrilProjectile>())
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
                t.Cooldown = 60 * 12;

                t.proj.Projectile.active = false;
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
                Player.AddCooldown(LeechScarfCooldown.ID, 1);
            }

          

        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {

            for (int i = 0; i < TendrilList.Count; i++)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)HeavenlyArsenal.MessageType.LeechScarf_Sync);
                packet.Write((byte)Player.whoAmI);
                var t = TendrilList[i];
                packet.Write((byte)t.Slot);
                packet.Write((byte)t.Cooldown);
                packet.Write((byte)t.HitCooldown);
                packet.Send(toWho, fromWho);
            }

            }
            // Called in ExampleMod.Networking.cs
        public void ReceivePlayerSync(BinaryReader reader)
        {
            int slot = reader.ReadByte();

            var t = TendrilList[slot];
            t.Slot = slot;
            t.Cooldown = reader.ReadByte();
            t.HitCooldown = reader.ReadByte();


            TendrilList[slot] = t;

        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            LeechScarfPlayer clone = (LeechScarfPlayer)targetCopy;
            clone.TendrilList = new List<Tendril>(3);
            clone.TendrilList.Add(new Tendril());
            clone.TendrilList.Add(new Tendril());
            clone.TendrilList.Add(new Tendril());

            for (int i = 0; i < clone.TendrilList.Count; i++)
            {
                var t = clone.TendrilList[i];
                t.Cooldown = this.TendrilList[i].Cooldown;
                t.HitCooldown = this.TendrilList[i].HitCooldown;
                t.Slot = this.TendrilList[i].Slot;

                clone.TendrilList[i] = t;
            }
            targetCopy = clone;

        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            LeechScarfPlayer clone = (LeechScarfPlayer)clientPlayer;

            for (int i = 0; i < clone.TendrilList.Count; i++)
            {
                var t = TendrilList[i];
                var e = clone.TendrilList[i];
                if (t.Cooldown != e.Cooldown || t.HitCooldown != e.HitCooldown)
                {
                    // This example calls SyncPlayer to send all the data for this ModPlayer when any change is detected,
                    // but if you are dealing with a large amount of data you should try
                    // to be more efficient and use custom packets to selectively send only specific data that has changed.
                    SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
                }
            }

        }


        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Active)
                return;
            if (Player.Distance(target.Center) < 50)
                return;

            for (int i = 0; i < MAX_TENDRILS; i++)
            {
                var t = TendrilList[i];
                if (!t.Active && t.HitCooldown <= 0)
                {
                    t.Cooldown -= 50;
                    t.HitCooldown = 30;
                }
                TendrilList[i] = t;


            }

        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {

            if (!Active)
                return;
            //If too far or is a leech scarf, don't reduce cooldown
            if (Player.Distance(target.Center) < 50 || proj.type == ModContent.ProjectileType<LeechScarf_TendrilProjectile>())
                return;

            for (int i = 0; i < MAX_TENDRILS; i++)
            {
                var t = TendrilList[i];
                if (!t.Active && t.HitCooldown <= 0)
                {
                    t.Cooldown -= 50;
                    t.HitCooldown = 30;
                }
                TendrilList[i] = t;

            }

        }

        public override void ResetEffects()
        {
            if (!Active)
            {
                for (int i = 0; i < TendrilList.Count; i++)
                {

                    var t = TendrilList[i];

                    t.Cooldown = MAX_TENDRIL_COOLDOWN;

                    TendrilList[i] = t;
                }
            }
            Active = false;
        }
    }


}
