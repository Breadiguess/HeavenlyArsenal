namespace HeavenlyArsenal.Content.Buffs;

internal class BloodArmorFrenzy : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        base.Update(player, ref buffIndex);
    }

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = false;
        BuffID.Sets.LongerExpertDebuff[Type] = false;
    }
}