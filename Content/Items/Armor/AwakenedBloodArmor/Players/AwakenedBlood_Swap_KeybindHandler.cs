using HeavenlyArsenal.Common.Keybinds;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players
{
    internal class AwakenedBlood_Swap_KeybindHandler : ModPlayer, IKeybindHandler
    {
        public override void Load()
        {
            HAKeybindRegistry.Register(new AwakenedBlood_Swap_KeybindHandler());
        }
        public void Process(Player player)
        {
            var awakened = player.GetModPlayer<AwakenedBloodPlayer>();
            if (!awakened.AwakenedBloodSetActive)
                return;

            if (!KeybindSystem.HaemsongBind.JustPressed)
                return;


            SoundEngine.PlaySound(
                GennedAssets.Sounds.Avatar.ArmJutOut with
                {
                    Volume = 0.2f,
                    Pitch = -1f
                },
                player.Center
            );

       

            awakened.CurrentForm =
                awakened.CurrentForm == AwakenedBloodPlayer.Form.Offense
                    ? AwakenedBloodPlayer.Form.Defense
                    : AwakenedBloodPlayer.Form.Offense;
        }
    }
}
