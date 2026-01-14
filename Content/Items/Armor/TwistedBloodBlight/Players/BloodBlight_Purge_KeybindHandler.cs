using HeavenlyArsenal.Common.Keybinds;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players
{
    internal class BloodBlight_Purge_KeybindHandler : ModPlayer, IKeybindHandler
    {
        public override void Load()
        {
            HAKeybindRegistry.Register(new BloodBlight_Purge_KeybindHandler());
        }
        public void Process(Player player)
        {
            var awakened = player.GetModPlayer<BloodBlightParasite_Player>();
            
            if (!KeybindSystem.BloodBlight_Purge.JustPressed)
                return;
            BloodBlightParasite_Player.AttemptPurge(player);
            
        }
    }
}
