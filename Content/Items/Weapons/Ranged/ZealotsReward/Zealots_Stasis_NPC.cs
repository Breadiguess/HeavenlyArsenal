using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_Stasis_NPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public enum SlowTier
        {
            None,
            Low,
            High
        }

        public int SlowStacks;
        public int FreezeTime;
        public int StasisOwner = -1;
        public bool DrawToFrozenRT;

        public SlowTier CurrentSlowTier;
        public float FreezeInterpolant;
        public bool IsFrozen => FreezeTime > 0;

        public const int MaxStacks = 7 * 5;
        public const int FreezeThreshold = 7 * 5;
        public const int StackDecayDelay = 12;
        private int stackDecayTimer;

        public Player StackOwner
        {
            get
            {
                if (StasisOwner < 0 || StasisOwner >= Main.maxPlayers)
                    return null;

                Player player = Main.player[StasisOwner];
                return player.active ? player : null;
            }
        }

        public override void ResetEffects(NPC npc)
        {
            DrawToFrozenRT = false;
        }

        public override bool PreAI(NPC npc)
        {
            if (IsFrozen)
            {
                npc.velocity *= 0.9f;

                if (npc.knockBackResist > 0f)
                    npc.velocity *= 0.95f;

                return false;
            }

            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (FreezeTime > 0)
            {
                FreezeTime--;

                HandleStackDecay(npc, decayFasterWhileFrozen: true);

                npc.velocity *= 0.9f;

                if (FreezeTime == 0)
                    npc.netUpdate = true;
            }
            else
            {
                HandleStackDecay(npc, decayFasterWhileFrozen: false);

                UpdateSlowTier();

                if (SlowStacks > 0)
                    ApplySlowVelocityPenalty(npc);
            }

            UpdateSlowTier();

            FreezeInterpolant = LumUtils.InverseLerp(0f, MaxStacks, SlowStacks);
            DrawToFrozenRT = SlowStacks > 0 || FreezeTime > 0;
        }
        private void HandleStackDecay(NPC npc, bool decayFasterWhileFrozen)
        {
            if (SlowStacks <= 0)
            {
                stackDecayTimer = 0;
                return;
            }

            int decayDelay = decayFasterWhileFrozen ? StackDecayDelay / 2 : StackDecayDelay;
            if (decayDelay < 1)
                decayDelay = 1;

            stackDecayTimer++;
            if (stackDecayTimer >= decayDelay)
            {
                stackDecayTimer = 0;
                SlowStacks--;

                if (SlowStacks < 0)
                    SlowStacks = 0;

                npc.netUpdate = true;
            }
        }
        private void UpdateSlowTier()
        {
            if (SlowStacks >= 7 * 4)
                CurrentSlowTier = SlowTier.High;
            else if (SlowStacks >= 1)
                CurrentSlowTier = SlowTier.Low;
            else
                CurrentSlowTier = SlowTier.None;
        }



        private void ApplySlowVelocityPenalty(NPC npc)
        {
            float multiplier = 1f;

            switch (CurrentSlowTier)
            {
                case SlowTier.Low:
                    multiplier = 0.96f;
                    break;

                case SlowTier.High:
                    multiplier = 0.88f;
                    break;
            }

            npc.velocity *= multiplier;
        }

        private static int GetRealLifeAnchorIndex(NPC npc)
        {
            if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
                return npc.realLife;

            return npc.whoAmI;
        }

        private static bool IsInSameRealLifeGroup(NPC npc, int anchorIndex)
        {
            if (!npc.active)
                return false;

            if (npc.whoAmI == anchorIndex)
                return true;

            return npc.realLife == anchorIndex;
        }

        private static void ForEachRealLifeMember(NPC npc, System.Action<NPC, Zealots_Stasis_NPC> action)
        {
            int anchorIndex = GetRealLifeAnchorIndex(npc);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];

                if (!IsInSameRealLifeGroup(other, anchorIndex))
                    continue;

                action(other, other.GetGlobalNPC<Zealots_Stasis_NPC>());
            }
        }

        private static void CopyStateFrom(Zealots_Stasis_NPC source, Zealots_Stasis_NPC destination)
        {
            destination.SlowStacks = source.SlowStacks;
            destination.FreezeTime = source.FreezeTime;
            destination.StasisOwner = source.StasisOwner;
            destination.CurrentSlowTier = source.CurrentSlowTier;
            destination.DrawToFrozenRT = source.DrawToFrozenRT;
        }

        private void PropagateStateToRealLifeGroup(NPC npc)
        {
            ForEachRealLifeMember(npc, (other, global) =>
            {
                if (global == this)
                {
                    other.netUpdate = true;
                    return;
                }

                CopyStateFrom(this, global);
                other.netUpdate = true;
            });
        }

        public void AddStacks(NPC npc, int amount, IEntitySource_OnHit hit, int owner = -1)
        {
            if (IsFrozen)
            {
                Shatter(npc);
                return;
            }




            SlowStacks += amount;
            SlowStacks = Utils.Clamp(SlowStacks, 0, MaxStacks);

            if (owner >= 0 && owner < Main.maxPlayers)
                StasisOwner = owner;

            if (SlowStacks >= FreezeThreshold)
            {
                SlowStacks = FreezeThreshold;
                FreezeTime = System.Math.Max(FreezeTime, 60);
            }

            UpdateSlowTier();
            PropagateStateToRealLifeGroup(npc);
            npc.netUpdate = true;
        }

        public void Freeze(NPC npc, int time)
        {
            if (time > FreezeTime)
                FreezeTime = time;

            SlowStacks = FreezeThreshold;

            UpdateSlowTier();
            PropagateStateToRealLifeGroup(npc);
            npc.netUpdate = true;
        }


        public bool IsShattering;

        public void Shatter(NPC target)
        {
            if (!target.active || IsShattering)
                return;

            IsShattering = true;

            int perStackDamage = 400;
            int baseDamage = SlowStacks * perStackDamage;
            int frozenBonus = IsFrozen ? CalamityUtils.ScaleWithDifficulty(40_000) : 0;
            int totalDamage = baseDamage + frozenBonus;

            Player owner = StackOwner;
            int hitDirection = 0;

            if (owner != null && owner.active)
            {
                hitDirection = target.Center.X < owner.Center.X ? -1 : 1;
                totalDamage = (int)(totalDamage * 1.1f);
            }

            if (totalDamage <= 0)
                totalDamage = 10;

            ClearStasis(target);


            Zealots_ShatterParticle Particle = new();
            Vector2 Velocity = Vector2.Zero;
            Particle.Prepare(target.Center, Velocity, 120);
            ParticleEngine.ShaderParticles.Add(Particle);




            SoundEngine.PlaySound(SoundID.Item14, target.position).WithVolumeBoost(12f);

            var hit = target.CalculateHitInfo(totalDamage, hitDirection);
            target.StrikeNPC(hit);
            target.netUpdate = true;

            const float radius = 400f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (!other.active || other.whoAmI == target.whoAmI || other.friendly || other.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(target.Center, other.Center);
                if (dist > radius)
                    continue;

                Zealots_Stasis_NPC otherGlobal = other.GetGlobalNPC<Zealots_Stasis_NPC>();

                if (otherGlobal != null && !otherGlobal.IsShattering && (otherGlobal.SlowStacks > 0 || otherGlobal.IsFrozen))
                {
                    otherGlobal.Shatter(other);
                    continue;
                }

                float proximityFactor = 1f - dist / radius;
                int splashDamage = (int)Math.Max(1, totalDamage * 0.5f * proximityFactor);

                int otherHitDirection = 0;
                if (owner != null && owner.active)
                    otherHitDirection = other.Center.X < owner.Center.X ? -1 : 1;

                var splashHit = other.CalculateHitInfo(splashDamage, otherHitDirection);
                other.StrikeNPC(splashHit);
                other.netUpdate = true;
            }
            var position = target.position;
            int PosX = (int)(position.X - target.width / 2);
            int PosY = (int)(position.Y - target.height / 2);

            Rectangle r = target.Hitbox;
            r.Inflate(60, 60);
            Zealots_FreezeGore.AddFreezeZone(r, lifetime: 120, TimeUntilShader: 60, false);

            IsShattering = false;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (!npc.active || !IsFrozen || IsShattering)
                return;
            modifiers.FinalDamage *= 0;

            ShatterFromAnyHit(npc);
        }
        private void ShatterFromAnyHit(NPC npc)
        {
            Shatter(npc);
        }
        public void ClearStasis(NPC npc)
        {
            SlowStacks = 0;
            FreezeTime = 0;
            stackDecayTimer = 0;
            CurrentSlowTier = SlowTier.None;
            StasisOwner = -1;
            DrawToFrozenRT = false;

            PropagateStateToRealLifeGroup(npc);
            npc.netUpdate = true;
        }



        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(SlowStacks);
            binaryWriter.Write(FreezeTime);
            binaryWriter.Write(StasisOwner);
            binaryWriter.Write((byte)CurrentSlowTier);
            binaryWriter.Write(DrawToFrozenRT);
            binaryWriter.Write(stackDecayTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            SlowStacks = binaryReader.ReadInt32();
            FreezeTime = binaryReader.ReadInt32();
            StasisOwner = binaryReader.ReadInt32();
            CurrentSlowTier = (SlowTier)binaryReader.ReadByte();
            DrawToFrozenRT = binaryReader.ReadBoolean();
            stackDecayTimer = binaryReader.ReadInt32();
        }

    }

}