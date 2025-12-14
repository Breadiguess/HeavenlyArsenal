using NoxusBoss.Core.AdvancedProjectileOwnership;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;

public class VortexCaptureGlobal : GlobalProjectile
{
    public bool captured;

    public int captorWhoAmI = -1; // vortex projectile whoAmI

    public int captureTimer; // ticks since capture

    public float orbitAngle; // per-proj phase

    public float orbitRadius = 48f; // can be adjusted per projectile

    public bool savedFriendly;

    public bool savedHostile;

    public bool savedTileCollide;

    public int savedPenetrate;

    public int savedTimeLeftSnapshot;

    public override bool InstancePerEntity => true;

    // --- Call this when you capture a projectile ---
    public void BeginCapture(Projectile proj, Projectile captor, float startAngle, float radius)
    {
        if (captured || !proj.active || captor == null || !captor.active)
        {
            return;
        }

        captured = true;
        captorWhoAmI = captor.whoAmI;
        captureTimer = 0;
        orbitAngle = startAngle;
        orbitRadius = radius;

        // Save state we’re going to override
        savedFriendly = proj.friendly;
        savedHostile = proj.hostile;
        savedTileCollide = proj.tileCollide;
        savedPenetrate = proj.penetrate;

        savedTimeLeftSnapshot = proj.timeLeft;

        proj.friendly = false;
        proj.hostile = false;
        proj.tileCollide = false;
        proj.penetrate = -1; // don’t auto-die on hits
        proj.aiStyle = -1; // don’t run vanilla ai style fallbacks
        proj.velocity = Vector2.Zero;
        proj.netUpdate = true;
    }

    // --- Call this to free (or just before converting/kill+spawn) ---
    public void EndCapture(Projectile proj, bool restoreState)
    {
        if (!captured)
        {
            return;
        }

        if (restoreState && proj.active)
        {
            proj.friendly = savedFriendly;
            proj.hostile = savedHostile;
            proj.tileCollide = savedTileCollide;
            proj.penetrate = savedPenetrate;
        }

        captured = false;
        captorWhoAmI = -1;
        proj.netUpdate = true;
    }

    public override bool PreAI(Projectile proj)
    {
        if (!captured)
        {
            return true; // run normal AI
        }

        // If captor died, release gracefully
        if (captorWhoAmI < 0 || !Main.projectile[captorWhoAmI].active)
        {
            EndCapture(proj, true);

            return true; // resume normal AI next tick
        }

        // --- Suppress original AI completely while captured ---
        // (Returning false prevents vanilla/mod AI from running.)
        // We’ll take full control of movement/rotation below in PostAI.
        return false;
    }

    public override void PostAI(Projectile proj)
    {
        if (!captured)
        {
            return;
        }

        var captor = Main.projectile[captorWhoAmI];

        if (!captor.active)
        {
            EndCapture(proj, true);

            return;
        }

        captureTimer++;

        // Rotation speed of orbit (affects spiral tightness)
        var angularSpeed = 0.15f;
        orbitAngle += angularSpeed;

        // Smoothly reduce orbit radius over time
        var shrinkSpeed = 0.5f; // pixels per frame shrink
        orbitRadius = Math.Max(orbitRadius - shrinkSpeed, 0f);

        // Compute target orbit position
        var desiredPos = captor.Center + orbitAngle.ToRotationVector2() * orbitRadius;

        proj.scale = float.Lerp(proj.scale, 0f, 0.005f);
        // Lerp the projectile toward that position (smooth spiral)
        var lerpFactor = 0.15f; // smaller = smoother, slower
        proj.Center = Vector2.Lerp(proj.Center, desiredPos, lerpFactor);

        // Keep it visually consistent
        proj.velocity = Vector2.Zero;
        proj.rotation = proj.oldPosition.AngleTo(proj.position) + MathHelper.PiOver2;
        proj.timeLeft = 2;

        if (captureTimer > 120)
        {
            proj.Kill();
            proj.NewProjectileBetter(proj.GetSource_FromThis(), proj.Center, Vector2.Zero, ModContent.ProjectileType<SwirlCloak_Star>(), 600, 0);
        }
        // Add light or particles if you like
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        return captured ? false : base.CanHitPlayer(projectile, target);
    }

    public override bool? CanDamage(Projectile proj)
    {
        return captured ? false : null;
    }

    public override void Kill(Projectile proj, int timeLeft)
    {
        // If something external kills it while captured, release state
        if (captured)
        {
            EndCapture(proj, false);
        }
    }
}