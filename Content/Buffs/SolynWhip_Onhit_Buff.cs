using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Buffs
{
    class SolynWhip_Onhit_Buff : ModBuff
    {
        public static readonly int TagDamage = 500;
        public override void SetStaticDefaults() 
        {
          
        }
        //public override void Update(Player player, ref int buffIndex)
        //{
        //    BuffID.Sets.IsATagBuff[Type] = true;
        //}

    }
    public class Solynel : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff<SolynWhip_Onhit_Buff>())
            {
                modifiers.FlatBonusDamage += SolynWhip_Onhit_Buff.TagDamage * ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
            }
        }
    }
}
