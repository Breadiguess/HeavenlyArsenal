using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

public class BloodwhipBuff : ModBuff
{
    public static readonly int TagDamage = 15;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}