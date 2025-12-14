using HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.GameInput;

namespace HeavenlyArsenal.Common;

//idunwannahearit
public class HeavenlyArsenalKeybinds : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        var bloodArmorPlayer = Player.GetModPlayer<BloodArmorPlayer>();
        var modPlayer = Player.GetModPlayer<AwakenedBloodPlayer>();

        if (KeybindSystem.HaemsongBind.JustPressed && modPlayer.AwakenedBloodSetActive)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.ArmJutOut with
                {
                    Volume = 0.2f,
                    Pitch = -1f
                },
                Player.Center
            );

            bloodArmorPlayer.CurrentForm = bloodArmorPlayer.CurrentForm == BloodArmorForm.Offense
                ? BloodArmorForm.Defense
                : BloodArmorForm.Offense;

            modPlayer.CurrentForm = modPlayer.CurrentForm == AwakenedBloodPlayer.Form.Offense
                ? AwakenedBloodPlayer.Form.Defense
                : AwakenedBloodPlayer.Form.Offense;
        }

        var ShintoPlayer = Player.GetModPlayer<ShintoArmorPlayer>();

        if (KeybindSystem.ShadowTeleport.JustPressed && ShintoPlayer.SetActive)
        {
            ShintoPlayer.isShadeTeleporting = true;
        }

        var SwirlCloak = Player.GetModPlayer<CloakPlayer>();

        if (KeybindSystem.SwirlCloak.JustPressed && SwirlCloak.Active)
        {
            SwirlCloak.CreateSwirlVortex();
        }
    }
}

public class KeybindSystem : ModSystem
{
    public static ModKeybind SwirlCloak { get; private set; }

    public static ModKeybind ShadowTeleport { get; private set; }

    public static ModKeybind HaemsongBind { get; private set; }

    public override void Load()
    {
        // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
        SwirlCloak = KeybindLoader.RegisterKeybind(Mod, "SwirlCloak_Veil Cloak", "F");
        HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Swap BloodArmor Form", "F");
        ShadowTeleport = KeybindLoader.RegisterKeybind(Mod, "Shadow Teleport", "F");
    }

    public override void Unload()
    {
        HaemsongBind = null;
        ShadowTeleport = null;
    }
}