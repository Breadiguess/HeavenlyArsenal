using HeavenlyArsenal.Common.Keybinds;
using Terraria.GameInput;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public sealed class AwakenedBloodParryKeybindPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        base.ProcessTriggers(triggersSet);

        var awakened = Player.GetModPlayer<AwakenedBloodPlayer>();

        if (!awakened.Enabled || !KeybindSystem.BloodArmorParry.JustPressed)
        {
            return;
        }

        Player.Parry(AwakenedBloodParryPlayer.BLOOD_THORN_PARRY_TIME);
    }
}