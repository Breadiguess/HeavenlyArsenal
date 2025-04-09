using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using HeavenlyArsenal.ArsenalPlayer;

namespace HeavenlyArsenal.Content.Buffs
{
    class CombatStimBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }


        
        public override void Update(Player player, ref int buffIndex)
        {
            /* Exquisitely stuffed stats, for reference.
            player.wellFed = true;
            player.statDefense += 4;
            player.meleeCrit += 4;
            player.meleeDamage += 0.1f;
            player.meleeSpeed += 0.1f;
            player.magicCrit += 4;
            player.magicDamage += 0.1f;
            player.rangedCrit += 4;
            player.rangedDamage += 0.1f;
            player.moveSpeed += 0.4f;
            player.pickSpeed -= 0.15f;
            */
            //
            


            if (!GeneralScreenEffectSystem.ChromaticAberration.Active)
                GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 0.75f, 0);
            player.wellFed = true;
            player.statDefense += 5;
            player.GetAttackSpeed<MeleeDamageClass>() += 1f;
            player.GetDamage<GenericDamageClass>() += 0.425f;
            player.GetCritChance<GenericDamageClass>() += 5f;
            player.GetKnockback<SummonDamageClass>() += 1f;
            
            
            player.moveSpeed += 1f;
            player.pickSpeed -= 0.2f;
            player.jumpSpeedBoost += 1f;

            player.ClearBuff(BuffID.WellFed);
            player.ClearBuff(BuffID.WellFed2);
            player.ClearBuff(BuffID.WellFed3);
            player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());

        }




    }
}
