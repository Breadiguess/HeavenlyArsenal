using HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Core
{
    internal class HitSquadSystem : ModSystem
    {
        public HitSquadStructure CreateHitSquad()
        {
            return new HitSquadStructure(new int[]
            {
                ModContent.NPCType<FleshkinAcolyte_Assassin>()
            });
        }
    }
}
