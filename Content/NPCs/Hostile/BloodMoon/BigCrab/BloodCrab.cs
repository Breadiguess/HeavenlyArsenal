using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    partial class BloodCrab : BloodMoonBaseNPC
    {
        public override int blood => 0;
        public override int bloodBankMax => 50;
        public bool Anchored = false;
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab";
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
				new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab1")
            ]);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 100;
            NPC.height = 55;
            NPC.damage = 200;
            NPC.defense = 130 / 2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0.5f;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 13;
        }

        public override void AI()
        {
            StateMachine();
            Time++;
        }

        public override void PostAI()
        {
            
        }
    }
}
