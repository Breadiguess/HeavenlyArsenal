using System.ComponentModel;
using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Common.Configuration;

/// <summary>
///     The implementation of the client-side <see cref="ModConfig"/> of Heavenly Arsenal.
/// </summary>
public sealed class ClientSideConfiguration : ModConfig
{
    /// <summary>
    ///     Gets the singleton instance of <see cref="ClientSideConfiguration"/>. Shorthand for <see cref="ModContent.GetInstance"/>
    /// </summary>
    public static ClientSideConfiguration Instance => ModContent.GetInstance<ClientSideConfiguration>();

    public override ConfigScope Mode { get; } = ConfigScope.ClientSide;

    [Header("$Mods.HeavenlyArsenal.Config.Graphics")]
    [LabelArgs(typeof(CombatStim))]
    [BackgroundColor(192, 54, 64, 192)]
    [Range(0f, 6f)]
    [DefaultValue(true)]
    public bool StimVFX { get; set; }

    [LabelArgs(typeof(ItemID), nameof(ItemID.AviatorSunglasses))]
    [BackgroundColor(192, 54, 64, 192)]
    [DefaultValue(0.75f)]
    [Range(0.1f, 4f)]
    public float ChromaticAbberationMultiplier { get; set; }
}