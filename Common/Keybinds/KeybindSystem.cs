using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.Keybinds
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind HaemsongBind { get; private set; }
        public static ModKeybind ShadowTeleport { get; private set; }
        public static ModKeybind SwirlCloak { get; private set; }
        
        public static ModKeybind BloodBlight_Purge { get; private set; }
        public static ModKeybind BloodArmorParry { get; private set; }
        public override void Load()
        {
            HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Swap Blood Armor Form", "F");
            ShadowTeleport = KeybindLoader.RegisterKeybind(Mod, "Shadow Teleport", "F");
            SwirlCloak = KeybindLoader.RegisterKeybind(Mod, "Swirl Cloak", "F");
            BloodArmorParry = KeybindLoader.RegisterKeybind(Mod, "Blood Armor Parry", "T");
            BloodBlight_Purge = KeybindLoader.RegisterKeybind(Mod, "Blood Blight Purge", "V");
        }

        public override void Unload()
        {
            HaemsongBind = null;
            ShadowTeleport = null;
            SwirlCloak = null;
        }
    }
}
