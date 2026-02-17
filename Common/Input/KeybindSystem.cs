namespace HeavenlyArsenal.Common.Input;

public sealed class KeybindSystem : ModSystem
{
    /// <summary>
    ///     Gets or sets the <see cref="ModKeybind"/> instance for the Swirl Cloak.
    /// </summary>
    public static ModKeybind SwirlCloak { get; private set; }
    
    /// <summary>
    ///     Gets or sets the <see cref="ModKeybind"/> instance for the Haemsong Bind.
    /// </summary>
    public static ModKeybind HaemsongBind { get; private set; }
    
    /// <summary>
    ///     Gets or sets the <see cref="ModKeybind"/> instance for the Shadow Teleport.
    /// </summary>
    public static ModKeybind ShadowTeleport { get; private set; }

    public override void Load()
    {
        base.Load();
        
        // TODO: Maybe don't make the default keybind the same for all keybinds.
        SwirlCloak = KeybindLoader.RegisterKeybind(Mod, $"{nameof(HeavenlyArsenal)}:{nameof(SwirlCloak)}", "F");
        HaemsongBind = KeybindLoader.RegisterKeybind(Mod, $"{nameof(HeavenlyArsenal)}:{nameof(HaemsongBind)}", "F");
        ShadowTeleport = KeybindLoader.RegisterKeybind(Mod, $"{nameof(HeavenlyArsenal)}:{nameof(ShadowTeleport)}", "F");
    }

    public override void Unload()
    {
        base.Unload();
        
        SwirlCloak = null;
        HaemsongBind = null;
        ShadowTeleport = null;
    }
}