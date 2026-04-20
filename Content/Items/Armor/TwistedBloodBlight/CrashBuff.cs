namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight;

public class CrashBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.debuff[Type] = true;
        
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}