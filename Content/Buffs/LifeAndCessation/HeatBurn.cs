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
    class HeatBurn : ModBuff
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
            player.AddBuff(ModContent.BuffType<HeatBurn>(), 2);
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            
        }
    }
}
