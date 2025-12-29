using HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner;
using HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
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
        var parasite = Owner.GetModPlayer<BloodBlightParasite_Player>();

        if (parasite == null)
            return;

        if (!parasite.Active)
            return;
        //prepCone(Owner);
        //float fallSpeedInterpolant = Luminance.Common.Utilities.Utilities.InverseLerp(25f, 130f, Owner.velocity.Y);
        string msg =
        $"""
        [BLOODBLIGHT DEBUG]
        Saturation: {parasite.BloodSaturation:F1} / {parasite.BloodSaturationMax}
        Band: {parasite.CurrentBand}
        State: {parasite.CurrentState}
        Crashing: {parasite.IsCrashing}
        Morph: {parasite.CurrentMorph?.Name ?? "None"}
        DominantClass: {parasite.DominantClass?.Name ?? "None"}
        DominantTimer: {parasite.DominantClassTimer}
        InCombat: {parasite.InCombat}
        CombatTimer: {parasite.CombatTimer}
        AscensionTimer: {parasite.AscensionTimer};
        Controller: {parasite.ConstructController?.GetType().Name ?? "None"}
        """;

        if (parasite.ConstructController is SummonerBloodController summoner)
        {
            msg += $"\nOvermind: {summoner.overmindActive}";
        }
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
        // msg += $"Authority: {Owner.GetModPlayer<Aoe_Rifle_Player>().Authority}\n AuthorityTimer: {Owner.GetModPlayer<Aoe_Rifle_Player>().AuthorityTimer}\n {Owner.GetModPlayer<Aoe_Rifle_Player>().BulletCount}";
        if (!Main.gameMenu && Owner.GetModPlayer<LeechScarf_Player>().Active)
        for (int i = 0; i< Owner.GetModPlayer<LeechScarf_Player>().TendrilList.Count; i++)
        {

            //msg += $"Slot: {Owner.GetModPlayer<LeechScarf_Player>().TendrilList[i].Slot}, Cooldown: {Owner.GetModPlayer<LeechScarf_Player>().TendrilList[i].Cooldown}\n";
        }
        Utils.DrawBorderString(Main.spriteBatch, msg, Owner.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -0.2f);

        //Main.EntitySpriteDraw(newLeech.leechTarget, Owner.Center - Main.screenPosition, null, Color.AntiqueWhite, 0, Vector2.Zero, 1, 0);
    }
}