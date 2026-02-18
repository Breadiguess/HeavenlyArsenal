using System.IO;
using HeavenlyArsenal.Common.Networking;
using HeavenlyArsenal.Common.Networking.Enums;

// TODO: Implement packet handler from new networking system.
namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;

public sealed class LeechScarf_PacketHandler : ModPacketHandler
{
    public override NetworkMessageType Type { get; } = NetworkMessageType.SyncLeechScarf;

    public override void Handle(in BinaryReader reader, int whoAmI)
    {
        var playerIndex = reader.ReadByte();
        var slot = reader.ReadByte();
        var cooldown = reader.ReadInt16();
        var hitCooldown = reader.ReadInt16();

        if (playerIndex >= Main.maxPlayers)
        {
            return;
        }

        var player = Main.player[playerIndex];

        if (!player.active)
        {
            return;
        }

        ModContent.GetInstance<HeavenlyArsenal>().Logger.InfoFormat($"{ModContent.GetInstance<HeavenlyArsenal>().Name}: {LeechScarfPlayer.PacketName}: Cooldown: {cooldown}, HitCooldown: {hitCooldown}, Slot:{slot}");

        var mp = player.GetModPlayer<LeechScarfPlayer>();

        if (slot >= mp.TendrilList.Count)
        {
            return;
        }

        // just grab a copy, im fairly sure that it will work
        var t = mp.TendrilList[slot];

        t.Cooldown = cooldown;
        t.HitCooldown = hitCooldown;

        mp.TendrilList[slot] = t;
    }
}