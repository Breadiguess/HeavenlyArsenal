using CalamityMod;
using CalamityMod.Cooldowns;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Armor;

public class AbyssDashCooldown : CooldownHandler
{
    public static new string ID => "AbyssDash";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
    public override string Texture => "CalamityMod/Cooldowns/GodSlayerDash";
    public override Color OutlineColor => Color.Lerp(new Color(255, 66, 203), new Color(252, 109, 202), instance.Completion);
    public override Color CooldownStartColor => new Color(252, 109, 202);
    public override Color CooldownEndColor => new Color(255, 254, 254);
}