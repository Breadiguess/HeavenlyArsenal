using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using HeavenlyArsenal.Content.Biomes;
using HeavenlyArsenal.Content.Items.Misc;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Luminance.Assets;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.SwagRain;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;

public class BlackListProjectileNPCs : ModSystem
{
    //blacklisted NPCs are to be ignored as potential targets.
    public static HashSet<int> BlackListedNPCs = new();

    //todo: create a modsystem that does this for us, and then write it back to this npc upon loading the world or some shit

    public override void PostSetupContent()
    {
        //doubtless doesn't work, but you know what who gaf, i'm writing in github i can fix it later.
        //the whole point is to have something in place already to work off of.
        for (var i = 0; i < NPCLoader.NPCCount; i++)
        {
            if (NPCID.Sets.ProjectileNPC[i])
            {
                BlackListedNPCs.Add(i);
            }
        }

        BlackListedNPCs.Add(ModContent.NPCType<Solyn>());
        BlackListedNPCs.Add(ModContent.NPCType<CeaselessVoidRift>());

        BlackListedNPCs.Add(ModContent.NPCType<SuperDummyNPC>());
    }
}

public class BloodmoonSpawnControl : GlobalNPC
{
    public override void SetDefaults(NPC entity)
    {
        base.SetDefaults(entity);
    }

    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
    {
        //spawnRangeY = (int)(Main.worldSurface * 0.2f);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (Main.bloodMoon && !Main.dayTime && RiftEclipseBloodMoonRainSystem.EffectActive)
        {
            if (spawnInfo.Player.ZoneOverworldHeight)
            {
                pool.Clear();

                // The float value is the spawn weight relative to others in the pool stupid
                pool[ModContent.NPCType<ArtilleryCrab>()] = SpawnCondition.OverworldNightMonster.Chance * 0.14f;
                pool[ModContent.NPCType<newLeech>()] = SpawnCondition.OverworldNightMonster.Chance * 0.074f;

                if (spawnInfo.SpawnTileY < Main.worldSurface * 0.5f)
                {
                    pool[ModContent.NPCType<BloodJelly>()] = SpawnCondition.OverworldNightMonster.Chance * 0.04f;
                }
                pool[ModContent.NPCType<FleshkinAcolyte_Assassin>()] = SpawnCondition.OverworldNightMonster.Chance * 0.05f;
                pool[ModContent.NPCType<RitualAltar>()] = SpawnCondition.OverworldNightMonster.Chance * 0.035f;
                pool[ModContent.NPCType<FleshlingCultist.FleshlingCultist>()] = SpawnCondition.OverworldNightMonster.Chance * 0.42f;
            }
            else if (spawnInfo.Player.ZoneSkyHeight && !spawnInfo.PlayerInTown)
            {
                //if (spawnInfo.SpawnTileY < Main.worldSurface * 0.5f)
                {
                    pool.Clear();
                    pool[ModContent.NPCType<BloodJelly>()] = SpawnCondition.Sky.Chance * 1.17f;
                }
            }
        }
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (Main.bloodMoon && !Main.dayTime && RiftEclipseBloodMoonRainSystem.EffectActive)
        {
            /*
            if (Main.LocalPlayer.name.ToLower() == "tester2")
            {
                int totalActive = 0;
                var counts = new Dictionary<int, int>();

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    var n = Main.npc[i];
                    if (n != null && n.active)
                    {
                        totalActive++;
                        if (counts.ContainsKey(n.type))
                            counts[n.type]++;
                        else
                            counts[n.type] = 1;
                    }
                }

                int uniqueTypes = counts.Count;

                // Build a compact message showing totals and the top few types
                var top = counts.OrderByDescending(kv => kv.Value).Take(10).ToList();
                var sb = new StringBuilder();
                sb.Append($"Active NPCs: {totalActive}, Unique types: {uniqueTypes}. \nTop= ");

                for (int i = 0; i < top.Count; i++)
                {
                    var kv = top[i];
                    string name;
                    try
                    {
                        name = Lang.GetNPCNameValue(kv.Key);
                        if (string.IsNullOrEmpty(name)) name = $"NPC#{kv.Key}";
                    }
                    catch
                    {
                        name = $"NPC#{kv.Key}";
                    }

                    sb.Append($"{name}({kv.Key}) x{kv.Value}\n");
                    if (i < top.Count - 1) sb.Append(", ");
                }

                Main.NewText(sb.ToString());
            }*/

            spawnRate = (int)(spawnRate * 0.8f); // Half the delay -> roughly double the spawn frequency
            maxSpawns = (int)(maxSpawns * 1.5f); // Increase the cap by 50%

            //clamps? hmm
            spawnRate = Math.Max(spawnRate, 30);
            maxSpawns = Math.Min(maxSpawns, 90);
        }
    }
}



public class SolynBookDropNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {

        if (SolynBookRegistry.SolynBookItemType <= 0)
            return;

        // Replace this check with however BloodMoonBaseNPC is identified
        if (npc.ModNPC is BaseBloodMoonNPC && npc.type != ModContent.NPCType<UmbralLarva>())
        {
            if (Main.rand.NextBool(300)) // 1 / 300
            {
                Item.NewItem(
                    npc.GetSource_Loot(),
                    npc.getRect(),
                    SolynBookRegistry.SolynBookItemType
                );
            }
        }
    }
}
