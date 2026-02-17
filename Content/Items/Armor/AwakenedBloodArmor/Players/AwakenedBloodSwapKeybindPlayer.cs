using HeavenlyArsenal.Common.Keybinds;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.GameInput;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public sealed class AwakenedBloodSwapKeybindPlayer : ModPlayer
{
    /// <summary>
    ///     The <see cref="SoundStyle"/> played when the player swaps between offense and defense forms of the Awakened Blood Armor.
    /// </summary>
    public static readonly SoundStyle SwapSound = GennedAssets.Sounds.Avatar.ArmJutOut with
    {
        Volume = 0.2f,
        Pitch = -1f
    };
    
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        base.ProcessTriggers(triggersSet);

        var awakenedBloodPlayer = Player.GetModPlayer<AwakenedBloodPlayer>();

        if (!awakenedBloodPlayer.Enabled || !KeybindSystem.HaemsongBind.JustPressed)
        {
            return;
        }

        SoundEngine.PlaySound(in SwapSound, Player.Center);

        awakenedBloodPlayer.Form = awakenedBloodPlayer.Form == AwakenedBloodForm.Offense ? AwakenedBloodForm.Defense : AwakenedBloodForm.Offense;
    }
}