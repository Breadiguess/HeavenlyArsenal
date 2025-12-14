namespace HeavenlyArsenal.Content.Buffs.Stims;

internal class StimWithdrawl_Debuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense -= 5;
        player.GetAttackSpeed<MeleeDamageClass>() *= 0.425f;
        player.GetDamage<GenericDamageClass>() *= 0.425f;
        player.GetCritChance<GenericDamageClass>() *= 0.8f;
        player.GetKnockback<SummonDamageClass>() *= 0.1f;
        player.moveSpeed *= 0.45f;
        player.lifeRegen -= 4;
        player.pickSpeed *= 0.2f;

        if (player.HasBuff(ModContent.BuffType<CombatStimBuff>()))
        {
            player.ClearBuff(ModContent.BuffType<CombatStimBuff>());
        }
        //player.ClearBuff(ModContent.BuffType<CombatStimBuff>());
        //player.GetModPlayer<StimPlayer>().Withdrawl = true;
    }
}