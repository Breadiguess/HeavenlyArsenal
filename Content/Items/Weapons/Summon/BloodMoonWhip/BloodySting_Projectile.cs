using HeavenlyArsenal.Common;
using static HeavenlyArsenal.Common.WhipMotions;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

public class BloodySting_Projectile : BaseWhipProjectile
{
    protected override Texture2D PrimitiveTex => ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BloodMoonWhip/BloodySting_Primitive").Value;

    protected override Texture2D WhipHandle => ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BloodMoonWhip/BloodySting_Handle").Value;

    protected override Texture2D WhipHead => ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BloodMoonWhip/BloodySting_Head").Value;
    protected override IWhipMotion CreateMotion()
    {
        var item = Owner.HeldItem.ModItem as BloodySting_Item;

        if (item.SwingStage == 1)
        {
            return new FancyWhipMotion();
            
        }
        else
            return new VanillaWhipMotion();

    }
    protected override void SetupModifiers(ModularWhipController controller)
    {

       
    }
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

    }
    public override void Prepare()
    {
        Projectile.alpha = 0;
        this._ShouldDrawNormal = false;
        AddHitEffects(BuffID.Bleeding, 60 * 10);
        AddHitEffects(ModContent.BuffType<BloodwhipBuff>(), 60 * 4);
    }
    public override Color GetWhipColor(float t)
    {
        return Color.Lerp(Color.Crimson, Color.Red, t);
    }
    public override float _PixelsPerRepeat => 64;
    protected override void drawHead(Vector2 HeadPos, float BaseRotation, List<Vector2> list, SpriteEffects flip, Color LightColor)
    {
        base.drawHead(HeadPos, BaseRotation, list, flip, LightColor);
    }
    public override float GetWhipWidth(float baseWidth, float t)
    {
        float thing = SecondsToFrames((40 + 60f) /2f);
        float func = baseWidth+MathF.Sin(Main.GlobalTimeWrappedHourly*thing+t*20)*2*t;

        if (t > 0.8f)
            return float.Lerp(baseWidth, func, 0.3f)*4;


        return func*4;
    }
}