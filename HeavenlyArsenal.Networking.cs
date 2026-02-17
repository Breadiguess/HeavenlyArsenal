using System.IO;
using HeavenlyArsenal.Common.Networking;

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
    }
}