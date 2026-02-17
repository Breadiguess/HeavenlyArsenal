using HeavenlyArsenal.Common.Keybinds;
using Terraria.GameInput;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players;

public sealed class BloodLightPurgeKeybindPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        base.ProcessTriggers(triggersSet);
            
        var parasitePlayer = Player.GetModPlayer<BloodBlightParasite_Player>();

        if (!KeybindSystem.BloodBlightPurge.JustPressed)
        {
            return;
        }
            
        BloodBlightParasite_Player.AttemptPurge(Player);
    }
}