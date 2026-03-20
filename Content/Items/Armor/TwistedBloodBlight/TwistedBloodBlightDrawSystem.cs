using System.Text;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight;

[Autoload(Side = ModSide.Client)]
public sealed class TwistedBloodBlightDrawSystem : ModSystem
{
    public override void Load()
    {
        base.Load();
        
        On_Main.DrawInfernoRings += Main_DrawInfernoRings_Hook;
    }

    private static void Main_DrawInfernoRings_Hook(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);
        
        DrawSaturation();

#if DEBUG
        DrawSaturationDetails();
#endif
    }

    private static void DrawSaturation()
    {
        var player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<TwistedBloodBlightPlayer>();

        var font = FontAssets.MouseText.Value;
        
        var text = $"Saturation: {modPlayer.Saturation} / {TwistedBloodBlightPlayer.MAX_SATURATION}";
        var size = font.MeasureString(text);

        const float padding = 8f;
        
        var offset = new Vector2(size.X + padding, 0f);
        var position = player.Center - Main.screenPosition + new Vector2(0f, player.gfxOffY) - offset;

        var origin = size / 2f;
        
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, position, Color.Red, 0f, origin, Vector2.One);
    }
    
#if DEBUG
    private static void DrawSaturationDetails()
    {
        var player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<TwistedBloodBlightPlayer>();

        var font = FontAssets.MouseText.Value;

        var builder = new StringBuilder();
        
        builder.AppendLine($"Gain Buffer: {modPlayer.SaturationGainBuffer} / {TwistedBloodBlightPlayer.MAX_SATURATION_GAIN_BUFFER} @ {modPlayer.SaturationGainBuffer / 60} (seconds)");
        builder.AppendLine($"Decay Buffer: {modPlayer.SaturationDecayBuffer} / {TwistedBloodBlightPlayer.MAX_SATURATION_DECAY_BUFFER} @ {modPlayer.SaturationDecayBuffer / 60} (seconds)");
        
        var text = builder.ToString();
        var size = font.MeasureString(text);

        const float padding = 8f;
        
        var offset = new Vector2(size.X + padding, 0f);
        var position = player.Center - Main.screenPosition + new Vector2(0f, player.gfxOffY) + offset;

        var origin = size / 2f;
        
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, position, Color.Blue, 0f, origin, Vector2.One);
    }
}
#endif