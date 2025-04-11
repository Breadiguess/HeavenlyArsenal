using System;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace HeavenlyArsenal.Content.Items.Armor;

public class BarrierDurability : CooldownHandler
{
    private static Color ringColorLerpStart = new Color(220, 20, 78);
    private static Color ringColorLerpEnd = new Color(0, 0, 0);

    private float AdjustedCompletion => instance.timeLeft / (float)ShintoArmorBreastplate.ShieldDurabilityMax;

    public static new string ID => "BarrierDurability";
    public override bool CanTickDown => !instance.player.GetModPlayer<ShintoArmorPlayer>().SetActive || instance.timeLeft <= 0;
    public override bool ShouldDisplay => instance.player.GetModPlayer<ShintoArmorPlayer>().SetActive;
    public override LocalizedText DisplayName => Language.GetOrRegister("AntiShield Decay");//"HeavenlyArsenal.Cooldowns.AntiShield.BarrierCooldown");
    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldown_Icon";
    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOutline_Icon";
    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOverlay_Icon";
    public override Color OutlineColor => new Color(220, 20, 70);
    public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
    public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
    public override bool SavedWithPlayer => false;
    public override bool PersistsThroughDeath => false;

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

        float Xoffset = instance.timeLeft > 9 ? -10f : -5;
        DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.timeLeft.ToString(), position + new Vector2(Xoffset, 4) * scale, Color.Lerp(ringColorLerpStart, Color.OrangeRed, 1 - instance.Completion), Color.Black, scale);
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        Texture2D sprite = Request<Texture2D>(Texture).Value;
        Texture2D outline = Request<Texture2D>(OutlineTexture).Value;
        Texture2D overlay = Request<Texture2D>(OverlayTexture).Value;

        // Draw the outline
        spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        // Draw the icon
        spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        // Draw the small overlay
        int lostHeight = (int)Math.Ceiling(overlay.Height * AdjustedCompletion);
        Rectangle crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
        spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        float Xoffset = instance.timeLeft > 9 ? -10f : -5;
        DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.timeLeft.ToString(), position + new Vector2(Xoffset, 4) * scale, Color.Lerp(ringColorLerpStart, Color.OrangeRed, 1 - instance.Completion), Color.Black, scale);
    }
}

public class BarrierRecharge : CooldownHandler
{
    private static Color ringColorLerpStart = new Color(0, 0, 0);
    private static Color ringColorLerpEnd = new Color(220, 220, 220);

    public static new string ID => "BarrierRecharge";
    public override bool ShouldDisplay => true;
    public override LocalizedText DisplayName => Language.GetOrRegister("AntiShield Inversion");
    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Icon";
    public override string OutlineTexture => "CalamityMod/Cooldowns/SpongeOutline";
    public override string OverlayTexture => "CalamityMod/Cooldowns/SpongeOverlay";
    public override bool SavedWithPlayer => false;
    public override bool PersistsThroughDeath => false;
    public override Color OutlineColor => new Color(133, 204, 237);
    public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
    public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
    public override SoundStyle? EndSound => ShintoArmorBreastplate.ActivationSound;
    public override bool ShouldPlayEndSound => true;


    //public override void Tick() => instance.player.Calamity().playedSpongeShieldSound = false;
    // When the recharge period completes, grant 1 point of shielding immediately so the rest my refill normally.
    // The shield durability cooldown is added elsewhere, in Misc Effects.
    public override void OnCompleted()
    {
        SoundEngine.PlaySound(ShintoArmorBreastplate.BreakSound);
    }
}