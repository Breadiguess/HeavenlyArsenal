using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor
{
    internal class ShintoWingManager : ModPlayer
    {
        public bool Active;
        public override void PostUpdateEquips()
        {
            if (!Active)
                return;

            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ShintoArmorWings", EquipType.Wings);
            bool thing = Player.equippedWings == null;

            if (thing)
            {
                Player.wings = ShintoArmorWings.WingSlotID;
                //Player.wings = wingSlot;
                Player.wingsLogic = wingSlot;


                Player.wingTime = 1000;
                Player.wingTimeMax = 1000;
                // Player.equippedWings = Player.armor[1];


                if (ModLoader.HasMod("CalamityMod"))
                {
                    ModLoader.GetMod("CalamityMod").Call("ToggleInfiniteFlight", Player, true);
                }
                Player.noFallDmg = true;
            }

        }

        public override void ResetEffects()
        {
            Active = false;
        }
    }
}
