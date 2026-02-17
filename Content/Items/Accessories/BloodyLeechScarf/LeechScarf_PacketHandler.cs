using CalamityMod.Cooldowns;

using System.IO;
using System.Runtime.CompilerServices;

// TODO: Implement packet handler from new networking system.
namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    /*
     *     internal sealed class LeechScarf_PacketHandler : PacketHandler
    {
        public LeechScarf_PacketHandler(byte packetType)
            : base(packetType)
        {
        }

        public override void HandlePacket(BinaryReader reader, int fromWho)
        {
            byte playerIndex = reader.ReadByte();
            byte slot = reader.ReadByte();
            short cooldown = reader.ReadInt16();
            short hitCooldown = reader.ReadInt16();


            if (playerIndex >= Main.maxPlayers)
                return;

            Player player = Main.player[playerIndex];
            if (!player.active)
                return;
            ModContent.GetInstance<HeavenlyArsenal>().Logger.InfoFormat($"{ModContent.GetInstance<HeavenlyArsenal>().Name}: {LeechScarfPlayer.PacketName}: Cooldown: {cooldown}, HitCooldown: {hitCooldown}, Slot:{slot}");

            var mp = player.GetModPlayer<LeechScarfPlayer>();

            if (slot >= mp.TendrilList.Count)
                return;

            // just grab a copy, im fairly sure that it will work
            LeechScarfPlayer.Tendril t = mp.TendrilList[slot];

            t.Cooldown = cooldown;
            t.HitCooldown = hitCooldown;


            mp.TendrilList[slot] = t;
        }
    }
     */
}
