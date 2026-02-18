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

public abstract class BloodMoonBaseNPC : ModNPC
{
    public Player playerTarget = null;

    public NPC NPCTarget = null;

    public Entity currentTarget = null;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override string LocalizationCategory => "NPCs";

    public virtual bool ResistantToTrueMelee => true;

    public virtual bool canBebuffed => true;

    public virtual int Time
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    /// <summary>
    ///     the current blood in this npc.
    /// </summary>
    public virtual int blood { get; set; }

    /// <summary>
    ///     the total cap of blood this npc can hold.
    /// </summary>
    public virtual int bloodBankMax { get; set; } = 100;

    public override void ModifyTypeName(ref string typeName)
    {
        /*
        if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
        {
            var val = (int)NPC.GetGlobalNPC<RitualBuffNPC>().BuffType;
            var Modifier = $"{RitualBuffNPC.NameModifiers[val]}";
            typeName = $"{Modifier} {typeName}";
        }*/
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((float)blood);
        writer.Write((float)bloodBankMax);
        writer.Write(buffPrio);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        blood = (int)reader.ReadSingle();
        blood = (int)reader.ReadSingle();
        buffPrio = reader.ReadSingle();
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (ResistantToTrueMelee)
        {
            if (modifiers.DamageType == ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>() || modifiers.DamageType == ModContent.GetInstance<TrueMeleeDamageClass>())
            {
                modifiers.CritDamage *= 0.75f;
                modifiers.FinalDamage *= 0.59f;
            }
        }
    }

    public override void SetDefaults()
    {
        NPC.Calamity().VulnerableToHeat = false;
        SpawnModBiomes = [ModContent.GetInstance<RiftEclipseBiome>().Type];
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        //Utils.DrawBorderString(spriteBatch, Time.ToString(), NPC.Center - screenPos, Color.AntiqueWhite, 1, anchory: 2);
        //Utils.DrawBorderString(spriteBatch, $"{NPC.Calamity().VulnerableToHeat}", NPC.Center - screenPos + new Vector2(0, -40), Color.Red);

        /*
        string Msg = "";
        var a = NPC.GetGlobalNPC<RitualBuffNPC>();
        Msg += $"{a.BuffType.ToString()}\n";
        Msg += $"{a.ritualBuffTimer}\n";
        if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
        {
            Utils.DrawBorderString(spriteBatch, Msg, NPC.Center - screenPos, Color.AntiqueWhite, anchory: -2);
        }
        */

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    #region Snackrifice:tm:

    /// <summary>
    ///     How likely this npc is to recieve a buff compared to it's neighbors when another npc is
    ///     sacrificed.
    /// </summary>
    public virtual float buffPrio { get; set; }

    /// <summary>
    ///     Basically the priority of an npc to be sacrificed. if this is zero, they basically will almost
    ///     never be sacrificed.
    /// </summary>
    public virtual float SacrificePrio
    {
        get => !canBeSacrificed ? 0 : default;
        set => canBeSacrificed = value > 0;
    }

    /// <summary>
    ///     Determine whether this npc can be sacrificed.
    ///     this is a virtual because I feel like the ability to be sacrificed should be adjustable.
    /// </summary>
    public virtual bool canBeSacrificed { get; set; }

    /// <summary>
    ///     calculate the value of the sacrificed npc. we'll later multiply this depending on their value,
    ///     but im coding blind at the time of writing this, so that should best be left for later.
    /// </summary>
    public virtual float calculateSacrificeValue(NPC npc)
    {
        float bloodPercent = blood / bloodBankMax;
        var lifePercent = npc.life / (float)npc.lifeMax;

        var value = Utils.Clamp(bloodPercent + lifePercent, 0, 1);

        return value;
    }

    #endregion
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
