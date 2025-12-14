using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class GlassPlayer : ModPlayer
{
    public float Offset;

    public int animationTime;

    public float SheathingInterpolant;

    public Projectile GlassSun { get; set; }

    public bool SheathingSword { get; set; }

    public bool Empowered { get; set; }

    public int EmpoweredAttackCount { get; set; }

    public override void PostUpdateMiscEffects()
    {
        ManageSheathing();
        ManageSword();
    }

    public override void ArmorSetBonusActivated()
    {
        Empowered = false;
        EmpoweredAttackCount = 0;
    }

    public override void PreUpdateMovement()
    {
        if (SheathingSword)
        {
            Player.velocity = Vector2.Zero;

            if (GlassSun != null)
            {
                Player.Center = Vector2.Lerp(Player.Center, GlassSun.Center + new Vector2(0, -Offset * SheathingInterpolant), 0.2f);
            }
        }
    }

    #region Helper

    public void ManageSword()
    {
        if (EmpoweredAttackCount < 0)
        {
            Empowered = false;
        }
    }

    public void ManageSheathing()
    {
        if (SheathingInterpolant > 0)
        {
            SheathingSword = true;
            Player.SetDummyItemTime(2);
            //Main.NewText($"{SheathingInterpolant}");
            SheathingInterpolant = float.Lerp(SheathingInterpolant, 1, 0.05f);

            if (SheathingInterpolant >= 0.99f)
            {
                animationTime++;
            }
        }

        if (animationTime > 0)
        {
            animationTime++;

            if (animationTime >= 40)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.Chuckle);
                Unsheath();
            }
        }
    }

    public void SinkSwordIntoGlassMass(Projectile target)
    {
        GlassSun = target;
        SheathingInterpolant = 0.01f;
        Offset = GlassSun.scale * 50;
    }

    public void Unsheath()
    {
        Empowered = true;
        EmpoweredAttackCount = 12;
        SheathingInterpolant = 0;
        SheathingSword = false;
        animationTime = 0;

        if (GlassSun != null) { }
    }

    #endregion
}