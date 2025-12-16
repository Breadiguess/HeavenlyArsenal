using Terraria.DataStructures;

namespace HeavenlyArsenal.Common.Players;

public sealed class HidePlayer : ModPlayer
{
    /// <summary>
    ///     Gets or sets whether to hide the player.
    /// </summary>
    public bool Enabled { get; set; }

    public override void ResetEffects()
    {
        base.ResetEffects();

        Enabled = false;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        base.ModifyDrawInfo(ref drawInfo);

        if (!Enabled)
        {
            return;
        }

        drawInfo.hideEntirePlayer = true;

        drawInfo.stealth = 1f;

        drawInfo.colorDisplayDollSkin = Color.Transparent;

        drawInfo.legsGlowColor = Color.Transparent;
        drawInfo.armGlowColor = Color.Transparent;
        drawInfo.bodyGlowColor = Color.Transparent;
        drawInfo.headGlowColor = Color.Transparent;

        drawInfo.colorLegs = Color.Transparent;
        drawInfo.colorShoes = Color.Transparent;
        drawInfo.colorPants = Color.Transparent;
        drawInfo.colorUnderShirt = Color.Transparent;
        drawInfo.colorShirt = Color.Transparent;

        drawInfo.colorBodySkin = Color.Transparent;
        drawInfo.colorHead = Color.Transparent;
        drawInfo.colorHair = Color.Transparent;
        drawInfo.colorEyes = Color.Transparent;
        drawInfo.colorEyeWhites = Color.Transparent;

        drawInfo.colorArmorLegs = Color.Transparent;
        drawInfo.colorArmorBody = Color.Transparent;
        drawInfo.colorArmorHead = Color.Transparent;

        Player.heldProj = -1;
    }
}