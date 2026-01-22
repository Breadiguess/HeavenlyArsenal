namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles
{
    public class SmallBladeSlash : BaseSwordProjectile
    {
        protected override void HandleMovement()
        {
            float progress = Projectile.timeLeft / (float)Attack.ActiveFrames;

            // Arc swing
            float angle = MathHelper.Lerp(
                -1.2f,
                1.2f,
                progress
            );

            Projectile.Center = Owner.Center +
                angle.ToRotationVector2() * 60f;

            Projectile.rotation = angle;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }
    }

}