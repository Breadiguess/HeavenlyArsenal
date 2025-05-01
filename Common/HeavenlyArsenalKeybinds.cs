using HeavenlyArsenal.Content.Items.Armor.Haemsong;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common
{
    public class HeavenlyArsenalKeybinds : ModPlayer
    {
        //private readonly int LearningExampleKeybindHeldTimer;
        //private int LearningExampleKeybindDoubleTapTimer;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            /*
            if (KeybindSystem.DashBind.JustPressed)
            {
                Player.GetModPlayer<VectorModPlayer>().quickDash = true;
            }
            */

            

            var bloodArmorPlayer = Player.GetModPlayer<BloodArmorPlayer>();
            var bloodPlayer = Player.GetModPlayer<BloodPlayer>();
            if (KeybindSystem.HaemsongBind.JustPressed && bloodArmorPlayer.BloodArmorEquipped)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Volume = 0.2f, Pitch = -1f }, Player.Center, null);

                bloodArmorPlayer.CurrentForm = bloodArmorPlayer.CurrentForm == BloodArmorForm.Offense
                    ? BloodArmorForm.Defense
                    : BloodArmorForm.Offense;


                if (bloodPlayer.offenseMode)
                {
                    bloodPlayer.offenseMode = false;
                   
                }
                //bloodPlayer.CurrentForm = bloodPlayer.CurrentForm == BloodArmorForm.Offense
                //    ? BloodArmorForm.Defense
                //    : BloodArmorForm.Offense;
            
            }

        }
    }

    public class KeybindSystem : ModSystem
    {
        public static ModKeybind DashBind { get; private set; }
        public static ModKeybind HaemsongBind { get; private set; }
        public override void Load()
        {
            // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
            DashBind = KeybindLoader.RegisterKeybind(Mod, "Quick Dash", "Mouse4");
            HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Toggle Haemsong Mode", "F");
        }
        public override void Unload()
        {
            DashBind = null;
            HaemsongBind = null;
        }
    }
}