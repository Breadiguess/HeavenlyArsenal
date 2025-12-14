using Luminance.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

internal class UmbralSickness : ModBuff
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true; // It's a debuff so it can be removed by Nurse NPCs and items like Cleansing Powder
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = false; // Nurse can remove this debuff
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        var temp = npc.buffTime[buffIndex];

        if (temp == 0)
        {
            npc.SimpleStrikeNPC(npc.lifeMax / 4, 0, noPlayerInteraction: true);

            for (var i = 0; i < Main.rand.Next(2, 5); i++)
            {
                NPC.NewNPCDirect(npc.GetSource_FromThis(), npc.Center, ModContent.NPCType<Umbralarva>());
                var metaball = ModContent.GetInstance<BloodMetaball>();

                for (var x = 0; x < 10; x++)
                {
                    var bloodSpawnPosition = npc.Center;
                    var bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - npc.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                    metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(40f, 80f), 40);
                }
            }
        }
    }

    public override void Update(Player player, ref int buffIndex)
    {
        var temp = player.buffTime[buffIndex];

        // Main.NewText(temp);
        if (temp == 0)
        {
            player.statLife -= player.statLifeMax2 / 10;
        }

        if (player.statLife <= 0)
        {
            for (var i = 0; i < Main.rand.Next(6, 21); i++)
            {
                NPC.NewNPCDirect(player.GetSource_FromThis(), player.Center, ModContent.NPCType<Umbralarva>());
                var metaball = ModContent.GetInstance<BloodMetaball>();

                for (var x = 0; x < 40; x++)
                {
                    var bloodSpawnPosition = player.Center;
                    var bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - player.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                    metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(40f, 80f), 40);
                }
            }

            player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} succumbed to the Umbral Sickness."), 9999, 0);
        }
    }
}