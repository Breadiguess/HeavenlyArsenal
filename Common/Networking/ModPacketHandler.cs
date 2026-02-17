using System.IO;
using HeavenlyArsenal.Common.Networking.Enums;

namespace HeavenlyArsenal.Common.Networking;

public abstract class ModPacketHandler : ModType, IPacketHandler
{
    public abstract NetworkMessageType Type { get; }
    
    public abstract void Handle(in BinaryReader reader, int whoAmI);

    protected sealed override void Register()
    {
        ModPacketLoader.Register(this);
        
        ModTypeLookup<ModPacketHandler>.Register(this);
    }
}