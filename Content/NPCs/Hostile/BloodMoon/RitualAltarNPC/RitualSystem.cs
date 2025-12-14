using System.Collections.Generic;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

public class RitualSystem : ModSystem
{
    private static readonly HashSet<NPC> _buffedNPCs = new();

    // Public read-only access (for foreach)
    public static IEnumerable<NPC> BuffedNPCs => _buffedNPCs;

    public static bool IsNPCBuffed(NPC npc)
    {
        return _buffedNPCs.Contains(npc);
    }

    public static void AddNPC(NPC npc)
    {
        if (npc != null && npc.active)
        {
            _buffedNPCs.Add(npc);
        }
    }

    public static void RemoveNPC(NPC npc)
    {
        _buffedNPCs.Remove(npc);
    }

    public override void PostUpdateEverything()
    {
        if (_buffedNPCs.Count <= 0)
        {
            return;
        }

        _buffedNPCs.RemoveWhere(npc => npc == null || !npc.active);
        _buffedNPCs.RemoveWhere(id => !id.active || id.life <= 0);
        _buffedNPCs.RemoveWhere(id => !id.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff);
        _buffedNPCs.RemoveWhere(id => id.type == ModContent.NPCType<RitualAltar>());
        /*
        string a = "";
        foreach (NPC npc in BuffedNPCs)
        {
            a += $"{npc.ToString()}\n";
        }
        if (a.Length > 0)
            Main.NewText(a);
        */
    }

    public override void OnWorldUnload()
    {
        _buffedNPCs.Clear();
    }
}