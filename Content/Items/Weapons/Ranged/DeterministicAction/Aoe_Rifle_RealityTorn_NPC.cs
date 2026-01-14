using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_RealityTorn_NPC : GlobalNPC
    {
        
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.HasBuff(ModContent.BuffType<Aoe_Rifle_RealityTorn_Buff>()) && lateInstantiation;
        }

        public override void PostAI(NPC npc)
        {
            
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            return false;
        }
    }
}
