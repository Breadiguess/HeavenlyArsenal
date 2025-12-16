using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeavenlyArsenal.Content.Biomes;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho;

public enum ZooidType
{
    basic,

    grabber,

    Ranged,

    Blizzard
}

public struct CryonophoreZooid
{
    public int id;

    public ZooidType type;

    public Vector2 position;

    public float rotation;

    public CryonophoreZooid(int id, ZooidType type, Vector2 position)
    {
        this.id = id;
        this.type = type;
        this.position = position;
    }
}

internal partial class Cryonophore : BloodMoonBaseNPC
{
    public override void AI()
    {
        if (Main.LocalPlayer.name.ToLower() == "tester2")
        {
            var totalActive = 0;
            var counts = new Dictionary<int, int>();

            for (var i = 0; i < Main.npc.Length; i++)
            {
                var n = Main.npc[i];

                if (n != null && n.active)
                {
                    totalActive++;

                    if (counts.ContainsKey(n.type))
                    {
                        counts[n.type]++;
                    }
                    else
                    {
                        counts[n.type] = 1;
                    }
                }
            }

            var uniqueTypes = counts.Count;

            // Build a compact message showing totals and the top few types
            var top = counts.OrderByDescending(kv => kv.Value).Take(10).ToList();
            var sb = new StringBuilder();
            sb.Append($"Active NPCs: {totalActive}, Unique types: {uniqueTypes}. \nTop= ");

            for (var i = 0; i < top.Count; i++)
            {
                var kv = top[i];
                string name;

                try
                {
                    name = Lang.GetNPCNameValue(kv.Key);

                    if (string.IsNullOrEmpty(name))
                    {
                        name = $"NPC#{kv.Key}";
                    }
                }
                catch
                {
                    name = $"NPC#{kv.Key}";
                }

                sb.Append($"{name}({kv.Key}) x{kv.Value}\n");

                if (i < top.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append($"{OwnedZooids.Count}");

            Main.NewText(sb.ToString());
        }

        if (NPC.ai[1] == 1)
        {
            NPC.Center = Main.MouseWorld;
            NPC.ai[1] = 0;

            return;
        }

        StateMachine();
        Time++;
    }

    public override void PostAI()
    {
        updateZooidPosition();
    }

    #region setup

    /// <summary>
    ///     dictionary used to keep track of each zooid this npc owns, as well as the npc that this zooid
    ///     will represent once it is made.
    /// </summary>
    private Dictionary<int, (CryonophoreZooid, NPC)> OwnedZooids = new();

    public override float buffPrio => 0.4f;

    public override bool canBeSacrificed => false;

    public override bool canBebuffed => true;

    public override int bloodBankMax => 30;

    public override void SetDefaults()
    {
        NPC.damage = 30;
        NPC.lifeMax = 600_000;
        NPC.defense = 70;
        NPC.aiStyle = -1;
        NPC.Size = new Vector2(30, 30);
        NPC.noGravity = true;
        NPC.HitSound = SoundID.NPCHit1;

        SpawnModBiomes =
        [
            ModContent.GetInstance<RiftEclipseBiome>().Type
        ];
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.MustAlwaysDraw[Type] = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        OwnedZooids = new Dictionary<int, (CryonophoreZooid, NPC)>(5);

        for (var i = 0; i < 6; i++)
        {
            var a = ZooidType.basic; //(ZooidType)Main.rand.Next(0, 4);
            addZooid(a);
        }
    }

    #endregion

    #region helpers

    private void addZooid(ZooidType type)
    {
        var a = new CryonophoreZooid(OwnedZooids.Count, type, NPC.Center);
        OwnedZooids.Add(a.id, (a, null));
    }

    private void SpawnZooid(int id)
    {
        if (!OwnedZooids.ContainsKey(id))
        {
            return;
        }

        if (OwnedZooids[id].Item2 != null)
        {
            return;
        }

        var placeholder = NPC.NewNPCDirect(NPC.GetSource_FromThis(), OwnedZooids[id].Item1.position, ModContent.NPCType<CryonophoreLimb>());

        if (placeholder == null)
        {
            return;
        }

        placeholder.damage = NPC.defDamage;
        var limb = placeholder.ModNPC as CryonophoreLimb;

        if (limb != null)
        {
            limb.self = OwnedZooids[id].Item1;
            limb.OwnerIndex = NPC.whoAmI;
        }

        var entry = OwnedZooids[id];
        entry.Item2 = placeholder;
        OwnedZooids[id] = entry;
    }

    // Pseudocode:
    // - Iterate the keys of OwnedZooids to avoid modifying the collection while enumerating.
    // - For each entry:
    //   - Read the value tuple (struct + NPC).
    //   - Update the struct's position (remember structs are copied, so modify the copy then reassign).
    //   - If the associated NPC exists and is active, set its Center to the new position so the in-world NPC matches.
    //   - Reassign the modified tuple back into the dictionary.
    //
    // This solves the TODO: ensure we properly update the stored struct instance instead of only modifying a copy.

    private void updateZooidPosition()
    {
        //am i dumb or just dumb
        var keys = new List<int>(OwnedZooids.Keys);

        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var entry = OwnedZooids[key];

            if (OwnedZooids[i].Item2 != null)
            {
                continue;
            }

            var zooid = entry.Item1;

            var desiredPos = NPC.Center + new Vector2(zooid.id * 10 - 20, 40);

            var below = NPC.Center - new Vector2(0, 50);

            var rot = below.AngleTo(zooid.position);
            zooid.rotation = rot;

            zooid.position = desiredPos;

            // Put the modified struct back into the tuple and write the tuple back to the dictionary.
            entry.Item1 = zooid;
            OwnedZooids[key] = entry;

            // If the associated NPC exists and is active, update its world position to match.
            if (entry.Item2 != null && entry.Item2.active)
            {
                entry.Item2.Center = desiredPos;
                OwnedZooids[key] = entry;
            }
        }
    }

    #endregion
}