using HeavenlyArsenal.Content.Items.Consumables;
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
    class HeavenlyArsenalConfig : ModConfig
    {
        public static HeavenlyArsenalConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => true;

        // thank you iban
        #region Graphics Changes
        [Header("$Mods.HeavenlyArsenal.Config.Graphics")]

        [LabelArgs(typeof(CombatStim))] // Fixed: Use typeof to pass a type as a constant expression
        [BackgroundColor(192, 54, 64, 192)]
        [Range(0f, 6f)]
        [DefaultValue(1)]
        public bool StimVFX {get; set;}


        [LabelArgs(typeof(ItemID), nameof(ItemID.AviatorSunglasses))] // Fixed: Use typeof and nameof for constant expressions
        [BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(0.75f)]
        [Range(0.1f, 4f)]
        public float ChromaticAbberationMultiplier { get; set; }

        #endregion
        /*
        [LabelArgs(typeof(ItemID), nameof(ItemID.SoulofLight))] // Fixed: Use typeof and nameof for constant expressions
        [BackgroundColor(192, 54, 64, 192)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0, 1000)]
        [DefaultValue(500)]
        public int ParticleLimit { get; set; }

        [LabelArgs(typeof(ItemID), nameof(ItemID.DrumSet))] // Fixed: Use typeof and nameof for constant expressions
        [BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(1f)]
        [Range(0f, 2f)]
        public float ScreenshakeMultiplier { get; set; }

        [LabelArgs(typeof(ItemID), nameof(ItemID.AviatorSunglasses))] // Fixed: Use typeof and nameof for constant expressions
        [BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(1f)]
        [Range(0f, 2f)]
        public float ChromaticAbberationMultiplier { get; set; }

        

        #region UI Changes
        [Header("$Mods.CalamityFables.Configs.FablesConfig.SectionTitle.UI")]

        [LabelArgs(typeof(ItemID), nameof(ItemID.TatteredWoodSign))] // Fixed: Use typeof and nameof for constant expressions
        [BackgroundColor(192, 54, 64, 192)]
        [DefaultValue(true)]
        public bool BossIntroCardsActivated { get; set; }

        [BackgroundColor(0, 0, 0, 0)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 6f)]
        [DefaultValue(1)]
        public bool VanillaCooldownDisplay { get; set; }
        */

    }
}
