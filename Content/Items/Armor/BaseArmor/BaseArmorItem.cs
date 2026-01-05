using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.BaseArmor
{
    /// <summary>
    /// Base class for armor items that automatically record applied stats
    /// and mirror them into tooltips.
    ///
    /// Authors should NOT override UpdateEquip directly.
    /// Instead, override <see cref="ApplyEquipStats"/> and apply stats using
    /// the <see cref="Stats"/> helper methods.
    ///
    /// Any stat applied via <see cref="Stats"/> will be:
    /// - applied normally at runtime
    /// - recorded for tooltip display
    /// - previewed safely when building tooltips (no side effects)
    /// </summary>
    public abstract class BaseArmorItem : ModItem
    {
        /// <summary>
        /// Runtime equip hook. Sealed to ensure stat recording is always correct.
        /// </summary>
        public sealed override void UpdateEquip(Player player)
        {
            StatsRecorder.BeginRuntime(Item.type);
            ApplyEquipStats(player);
            StatsRecorder.End();
        }
        /// <summary>
        /// Apply all equip-related stats here.
        ///
        /// This method fully replaces UpdateEquip for inheriting items.
        /// Anything valid to do in UpdateEquip may be done here, provided
        /// stats are applied through the <see cref="Stats"/> helper methods.
        /// </summary>
        /// <param name="player">The player wearing this armor piece.</param>
        protected abstract void ApplyEquipStats(Player player);
        /// <summary>
        /// Builds tooltip stat lines on demand by re-running
        /// <see cref="ApplyEquipStats"/> in preview mode.
        /// </summary>
        public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            StatsRecorder.BuildPreview(Item.type, Main.LocalPlayer, () => ApplyEquipStats(Main.LocalPlayer));
            StatsRecorder.InjectTooltips(Item.type, tooltips);
        }
    }


}
