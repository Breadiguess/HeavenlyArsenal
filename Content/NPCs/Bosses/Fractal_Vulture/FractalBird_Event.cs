using CalamityMod;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    internal class FractalBird_Event : ModSystem
    {
        public override void LoadWorldData(TagCompound tag)
        {
            hasBirdBeenDefeated = tag.GetBool("FractalBirdDefeated");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["FractalBirdDefeated"] = hasBirdBeenDefeated;
        }
        public bool hasBirdBeenDefeated;
        public override void OnWorldLoad()
        {
            hasBirdBeenDefeated = false;
        }

        public override void OnWorldUnload()
        {
            hasBirdBeenDefeated = false;
        }
        public override void PostUpdateEverything()
        {

            if (DownedBossSystem.downedYharon) 
            {
                if (Main.GameUpdateCount % 300 == 0 && Main.rand.NextBool(5) && voidVulture.Myself is null && !hasBirdBeenDefeated)
                {
                    if (FakeFlowerRender.Count < 1)
                    {
                        
                        FakeFlowerPlacementSystem.TryPlaceFlowerAroundGenesis();

                    }
                }
            }
        }
    }
}
