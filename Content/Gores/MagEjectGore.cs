namespace HeavenlyArsenal.Content.Gores;

internal class MagEjectGore : ModGore
{
    public override string Texture => base.Texture;

    public override void SetStaticDefaults()
    {
        Gore.goreTime = 40;
    }
}