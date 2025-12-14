using System.Collections.Generic;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

/// <summary>
///     Datastructure for cults.
/// </summary>
public class Cult
{
    public int CultID;

    public NPC Leader;

    public List<NPC> Cultists;

    public int MaxCultists;

    public bool IsValid => Leader != null && Leader.active;

    /// <summary>
    ///     Constructor of the data Structure.
    /// </summary>
    /// <param name="id">
    ///     the ID of the cult. this is managed with a static number, so hopefully there
    ///     should never be repeats.
    /// </param>
    /// <param name="leader"></param>
    /// <param name="maxCultists"></param>
    public Cult(int id, NPC leader, int maxCultists)
    {
        CultID = id;
        Leader = leader;
        MaxCultists = maxCultists;
        Cultists = new List<NPC>(maxCultists);
    }
}

internal class CultistCoordinator : GlobalNPC
{
    public static readonly Dictionary<int, Cult> Cults = new();

    private static int nextCultID;

    /// <summary>
    ///     Creates a new cult with the given leader npc.
    /// </summary>
    /// <param name="leader"> the "Leader" of the cult. this will always be a ritual altar.</param>
    /// <param name="maxCultists">The maximum amount of cultists this cult can have.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Invalid Cult Leader</exception>
    public static int CreateNewCult(NPC leader, int maxCultists = 3)
    {
        if (leader == null || !leader.active)
        {
            throw new Exception("Invalid cult leader.");
        }

        var id = nextCultID++;
        var cult = new Cult(id, leader, maxCultists);
        Cults[id] = cult;

        return id;
    }

    /// <summary>
    ///     Attaches an instance of an npc to a cult.
    /// </summary>
    /// <param name="id">
    ///     The ID of the cult to attach to. see <see cref="GetCultOfNPC(NPC)" /> to get the
    ///     ID.
    /// </param>
    /// <param name="npc"> The NPC to attach to this cult.</param>
    /// <exception cref="Exception"></exception>
    public static void AttachToCult(int id, NPC npc)
    {
        if (!Cults.TryGetValue(id, out var cult))
        {
            throw new Exception($"Cult with ID {id} not found!");
        }

        if (!cult.Cultists.Contains(npc))
        {
            cult.Cultists.Add(npc);
        }
    }

    public static void RemoveFromCult(int id, NPC npc)
    {
        if (!Cults.TryGetValue(id, out var cult))
        {
            throw new Exception($"Cult with ID {id} not found!");
        }

        cult.Cultists.Remove(npc);
    }

    /// <summary>
    ///     Returns the value of the cult that the given npc is a part of.
    ///     returns null if not in a cult.
    /// </summary>
    /// <param name="npc"> this npc. </param>
    /// <returns></returns>
    public static Cult GetCultOfNPC(NPC npc)
    {
        foreach (var cult in Cults.Values)
        {
            if (cult.Leader == npc || cult.Cultists.Contains(npc))
            {
                return cult;
            }
        }

        return null;
    }

    public static void UpdateCults()
    {
        if (Cults.Count <= 0)
        {
            return;
        }

        List<int> invalid = new();

        foreach (var kvp in Cults)
        {
            if (!kvp.Value.IsValid)
            {
                invalid.Add(kvp.Key);
            }

            kvp.Value.Cultists.RemoveAll(id => !id.active || id == null);
        }

        foreach (var id in invalid)
        {
            for (var i = 0; i < Cults[id].Cultists.Count; i++)
            {
                var cultist = Cults[id].Cultists[i];

                if (cultist != null && cultist.active)
                {
                    RemoveFromCult(id, cultist);
                }
            }

            Cults.Remove(id);
            nextCultID--;
        }
    }
}

internal class CultSystem : ModSystem
{
    public override void PostUpdateEverything()
    {
        CultistCoordinator.UpdateCults();
    }
}