using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
namespace HeavenlyArsenal.Core.Overrides
{
    class CeaselessRiftOverride : ModSystem
    {
        
       
        public override void Load()
        {

             // Hook the static getter for CeaselessVoidRift.CanEnterRift
            NoxusBoss.Content.NPCs.Bosses.CeaselessVoid.CeaselessVoidRift.CanEnterRift +=
                (orig_get) =>
                {
                // First run the original logic:
                bool baseResult = orig_get();
                // Then OR in your override flag:
                return RiftOverrides.ForceCanEnter || baseResult;
                };
        }
    }
}

