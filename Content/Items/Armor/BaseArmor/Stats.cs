namespace HeavenlyArsenal.Content.Items.Armor.BaseArmor
{

    internal sealed class StatSystemBootstrap : ModSystem
    {
        public override void Load()
        {
            Stats.OwnerMod = Mod;
        }

        public override void Unload()
        {
            Stats.OwnerMod = null;
        }
    }
    /// <summary>
    /// Helper API for applying player stats while automatically recording
    /// tooltip lines.
    ///
    /// All vanilla stat modifications should go through this class.
    /// For non-vanilla or modded stats, use <see cref="RecordCustom"/>.
    ///
    /// IMPORTANT:
    /// If a text parameter starts with "Mods.", it is treated as a localization key.
    /// Otherwise, it is treated as raw tooltip text.
    /// </summary>
    public static class Stats
    {
        internal static Mod OwnerMod;

        public static void AddCrit(Player p, int crit, string locOverride = null, Color? color = null)
        {
            if (!StatsRecorder.IsPreview)
                p.GetCritChance<GenericDamageClass>() += crit;

            RecordFlat($"Mods.{OwnerMod.Name}.Stats.Crit", crit, locOverride, color ?? Color.White);
        }

        public static void AddDamage(Player p, float percent, string locOverride = null, Color? color = null)
        {
            if (!StatsRecorder.IsPreview)
                p.GetDamage<GenericDamageClass>() += percent;

            RecordPercent($"Mods.{OwnerMod.Name}.Stats.Damage", percent, locOverride, color ?? Color.White);
        }

        public static void AddMoveSpeed(Player p, float percent, string locOverride = null, Color? color = null)
        {
            if (!StatsRecorder.IsPreview)
                p.moveSpeed += percent;

            RecordPercent($"Mods.{OwnerMod.Name}.Stats.MoveSpeed", percent, locOverride, color ?? Color.White);
        }
        /// <summary>
        /// Records a custom stat effect that is not covered by built-in helpers.
        ///
        /// This method is intended for:
        /// - modded stats
        /// - cross-mod integrations
        /// - conditional or unusual mechanics
        ///
        /// The stat application is skipped when building tooltips (preview mode),
        /// preventing unintended side effects.
        /// </summary>
        /// <param name="player">The affected player.</param>
        /// <param name="apply">
        /// Action that applies the stat effect to the player.
        /// This is only executed at runtime, not during tooltip preview.
        /// </param>
        /// <param name="textOrKey">
        /// Tooltip text or localization key.
        ///
        /// If this string starts with "Mods.", it is treated as a localization key
        /// and formatted using <paramref name="formatArgs"/>.
        ///
        /// Otherwise, it is treated as raw tooltip text.
        /// </param>
        /// <param name="colorOverride">
        /// Optional color override for the tooltip line.
        /// If null, the default stat color is used.
        /// </param>
        /// <param name="formatArgs">
        /// Optional format arguments used when <paramref name="textOrKey"/>
        /// is treated as a localization key.
        /// </param>
        public static void RecordCustom(
            Player player,
            System.Action<Player> apply,
            string textOrKey,
            Color color = default,
            params object[] formatArgs
        )
        {
            // Wildcard: do NOT run apply in preview (no side effects)
            if (!StatsRecorder.IsPreview)
                apply?.Invoke(player);

            EnsureInitialized();
            StatsRecorder.Record(RecordedStat.FromTextOrKey(OwnerMod, textOrKey, color, formatArgs));
        }

        private static void RecordPercent(string defaultKey, float value, string overrideKey, Color color = default)
        {
            EnsureInitialized();
            int pct = (int)(value * 100f);
            string key = overrideKey ?? defaultKey;
            StatsRecorder.Record(RecordedStat.FromTextOrKey(OwnerMod, key, color, pct));
        }

        private static void RecordFlat(string defaultKey, int value, string overrideKey, Color color = default)
        {
            EnsureInitialized();
            string key = overrideKey ?? defaultKey;
            StatsRecorder.Record(RecordedStat.FromTextOrKey(OwnerMod, key, color, value));
        }

        private static void EnsureInitialized()
        {
            if (OwnerMod == null)
                throw new System.InvalidOperationException("Stats.OwnerMod was not initialized.");
        }
    }


}
