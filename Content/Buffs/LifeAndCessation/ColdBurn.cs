using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Buffs.LifeAndCessation
{
    class ColdBurn : ModBuff
    {
        public override string Texture => "HeavenlyArsenal/Content/Buffs/AntishadowAssassinBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
           // npc.coldDamage = true;
            if(npc.HasBuff(ModContent.BuffType<HeatBurn>()))
            {
                             npc.AddBuff(ModContent.BuffType<ThermalShock>(), 20);
            }
           
        }
    }
}
