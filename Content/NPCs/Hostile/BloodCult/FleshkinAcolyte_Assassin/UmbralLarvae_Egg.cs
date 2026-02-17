using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin
{
    internal class UmbralLarvae_Egg : ModNPC
    {
        public int OffsetSpawnTimer;
        public override void SetDefaults()
        {
            NPC.Size = new Vector2(20, 20);
            NPC.friendly = false;
            NPC.lifeMax = 20;
            NPC.defense = 5003994;
            NPC.knockBackResist = 0.01f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            OffsetSpawnTimer = 220+ Main.rand.Next(-1,10)*3;
        }

        public override void AI()
        {
            NPC.ai[0]++;

            NPC.rotation += MathHelper.ToRadians(NPC.velocity.X);
            if (NPC.ai[0] > OffsetSpawnTimer / 1.5f)
            {
                NPC.velocity = Main.rand.NextVector2Unit();
            }

            if (NPC.ai[0] > OffsetSpawnTimer)
            {
                NPC.StrikeInstantKill();
            }

        }

        public override void OnKill()
        {
            if (NPC.ai[0] > OffsetSpawnTimer)
            {
                NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<UmbralLarva>());
            }
            for(int i = 0; i< 10; i++)
            {

                Dust.NewDustPerfect(NPC.Center, DustID.Blood, Vector2.One.RotatedBy(i/10f));
            }
            SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash with { Pitch = -2, MaxInstances = 0}, NPC.Center);
        }
    }
}
