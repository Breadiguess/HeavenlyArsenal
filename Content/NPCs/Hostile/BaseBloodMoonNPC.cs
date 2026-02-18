using HeavenlyArsenal.Content.Biomes;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.NPCs.Hostile
{
    public readonly struct BloodMoonBalanceStrength
    {
        public readonly float HealthMulti_Influence;
        public readonly float DamageMulti_Influence;
        public readonly float DefenseMulti_Influence;
        public BloodMoonBalanceStrength(float healthMulti_Influence = 1, float damageMulti_Influence = 1, float DefenseInfluence = 1)
        {
            HealthMulti_Influence = healthMulti_Influence;
            DamageMulti_Influence = damageMulti_Influence;
            DefenseMulti_Influence = DefenseInfluence;
        }
    }
    public abstract class BaseBloodMoonNPC : ModNPC
    {
        public override void Load()
        {
            On_NPC.SetDefaults += ApplyBloodMoonBalancingRule;
        }

        private void ApplyBloodMoonBalancingRule(On_NPC.orig_SetDefaults orig, NPC self, int Type, NPCSpawnParams spawnparams)
        {
            orig(self, Type, spawnparams);
            if(self.ModNPC!=null && self.ModNPC is BaseBloodMoonNPC bloodMoonNPC)
            {
                float h = BloodMoonBalancing.HealthMultiplier;
                float d = BloodMoonBalancing.DamageMultiplier;
                float def = BloodMoonBalancing.DefenseMultiplier;

                var s = self.As<BaseBloodMoonNPC>().Strength;

                float hScale = 1f + (h - 1f) * s.HealthMulti_Influence;
                float dScale = 1f + (d - 1f) * s.DamageMulti_Influence;
                float defScale = 1f + (def - 1f) * s.DefenseMulti_Influence;
                hScale = MathF.Max(0.1f, hScale);
                dScale = MathF.Max(0.1f, dScale);
                defScale = MathF.Max(0.1f, defScale);


                self.lifeMax = (int)(self.lifeMax * hScale);
                self.damage = (int)(self.damage * d);
                self.defense = (int)(self.defense * def);

                self.life = self.lifeMax;
            }

        }

        public virtual void SendExtraAI2(BinaryWriter writer)
        {
        }
        public virtual void ReceiveExtraAI2(BinaryReader reader)
        {
        }
        public sealed override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            SendExtraAI2(writer);
            writer.Write(Blood);
            writer.Write(CanBeSacrificed);

        }
        public sealed override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            ReceiveExtraAI2(reader);
            Blood = (int)reader.ReadSingle();
            CanBeSacrificed = reader.ReadBoolean();
        }

        public float SacrificePriority;
        public virtual int BloodMoonSpawnWeight => 10;
        public virtual int BestiaryLineAmount => 1;
        public override string LocalizationCategory => "NPCs";

        public Entity Target;
        public int Blood;
        public bool CanBeSacrificed;
        public abstract int MaxBlood { get; }

        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        protected virtual void SetStaticDefaults2()
        {

        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

        }
        /// <summary>
        /// Alternate method to set defaults since its sealed to prevent issues.
        /// </summary>
        protected abstract void SetDefaults2();
        public sealed override void SetDefaults()
        {
            
            SetDefaults2();
            SpawnModBiomes =
            [
                ModContent.GetInstance<RiftEclipseBiome>().Type
            ];

        }
        public sealed override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            var avatarID = ModContent.NPCType<AvatarOfEmptiness>();

            bestiaryEntry.UIInfoProvider =
                new HighestOfMultipleUICollectionInfoProvider
                (
                    new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type], true),
                    new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[avatarID], true)
                );

            bestiaryEntry.Info.AddRange
            (
                new IBestiaryInfoElement[]
                {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,


                }
            );
            for (int i = 0; i < BestiaryLineAmount; i++)
            {
                bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement($"Mods.HeavenlyArsenal.Bestiary.{GetType().Name}_{i}"));
            }
        }


        public abstract BloodMoonBalanceStrength Strength { get; }


    }
}
