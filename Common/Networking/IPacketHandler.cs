using System.IO;
using HeavenlyArsenal.Common.Networking.Enums;

namespace HeavenlyArsenal.Common.Networking;

public interface IPacketHandler
{
    NetworkMessageType Type { get; }
    
    void Handle(in BinaryReader reader, int whoAmI);
}