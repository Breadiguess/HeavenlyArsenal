using HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using NoxusBoss.Assets;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameInput;

namespace HeavenlyArsenal.Common.Keybinds;



public class HeavenlyArsenalKeybinds : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        foreach (var handler in HAKeybindRegistry.Handlers)
            handler.Process(Player);
    }
}

