using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.Keybinds
{

    public sealed class HAKeybindRegistry : ModSystem
    {
        internal static readonly List<IKeybindHandler> Handlers = new();

        public static void Register(IKeybindHandler handler)
        {
            Handlers.Add(handler);
        }

        public override void Unload()
        {
            Handlers.Clear();
        }
    }
}
