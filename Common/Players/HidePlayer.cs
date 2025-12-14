using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Players;

public sealed class HidePlayer : ModPlayer
{
    /// <summary>
    ///     Gets or sets whether to hide the player.
    /// </summary>
    public bool ShouldHide { get; set; }
    
    /// <summary>
    ///     Gets or sets whether to hide the player's weapon.
    /// </summary>
    public bool ShouldHideWeapon { get; set; }

    public override void ResetEffects()
    {
        base.ResetEffects();
        
        ShouldHide = false;
        ShouldHideWeapon = false;
    }
    
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        base.ModifyDrawInfo(ref drawInfo);
        
        if (!ShouldHide)
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
        
        if (!ShouldHideWeapon)
        {
            return;
        }
        
        Player.heldProj = -1;
    }
}
