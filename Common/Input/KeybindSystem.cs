namespace HeavenlyArsenal.Common.Keybinds;

public sealed class KeybindSystem : ModSystem
{
    public static ModKeybind SwirlCloak { get; private set; }

    public static ModKeybind HaemsongBind { get; private set; }

    public static ModKeybind ShadowTeleport { get; private set; }
    
    public static ModKeybind BloodArmorParry { get; private set; }
        
    public static ModKeybind BloodBlightPurge { get; private set; }

    public static ModKeybind DualModeActivete { get; private set; }

    public override void Load()
    {
        base.Load();
            
        SwirlCloak = KeybindLoader.RegisterKeybind(Mod, "Swirl Cloak", "F");
        HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Swap Blood Armor Form", "F");
        ShadowTeleport = KeybindLoader.RegisterKeybind(Mod, "Shadow Teleport", "F");
        BloodArmorParry = KeybindLoader.RegisterKeybind(Mod, "Blood Armor Parry", "T");
        BloodBlightPurge = KeybindLoader.RegisterKeybind(Mod, "Blood Blight Purge", "V");
        DualModeActivete = KeybindLoader.RegisterKeybind(Mod, "Activete Dual Mode", "X");
    }

    public override void Unload()
    {
        base.Unload();
            
        SwirlCloak = null;
        HaemsongBind = null;
        ShadowTeleport = null;
        BloodArmorParry = null;
        BloodBlightPurge = null;
        DualModeActivete = null;
    }
}