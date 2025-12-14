using CalamityMod.Cooldowns;
using Terraria.Audio;
using Terraria.Localization;

namespace HeavenlyArsenal.Common.Ui.Cooldowns;

public class BarrierRecharge : CooldownHandler
{
    private static readonly Color ringColorLerpStart = new(0, 0, 0);

    private static readonly Color ringColorLerpEnd = new(220, 220, 220);

    public new static string ID => "BarrierRecharge";

    public override bool ShouldDisplay => true;

    public override LocalizedText DisplayName => Language.GetOrRegister("AntiShield Inversion");

    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Icon";

    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Outline";

    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Overlay";

    public override bool SavedWithPlayer => false;

    public override bool PersistsThroughDeath => false;

    public override Color OutlineColor => new(133, 204, 237);

    public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override SoundStyle? EndSound => AssetDirectory.Sounds.Items.Armor.Antishield_Regen;

    public override bool ShouldPlayEndSound => true;

    //public override void Tick() => instance.player.Calamity().playedSpongeShieldSound = false;
    // When the recharge period completes, grant 1 point of shielding immediately so the rest my refill normally.
    // The shield durability cooldown is added elsewhere, in Misc Effects.
    public override void OnCompleted()
    {
        SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Regen);
    }
}