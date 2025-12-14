using System.ComponentModel;
using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Common;

internal class HeavenlyArsenalClientConfig : ModConfig
{
    public static HeavenlyArsenalClientConfig Instance;

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
    {
        return true;
    }

    #region Graphics Changes

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

    #endregion
}

internal class HeavenlyArsenalServerConfig : ModConfig
{
    [Header("$Mods.HeavenlyArsenal.Configs.ServerConfig")]
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)] // Fixed: Ensure DefaultValueAttribute is recognized
    public bool EnableSpecialItems { get; set; }
}