using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;
using Luminance.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

internal class DebugJellySummon : ModItem
{
    public override string Texture => MiscTexturesRegistry.PixelPath;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // This helps sort inventory know that this is a boss summoning Item.
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 20;
        Item.value = 100;
        Item.rare = ItemRarityID.Blue;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = false;
    }

    public override bool CanUseItem(Player player)
    {
        // If you decide to use the below UseItem code, you have to include !NPC.AnyNPCs(id), as this is also the check the server does when receiving MessageID.SpawnBoss.
        // If you want more constraints for the summon item, combine them as boolean expressions:
        //    return !Main.IsItDay() && !NPC.AnyNPCs(ModContent.NPCType<MinionBossBody>()); would mean "not daytime and no MinionBossBody currently alive"
        return !NPC.AnyNPCs(ModContent.NPCType<BloodJelly>());
    }

    public override void HoldItem(Player player)
    {
        if (!player.dead)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<BloodJelly>()))
            {
                foreach (var npc in Main.npc)
                {
                    if (npc.active && npc.type == ModContent.NPCType<BloodJelly>())
                    {
                        npc.ai[2] = 1;
                    }

                    else
                    {
                        continue;
                    }
                }
            }
        }
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            // If the player using the item is the client
            // (explicitly excluded serverside here)
            SoundEngine.PlaySound(SoundID.Roar, player.position);

            var type = ModContent.NPCType<BloodJelly>();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, type);
            }
            else
            {
                // If the player is in multiplayer, request a spawn
                // This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, which we set in MinionBossBody
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
            }
        }

        return true;
    }
}