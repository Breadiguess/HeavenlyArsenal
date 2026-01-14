using System.IO;
using HeavenlyArsenal.Common.Networking.Enums;
using HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;

namespace HeavenlyArsenal.Common.Networking.Handlers;

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