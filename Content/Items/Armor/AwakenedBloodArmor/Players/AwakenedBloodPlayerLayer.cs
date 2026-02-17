using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public sealed class AwakenedBloodPlayerLayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Head);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        // drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
        return true;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;
        var awakenedBloodPlayer = drawPlayer.GetModPlayer<AwakenedBloodPlayer>();

        if (!awakenedBloodPlayer.Enabled && awakenedBloodPlayer.Blood <= 0 || !awakenedBloodPlayer.BloodBoostActive)
        {
            return;
        }

        var shader = ShaderManager.GetFilter("HeavenlyArsenal.BloodFrenzy");

        shader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly);
        shader.TrySetParameter("intensityFactor", 1);
        shader.TrySetParameter("opacity", 00);
        shader.TrySetParameter("psychadelicExponent", 0);
        shader.TrySetParameter("psychedelicColorTint", Color.Crimson.ToVector4());
        shader.TrySetParameter("colorAccentuationFactor", 0f);

        shader.SetTexture(GennedAssets.Textures.Extra.BloodWater, 2);

        shader.Activate();
    }

    protected void DrawDebug(ref PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;
        var awakenedBloodPlayer = drawPlayer.GetModPlayer<AwakenedBloodPlayer>();
        
        var bloodText = $"Blood: {awakenedBloodPlayer.Blood}";
        var gainTimerText = $"Blood Gain Timer: {awakenedBloodPlayer.BloodGainTimer}";
        var clotText = $"Clot: {awakenedBloodPlayer.Clot}";
        var decayTimeText = $"Blood Decay Time: {awakenedBloodPlayer.ClotDecayTimer}";
        var bloodBoostText = $"Blood Boost Active: {awakenedBloodPlayer.BloodBoostActive}";
        
        var text = bloodText + ", " + gainTimerText + ", " + clotText + ", " + decayTimeText + ", " + bloodBoostText;
        
        var position = drawPlayer.Center - Main.screenPosition + new Vector2(0f, drawPlayer.gfxOffY);
        
        var offset = new Vector2(120f);
        
        Utils.DrawBorderString(Main.spriteBatch, text, position - offset, Color.AntiqueWhite);
    }
}