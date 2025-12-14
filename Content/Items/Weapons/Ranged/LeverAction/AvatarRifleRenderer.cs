using CalamityMod;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction;

internal partial class AvatarRifle_Held
{
    private void RenderAmmunition()
    {
        if (CurrentState != State.Reload)
        {
            return;
        }

        var bulletAMMO = AmmoID.Bullet;

        var BulletValue = TextureAssets.Item[bulletAMMO].Value;
        var DrawPos = Owner.GetBackHandPositionImproved(Owner.compositeBackArm) - Main.screenPosition;
        Main.EntitySpriteDraw(BulletValue, DrawPos, null, Color.AntiqueWhite, 0, BulletValue.Size() * 0.5f, 0.5f, 0);
    }

    private void RenderLever(float Rot)
    {
        var lever = ModContent.Request<Texture2D>(Texture + "_Held_Lever").Value;
        var DrawPos = Projectile.Center - Main.screenPosition + new Vector2(20, 0).RotatedBy(Rot);
        var Origin = new Vector2(lever.Width, 0);

        var AdjustedRot = Rot + MathHelper.ToRadians(-30 * LeverCurveOutput);
        Main.EntitySpriteDraw(lever, DrawPos, null, Color.Purple, AdjustedRot, Origin, 1, 0);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        var DrawPos = Projectile.Center - Main.screenPosition;
        var origin = new Vector2(texture.Width / 4, texture.Height / 2);
        SpriteEffects flip = 0;

        Main.EntitySpriteDraw(texture, DrawPos, null, Color.AntiqueWhite, Projectile.rotation, origin, 1, flip);
        RenderLever(Projectile.rotation);
        RenderAmmunition();

        return false;
    }
}