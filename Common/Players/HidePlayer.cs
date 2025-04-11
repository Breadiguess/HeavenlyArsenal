using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Players;

public class HidePlayer : ModPlayer
{
    public bool ShouldHide { get; set; }
    public bool ShouldHideWeapon { get; set; }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (ShouldHide)
        {
            if (ShouldHideWeapon)
                Player.heldProj = -1;

            drawInfo.hideEntirePlayer = true;
            drawInfo.stealth = 1f;
            drawInfo.colorDisplayDollSkin = drawInfo.legsGlowColor = drawInfo.armGlowColor = drawInfo.bodyGlowColor = drawInfo.headGlowColor =
                drawInfo.colorLegs = drawInfo.colorShoes = drawInfo.colorPants = drawInfo.colorUnderShirt = drawInfo.colorShirt =
                drawInfo.colorBodySkin = drawInfo.colorHead = drawInfo.colorHair = drawInfo.colorEyes = drawInfo.colorEyeWhites =
                drawInfo.colorArmorLegs = drawInfo.colorArmorBody = drawInfo.colorArmorHead = Color.Transparent;
        }
    }

    public override void ResetEffects()
    {
        ShouldHide = false;
    }
}
