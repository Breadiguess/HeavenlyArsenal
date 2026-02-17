using CalamityMod.Cooldowns;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;

public class LeechScarfCooldown : CooldownHandler
{
    private static readonly Color ringColorLerpStart = new(220, 20, 78);

    private static readonly Color ringColorLerpEnd = new(0, 0, 0);

    public new static string ID => "LeechScarfCooldown";
    public override bool CanTickDown => false;
    public override bool ShouldDisplay =>
    instance.player.GetModPlayer<LeechScarfPlayer>().Active;

    public override LocalizedText DisplayName => Language.GetOrRegister("Leech Scarf Cooldown"); //"HeavenlyArsenal.Cooldowns.AntiShield.BarrierCooldown");

    public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldown_Icon";

    public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOutline_Icon";

    public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldownOverlay_Icon";

    public override Color OutlineColor => new(220, 20, 70);

    public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);

    public override bool SavedWithPlayer => false;

    public override bool PersistsThroughDeath => false;
    int TendrilCount =>
    instance.player.GetModPlayer<LeechScarfPlayer>().Tendrils.Count;

    float SegmentAngle => MathHelper.TwoPi / TendrilCount;


    float GetTendrilCompletion(LeechScarfPlayer.Tendril t, int maxCooldown)
    {
        if (t.Active)
            return 1f;

        if (t.Cooldown <= 0)
            return 1f;

        return 1f - t.Cooldown / (float)maxCooldown;
    }

    public override void ApplyBarShaders(float opacity)
    {
        var mp = instance.player.GetModPlayer<LeechScarfPlayer>();
        var tendrils = mp.Tendrils;

        int count = tendrils.Count;
        if (count == 0)
            return;
        float slice = 1f / LeechScarfPlayer.MAX_TENDRILS;

        for (int i = 0; i < count; i++)
        {
            var t = tendrils[i];
            float completion = GetTendrilCompletion(t, LeechScarfPlayer.MAX_TENDRIL_COOLDOWN);

            float rotation = i * slice * MathHelper.TwoPi;

            Color startColor = Color.Lerp(ringColorLerpStart, ringColorLerpEnd, 1f - completion);
            Color endColor = ringColorLerpEnd;
           
            float startFraction = i * slice;

            GameShaders.Misc["CalamityMod:CircularBarShader"]
            .UseOpacity(opacity)
            .UseSaturation(slice * completion)
            .UseColor(startColor)
            .UseSecondaryColor(endColor)
            .UseShaderSpecificData(new Vector4(startFraction, 0f, 0f, 0f))
            .Apply();


        }
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

        var lostHeight = (int)Math.Ceiling((double)overlay.Height);
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