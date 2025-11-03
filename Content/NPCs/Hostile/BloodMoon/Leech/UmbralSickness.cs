using Luminance.Assets;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Particles.Metaballs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    class UmbralSickness : ModBuff
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // It's a debuff so it can be removed by Nurse NPCs and items like Cleansing Powder
             BuffID.Sets.NurseCannotRemoveDebuff[Type] = false; // Nurse can remove this debuff
        }

        
        public override void Update(NPC npc, ref int buffIndex)
        {
            int temp = npc.buffTime[buffIndex];
           
            if (temp == 0)
            {
                npc.SimpleStrikeNPC((int)(npc.lifeMax / 4), 0, noPlayerInteraction: true);
                for (int i = 0; i < Main.rand.Next(2, 5); i++)
                {
                    NPC.NewNPCDirect(npc.GetSource_FromThis(), npc.Center, ModContent.NPCType<Umbralarva>());
                    BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                    for (int x = 0; x< 10; x++)
                    {
                        Vector2 bloodSpawnPosition = npc.Center;
                        Vector2 bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - npc.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                        metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(40f, 80f), 40);


                    }
}
            }
        }
        public override void Update(Player player, ref int buffIndex)
        {
            int temp = player.buffTime[buffIndex];
            // Main.NewText(temp);
            if (temp == 0)
            {

                player.statLife -= player.statLifeMax2/10;
                
            }
            if (player.statLife <= 0)
            {
               for (int i = 0; i < Main.rand.Next(6, 21); i++)
                {
                    NPC.NewNPCDirect(player.GetSource_FromThis(), player.Center, ModContent.NPCType<Umbralarva>());
                    BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                    for (int x = 0; x < 40; x++)
                    {
                        Vector2 bloodSpawnPosition = player.Center;
                        Vector2 bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - player.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                        metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(40f, 80f), 40);


                    }
                }
                player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason($"{player.name} succumbed to the Umbral Sickness."), 9999, 0);

            }

        }
    }
}
