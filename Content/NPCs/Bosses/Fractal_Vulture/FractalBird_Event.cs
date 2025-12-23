using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    internal class FractalBird_Event : ModSystem
    {
        

        public override void PostUpdateEverything()
        {

            if (DownedBossSystem.downedYharon)
            {
                if (Main.GameUpdateCount % 300 == 0 && Main.rand.NextBool(5) && voidVulture.Myself is null)
                {
                    FakeFlowerPlacementSystem.TryPlaceFlowerAroundGenesis();
                }
            }
        }
    }
}
