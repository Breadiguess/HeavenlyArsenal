using System.IO;
using HeavenlyArsenal.Common.Networking;
using HeavenlyArsenal.Common.Networking.Enums;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;

public sealed class LeechScarfNetworkHandler : ModPacketHandler
{
    public override NetworkMessageType Type { get; } = NetworkMessageType.SyncLeechScarf;

    public override void Handle(in BinaryReader reader, int whoAmI)
    {
        var index = reader.ReadByte();
        var player = Main.player[index];
        
        var leechScarfPlayer = player.GetModPlayer<LeechScarfPlayer>();
                
        leechScarfPlayer.ReceivePlayerSync(reader);

        if (Main.netMode != NetmodeID.Server)
        {
            return;
        }
        
        leechScarfPlayer.SyncPlayer(-1, whoAmI, false);
    }
}