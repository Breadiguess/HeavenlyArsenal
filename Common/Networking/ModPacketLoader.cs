using System.IO;
using HeavenlyArsenal.Common.Networking.Enums;

namespace HeavenlyArsenal.Common.Networking;

public sealed class ModPacketLoader : ModSystem
{
    private static readonly Dictionary<NetworkMessageType, IPacketHandler> Handlers = [];
    
    public static void Register<T>(T handler) where T : IPacketHandler
    {
        var type = handler.Type;
        
        if (Handlers.ContainsKey(type))
        {
            // TODO: Exception message.
            throw new InvalidOperationException();
        }
        
        Handlers[type] = handler;
    }

    internal static void Handle(in BinaryReader reader, int whoAmI)
    {
        var type = (NetworkMessageType)reader.ReadByte();
        
        if (!Handlers.TryGetValue(type, out var handler))
        {
            return;
        }
        
        handler.Handle(in reader, whoAmI);
    }
}