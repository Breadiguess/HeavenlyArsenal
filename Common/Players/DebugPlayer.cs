using HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;
using System.Collections.Generic;
using Terraria.DataStructures;
//using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
//using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
//using HeavenlyArsenal.Content.Projectiles;

namespace HeavenlyArsenal.Common.Players;

internal class DebugPlayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Torso);
    }

   

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var Owner = drawInfo.drawPlayer;

        //prepCone(Owner);
        //float fallSpeedInterpolant = Luminance.Common.Utilities.Utilities.InverseLerp(25f, 130f, Owner.velocity.Y);
        var msg = "";

        //msg += $"{Owner.GetModPlayer<ShintoArmorBarrier>().barrier}\n"
        //    + $"{Owner.GetModPlayer<ShintoArmorBarrier>().timeSinceLastHit}\n";
        //msg += $"{fallSpeedInterpolant}\n {Owner.maxFallSpeed}";
        /*
        msg = $"modStealth: {Owner.Calamity().modStealth} \n"
            + $"rogueStealth: {Owner.Calamity().rogueStealth}\n"
            + $"Stealth Max:{Owner.Calamity().rogueStealthMax * 100}\n"
            + $"StealthAcceleration: {Owner.Calamity().stealthAcceleration}\n"
            + $"{Owner.Calamity().stealthGenMoving}";
        */
        //msg += $"{Owner.GetModPlayer<MedusaPlayer>().MedusaStacks}\n{Owner.GetModPlayer<MedusaPlayer>().MedusaTimer}";
        // msg += $"{Owner.GetModPlayer<PlaceholderName>().blood}";
        // if(Owner.HeldItem.type == ModContent.ItemType<ViscousWhip_Item>())
        //msg += $"{Owner.Center.ToTileCoordinates()}";
        if(!Main.gameMenu && Owner.GetModPlayer<LeechScarf_Player>().Active)
        for (int i = 0; i< Owner.GetModPlayer<LeechScarf_Player>().TendrilList.Count; i++)
        {

            msg += $"Slot: {Owner.GetModPlayer<LeechScarf_Player>().TendrilList[i].Slot}, Cooldown: {Owner.GetModPlayer<LeechScarf_Player>().TendrilList[i].Cooldown}\n";
        }
        //Utils.DrawBorderString(Main.spriteBatch, msg, Owner.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, 1.2f);

        //Main.EntitySpriteDraw(newLeech.leechTarget, Owner.Center - Main.screenPosition, null, Color.AntiqueWhite, 0, Vector2.Zero, 1, 0);
    }
}