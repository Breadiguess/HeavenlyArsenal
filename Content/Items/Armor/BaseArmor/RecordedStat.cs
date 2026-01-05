using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Armor.BaseArmor
{

    internal sealed class RecordedStat
    {
        private readonly string text;
        private readonly Mod mod;
        private readonly Color? colorOverride;
        private RecordedStat(Mod mod, string text, Color? colorOverride)
        {
            this.mod = mod;
            this.text = text;

            this.colorOverride = colorOverride;
        }

        public static RecordedStat FromTextOrKey(Mod mod, string textOrKey,Color? colorOverride, params object[] formatArgs)
        {
            string resolvedText;

            if (!string.IsNullOrEmpty(textOrKey) && textOrKey.StartsWith("Mods."))
            {
                resolvedText = Language.GetTextValue(textOrKey, formatArgs);
            }
            else
            {
                resolvedText = textOrKey;
            }

            return new RecordedStat(mod, resolvedText, colorOverride);
        }

        public static RecordedStat Raw(Mod mod, string text, Color? colorOverride)
        {
            return new RecordedStat(mod, text, colorOverride);
        }

        public TooltipLine ToTooltipLine()
        {
            var color = colorOverride;

            if (color.HasValue && color.Value.A == 0)
                color = null;

            return new TooltipLine(mod, "Stat", text)
            {
                IsModifier = true,
                OverrideColor = colorOverride ?? Color.White
            };
        }
    }



}
