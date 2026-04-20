using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    /// <summary>
    /// Datastructure for cults.
    /// Server owns the authoritative state.
    /// Clients keep a mirrored copy for reads/visual logic.
    /// </summary>
    public class Cult
    {
        public int CultID;
        public NPC Leader;
        public List<NPC> Cultists;
        public int MaxCultists;

        public bool IsValid => Leader != null && Leader.active;

        public Cult(int id, NPC leader, int maxCultists)
        {
            CultID = id;
            Leader = leader;
            MaxCultists = maxCultists;
            Cultists = new List<NPC>(maxCultists);
        }
    }

    internal static class CultistCoordinator
    {
        public static readonly Dictionary<int, Cult> Cults = new();

        private static int nextCultID;

        public static void Clear()
        {
            Cults.Clear();
            nextCultID = 0;
        }

        public static int CreateNewCult(NPC leader, int maxCultists = 3)
        {
            if (leader == null || !leader.active)
                throw new Exception("Invalid cult leader.");

            // In multiplayer, only server should mutate NPC-owned world state.
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return -1;

            int id = nextCultID++;
            Cult cult = new(id, leader, maxCultists);
            Cults[id] = cult;

            SyncFull();
            return id;
        }

        public static void AttachToCult(int id, NPC npc)
        {
            if (!Cults.TryGetValue(id, out var cult))
                throw new Exception($"Cult with ID {id} not found!");

            if (npc == null || !npc.active)
                return;

            if (cult.Cultists.Contains(npc))
                return;

            if (cult.Cultists.Count >= cult.MaxCultists)
                return;

            cult.Cultists.Add(npc);

            if (Main.netMode == NetmodeID.Server)
                SyncFull();
        }

        public static void RemoveFromCult(int id, NPC npc)
        {
            if (!Cults.TryGetValue(id, out var cult))
                return;

            if (npc == null)
                return;

            if (cult.Cultists.Remove(npc) && Main.netMode == NetmodeID.Server)
                SyncFull();
        }

        public static Cult GetCultOfNPC(NPC npc)
        {
            foreach (var cult in Cults.Values)
            {
                if (cult.Leader == npc || cult.Cultists.Contains(npc))
                    return cult;
            }

            return null;
        }

        public static void UpdateCults()
        {
            if (Cults.Count == 0)
                return;

            List<int> invalidCults = new();
            bool changed = false;

            foreach (var kvp in Cults)
            {
                Cult cult = kvp.Value;

                if (!cult.IsValid)
                {
                    invalidCults.Add(kvp.Key);
                    continue;
                }

                // Null check first.
                int removed = cult.Cultists.RemoveAll(npc => npc == null || !npc.active);
                if (removed > 0)
                    changed = true;
            }

            foreach (int id in invalidCults)
            {
                Cults.Remove(id);
                changed = true;
            }

            if (changed && Main.netMode == NetmodeID.Server)
                SyncFull();
        }

        /// <summary>
        /// Server -> clients full snapshot.
        /// </summary>
        public static void SyncFull(int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = ModContent.GetInstance<HeavenlyArsenal>().GetPacket();
            packet.Write((byte)CultMessageType.FullSync);

            packet.Write(nextCultID);
            packet.Write(Cults.Count);

            foreach (var kvp in Cults)
            {
                Cult cult = kvp.Value;

                packet.Write(cult.CultID);
                packet.Write(cult.MaxCultists);
                packet.Write(cult.Leader?.whoAmI ?? -1);

                // Only send valid cultists.
                List<NPC> validCultists = new();
                foreach (NPC npc in cult.Cultists)
                {
                    if (npc != null && npc.active)
                        validCultists.Add(npc);
                }

                packet.Write(validCultists.Count);
                foreach (NPC npc in validCultists)
                    packet.Write(npc.whoAmI);
            }

            packet.Send(toClient, ignoreClient);
        }

        /// <summary>
        /// Client rebuild from server snapshot.
        /// </summary>
        public static void ReceiveFullSync(BinaryReader reader)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            Clear();

            nextCultID = reader.ReadInt32();
            int cultCount = reader.ReadInt32();

            for (int i = 0; i < cultCount; i++)
            {
                int cultID = reader.ReadInt32();
                int maxCultists = reader.ReadInt32();
                int leaderWhoAmI = reader.ReadInt32();

                NPC leader = leaderWhoAmI >= 0 && leaderWhoAmI < Main.maxNPCs ? Main.npc[leaderWhoAmI] : null;
                Cult cult = new(cultID, leader, maxCultists);

                int cultistCount = reader.ReadInt32();
                for (int j = 0; j < cultistCount; j++)
                {
                    int cultistWhoAmI = reader.ReadInt32();
                    if (cultistWhoAmI >= 0 && cultistWhoAmI < Main.maxNPCs)
                    {
                        NPC npc = Main.npc[cultistWhoAmI];
                        if (npc != null && npc.active)
                            cult.Cultists.Add(npc);
                    }
                }

                Cults[cultID] = cult;
            }
        }

        /// <summary>
        /// Client -> server asks for current cult snapshot.
        /// </summary>
        public static void RequestFullSync()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            ModPacket packet = ModContent.GetInstance<HeavenlyArsenal>().GetPacket();
            packet.Write((byte)CultMessageType.RequestFullSync);
            packet.Send();
        }
    }

    internal enum CultMessageType : byte
    {
        RequestFullSync,
        FullSync
    }

    internal class CultSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            CultistCoordinator.Clear();
        }

        public override void OnWorldUnload()
        {
            CultistCoordinator.Clear();
        }

        public override void PostUpdateEverything()
        {
            // Only server should do authoritative updates in MP.
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            CultistCoordinator.UpdateCults();
        }
    }

    internal class CultSyncPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                CultistCoordinator.RequestFullSync();
        }
    }
}