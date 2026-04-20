using HeavenlyArsenal.Common.Networking;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using System.IO;

namespace HeavenlyArsenal;

/// <summary>
///     The <see cref="Mod"/> implementation of Heavenly Arsenal.
/// </summary>
public sealed partial class HeavenlyArsenal : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        base.HandlePacket(reader, whoAmI);
        
        ModPacketLoader.Handle(in reader, whoAmI);



        CultMessageType msgType = (CultMessageType)reader.ReadByte();

        switch (msgType)
        {
            case CultMessageType.RequestFullSync:
                // Client asked server for current cult state.
                if (Main.netMode == NetmodeID.Server)
                    CultistCoordinator.SyncFull(toClient: whoAmI);
                break;

            case CultMessageType.FullSync:
                // Server sent the authoritative cult snapshot.
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    CultistCoordinator.ReceiveFullSync(reader);
                break;
        }
    }
}