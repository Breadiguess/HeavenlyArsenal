using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile
{
    public abstract class BaseBloodMoonNPC : ModNPC
    {
        public virtual int BloodMoonSpawnWeight => 10;
        public virtual int BestiaryLineAmount => 1;
        public override string LocalizationCategory => "NPCs";

        public Entity Target;
        public int Blood;
        public bool CanBeSacrificed;
        public abstract int MaxBlood { get; }

        public sealed override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

        }
        public sealed override void SetDefaults()
        {
            base.SetDefaults();
            SpawnModBiomes =
            [
                ModContent.GetInstance<RiftEclipseBloodMoon>().Type
            ];

        }


        public sealed override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
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
    }
}
