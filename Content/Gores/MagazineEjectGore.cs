namespace HeavenlyArsenal.Content.Gores;

public class MagazineEjectGore : ModGore
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Gore.goreTime = 40;
    }
}