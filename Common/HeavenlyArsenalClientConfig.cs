using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Common
{
    class HeavenlyArsenalClientConfig : ModConfig
    {
        public static HeavenlyArsenalClientConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => true;

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
    class HeavenlyArsenalServerConfig : ModConfig
    {
        [Header("$Mods.HeavenlyArsenal.Configs.ServerConfig")]
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(false)] // Fixed: Ensure DefaultValueAttribute is recognized
        public bool EnableSpecialItems { get; set; }
    }
}
