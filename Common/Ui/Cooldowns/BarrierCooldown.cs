using CalamityMod.Cooldowns;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace HeavenlyArsenal.Common.Ui.Cooldowns;

public class EnrageCooldown : CooldownHandler
{
    private static readonly Color ringColorLerpStart = new(0, 0, 0);

    private static readonly Color ringColorLerpEnd = new(220, 20, 78);

    public override bool CanTickDown => true;

    public new static string ID => "enrageCooldown";

    public override bool ShouldDisplay => true;

    public override LocalizedText DisplayName => Language.GetOrRegister("Enrage Cooldown");

    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageCooldown_Icon";

    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageCooldown_Outline";

    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageCooldown_Overlay";

    public override bool SavedWithPlayer => false;

    public override bool PersistsThroughDeath => true;

    private float AdjustedCompletion => instance.player.GetModPlayer<ShintoArmorPlayer>().enrageCooldown / (float)ShintoArmorPlayer.EnrageCooldownMax;

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

        var Xoffset = instance.timeLeft / 60 > 9 ? -10f : -5;

        DrawBorderStringEightWay
        (
            spriteBatch,
            FontAssets.MouseText.Value,
            (instance.timeLeft / 60).ToString(),
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
            Color.Lerp(ringColorLerpStart, ringColorLerpEnd, 1 - instance.Completion),
            Color.Black,
            scale
        );
    }
}

public class EnrageTimerVisual : CooldownHandler
{
    private static readonly Color ringColorLerpStart = new(0, 0, 0);

    private static Color ringColorLerpEnd = new(220, 20, 78);

    public override bool CanTickDown => true;

    public new static string ID => "EnrageTimerVisual";

    public override bool ShouldDisplay => true;

    public override LocalizedText DisplayName => Language.GetOrRegister("Enrage Time");

    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageDuration_Icon";

    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageDuration_Outline";

    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/EnrageDuration_Overlay";

    public override bool SavedWithPlayer => false;

    public override bool PersistsThroughDeath => false;

    public override Color OutlineColor => new(220, 20, 70);

    public override Color CooldownEndColor => new(0, 0, 0);

    private float AdjustedCompletion => instance.timeLeft / (float)ShintoArmorPlayer.EnrageCooldownMax;

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
            (instance.timeLeft / 60).ToString(),
            position + new Vector2(Xoffset, 4) * scale,
            Color.Lerp(Color.Crimson, Color.DarkRed, 1 - instance.Completion),
            Color.Black,
            scale
        );
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        var sprite = Request<Texture2D>(Texture).Value;
        var outline = Request<Texture2D>(OutlineTexture).Value;
        var overlay = Request<Texture2D>(OverlayTexture).Value;

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