using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common
{
    //idunwannahearit
    public class HeavenlyArsenalKeybinds : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            var bloodArmorPlayer = Player.GetModPlayer<BloodArmorPlayer>();
            var modPlayer = Player.GetModPlayer<AwakenedBloodPlayer>();
            if (KeybindSystem.HaemsongBind.JustPressed)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Volume = 0.2f, Pitch = -1f }, Player.Center, null);
               
                bloodArmorPlayer.CurrentForm = bloodArmorPlayer.CurrentForm == BloodArmorForm.Offense
                    ? BloodArmorForm.Defense
                    : BloodArmorForm.Offense;
               
                modPlayer.CurrentForm = modPlayer.CurrentForm == AwakenedBloodPlayer.Form.Offsense
                    ? AwakenedBloodPlayer.Form.Defense
                    : AwakenedBloodPlayer.Form.Offsense;
            }

        }
    }

    public class KeybindSystem : ModSystem
    {
        public static ModKeybind HaemsongBind { get; private set; }
        public override void Load()
        {
            // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
            HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Toggle Haemsong Mode", "F");
        }
        public override void Unload()
        {
            HaemsongBind = null;
        }
    }
}