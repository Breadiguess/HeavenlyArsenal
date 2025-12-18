using HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;
using System.IO;

namespace HeavenlyArsenal
{
    partial class HeavenlyArsenal
    {
        internal enum MessageType : byte
        {
            LeechScarf_Sync,
            ExampleStatIncreasePlayerSync,
            ExampleTeleportToStatue,
            ExampleDodge,
            ExampleTownPetUnlockOrExchange,
            ExampleResourceEffect,
            StartVictoryPose,
            CancelVictoryPose,
            SendCustomUseStylePlayerDirection,
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {

            MessageType msgType = (MessageType)reader.ReadByte();

            switch (msgType)
            {
                case MessageType.LeechScarf_Sync:
                    byte playerNumber = reader.ReadByte();
                    LeechScarf_Player examplePlayer = Main.player[playerNumber].GetModPlayer<LeechScarf_Player>();
                    
                        examplePlayer.ReceivePlayerSync(reader);

                    if (Main.netMode == NetmodeID.Server)
                    {
                        examplePlayer.SyncPlayer(-1, whoAmI, false);
                    }

                    break;
            }
        }
    }
}
