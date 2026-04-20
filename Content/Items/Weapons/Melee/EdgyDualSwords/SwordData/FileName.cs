using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public class ComboMusicScene : ModSceneEffect
    {
        public static int MusicSlot;

        public override void Load()
        {
            MusicSlot = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/BIGSHOT");

        }

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/BIGSHOT");

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override bool IsSceneEffectActive(Player player)
        {
            bool active = player.GetModPlayer<SwordDevPlayer>().KeepMusicAlive;

            //if (Main.myPlayer == player.whoAmI && Main.GameUpdateCount % 30 == 0)
           //     Main.NewText($"Scene active={active}, slot={MusicSlot}");

            return active;
        }

        public override float GetWeight(Player player)
        {
            float weight = player.GetModPlayer<SwordDevPlayer>().MusicFade;

            //if (Main.myPlayer == player.whoAmI && Main.GameUpdateCount % 30 == 0)
            //    Main.NewText($"Scene weight={weight:F2}");

            return weight;
        }
      
    }
}
