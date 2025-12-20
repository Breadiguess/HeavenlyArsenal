using CalamityMod.Cooldowns;
using HeavenlyArsenal.Content.Items.Armor;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace HeavenlyArsenal.Common.Ui.Cooldowns;

public class BarrierDurability : CooldownHandler
{
    private static readonly Color ringColorLerpStart = new(220, 20, 78);

    private static readonly Color ringColorLerpEnd = new(0, 0, 0);

    public new static string ID => "BarrierDurability";

    public override bool CanTickDown => !instance.player.GetModPlayer<ShintoArmorBarrier>().BarrierActive || instance.timeLeft <= 0;

    public override bool ShouldDisplay => instance.player.GetModPlayer<ShintoArmorBarrier>().BarrierActive;

    public override LocalizedText DisplayName => Language.GetOrRegister("AntiShield Decay"); //"HeavenlyArsenal.Cooldowns.AntiShield.BarrierCooldown");

    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldown_Icon";

    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOutline_Icon";

    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOverlay_Icon";

    public override Color OutlineColor => new(220, 20, 70);

    public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override bool SavedWithPlayer => false;

    public override bool PersistsThroughDeath => false;

    private float AdjustedCompletion => instance.player.GetModPlayer<ShintoArmorBarrier>().barrier / (float)ShintoArmorBreastplate.ShieldDurabilityMax;

    public override void ApplyBarShaders(float opacity)
    {
        // Use the adjusted completion
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseOpacity(opacity);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseSaturation(AdjustedCompletion);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseColor(CooldownStartColor);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseSecondaryColor(CooldownEndColor);
        GameShaders.Misc["CalamityMod:CircularBarShader"].Apply();
    }

    public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        base.DrawExpanded(spriteBatch, position, opacity, scale);

        var Xoffset = instance.timeLeft > 9 ? -10f : -5;

        DrawBorderStringEightWay
        (
            spriteBatch,
            FontAssets.MouseText.Value,
            instance.timeLeft.ToString(),
            position + new Vector2(Xoffset, 4) * scale,
            Color.Lerp(ringColorLerpStart, Color.OrangeRed, 1 - instance.Completion),
            Color.Black,
            scale
        );
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        var sprite = Request<Texture2D>(Texture).Value;
        var outline = Request<Texture2D>(OutlineTexture).Value;
        var overlay = Request<Texture2D>(OverlayTexture).Value;

        scale *= MathF.Sin(Main.GlobalTimeWrappedHourly);
        // Draw the outline
        spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        // Draw the icon
        spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        // Draw the small overlay
        var lostHeight = (int)Math.Ceiling(overlay.Height * AdjustedCompletion);
        var crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
        spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        var Xoffset = instance.timeLeft > 9 ? -10f : -5;

        DrawBorderStringEightWay
        (
            spriteBatch,
            FontAssets.MouseText.Value,
            instance.timeLeft.ToString(),
            position + new Vector2(Xoffset, 4) * scale,
            Color.Lerp(ringColorLerpStart, Color.OrangeRed, 1 - instance.Completion),
            Color.Black,
            scale
        );
    }
}