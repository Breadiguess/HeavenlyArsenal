using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

internal class LightSpear : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(18, 18);
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.extraUpdates = 1;
        Projectile.scale = 1.2f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
        Projectile.rotation = Projectile.velocity.ToRotation();
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Time > 60)
        {
            var a = Projectile.FindTargetWithinRange(2000, true);

            if (a != null)
            {
                var Tracking = Utils.Remap(Time, 0, Projectile.timeLeft, 0, 1);
                Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.Center.AngleTo(a.Center), Tracking);
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * 10;
            }
        }

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        base.ModifyHitNPC(target, ref modifiers);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var a = ModContent.Request<Texture2D>(Texture).Value;

        var DrawPos = Projectile.Center - Main.screenPosition;
        var Origin = new Vector2(a.Width / 2, a.Height / 2);
        var Scale = new Vector2(1.5f, 1f);
        var AAAAAA = Color.Lerp(Color.White, Color.AntiqueWhite, 0.5f);
        var Rot = Projectile.rotation;

        Main.EntitySpriteDraw(a, DrawPos, null, AAAAAA, Rot, Origin, Scale, SpriteEffects.None);

        return false;
    }
}