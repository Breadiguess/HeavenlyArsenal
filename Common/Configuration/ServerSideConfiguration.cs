using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Common.Configuration;

/// <summary>
///     The implementation of the server-side <see cref="ModConfig"/> of Heavenly Arsenal.
/// </summary>
public sealed class ServerSideConfiguration : ModConfig
{
    /// <summary>
    ///     Gets the singleton instance of <see cref="ServerSideConfiguration"/>. Shorthand for <see cref="ModContent.GetInstance"/>
    /// </summary>
    public static ServerSideConfiguration Instance => ModContent.GetInstance<ServerSideConfiguration>();
    
    [Header("$Mods.HeavenlyArsenal.Configs.ServerConfig")]
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)]
    public bool EnableSpecialItems { get; set; }
}