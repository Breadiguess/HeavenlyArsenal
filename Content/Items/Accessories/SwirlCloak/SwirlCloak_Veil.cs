using System.Collections.Generic;
using Luminance.Common.Easings;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;

internal class SwirlCloak_Veil : ModProjectile
{
    public PiecewiseCurve SwirlCurve;

    public int StarDamage = 600;

    public HashSet<int> TrappedProjectiles = new();

    private readonly float CaptureRadius = 300f;

    private Vector2 orbitRadius = new(10, 0);

    public ref float SwirlCloakInterp => ref Projectile.ai[0];

    public ref float t => ref Projectile.ai[1];

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override void SetDefaults()
    {
        Projectile.width = 60;
        Projectile.height = 60;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.extraUpdates = 3;
        Projectile.scale = 1f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (SwirlCurve == null)
        {
            SwirlCurve = new PiecewiseCurve()
                .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);
        }
    }

    public override void AI()
    {
        if (!Owner.active || Owner.dead)
        {
            Projectile.Kill();

            return;
        }

        Projectile.Center = Owner.Center;

        if (SwirlCloakInterp >= 0.999f)
        {
            Projectile.Kill();

            return;
        }

        Projectile.timeLeft++;

        SwirlCloakInterp = SwirlCurve.Evaluate(t);

        if (t < 1)
        {
            t = Math.Clamp(t + 0.0001f, 0, 1);
        }

        TrapProjectiles();
        doCaptureLogic();
    }

    private void TrapProjectiles()
    {
        foreach (var proj in Main.ActiveProjectiles)
        {
            if (!proj.active || (proj.type == ModContent.ProjectileType<SwirlCloak_Veil>() && !proj.friendly))
            {
                continue;
            }

            var distance = Vector2.Distance(proj.Center, Projectile.Center);

            if (distance < CaptureRadius)
            {
                // Store trapped state
                if (!TrappedProjectiles.Contains(proj.whoAmI))
                {
                    TrappedProjectiles.Add(proj.whoAmI);
                }
            }
        }
    }

    private void doCaptureLogic()
    {
        var trappedList = new List<int>(TrappedProjectiles);

        for (var i = trappedList.Count - 1; i >= 0; i--)
        {
            var trapped = Main.projectile[trappedList[i]];
            trapped.GetGlobalProjectile<VortexCaptureGlobal>().BeginCapture(trapped, Projectile, 0, 300);
        }
    }

    private void ConvertProjectiles()
    {
        // Convert HashSet<int> to a List<int> for indexed access
        var trappedList = new List<int>(TrappedProjectiles);

        for (var i = trappedList.Count - 1; i >= 0; i--)
        {
            var trapped = Main.projectile[trappedList[i]];

            if (!trapped.active)
            {
                TrappedProjectiles.Remove(trappedList[i]);
            }

            // float distance = Vector2.Distance(trapped.Center, Projectile.Center);
            // if (distance < 16f)
            /*
             {
                 // Convert
                 trapped.Kill();
                 Projectile.NewProjectile(
                     Projectile.GetSource_FromThis(),
                     Projectile.Center,
                     Vector2.Zero,
                     ModContent.ProjectileType<SwirlCloak_Star>(),
                     StarDamage,
                     0f,
                     Projectile.owner
                 );

                 TrappedProjectiles.Remove(trappedList[i]);
             }
            */
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var veilTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/SwirlCloak/SwirlCloak_Veil").Value;

        var DrawPos = Projectile.Center - Main.screenPosition;

        var frame = veilTexture.Frame();

        var Rot = MathHelper.ToRadians(360) * SwirlCloakInterp;
        var Scale = new Vector2(1) * SwirlCloakInterp;
        Main.EntitySpriteDraw(veilTexture, DrawPos, frame, Color.White * (1 - SwirlCloakInterp), Rot, frame.Size() * 0.5f, Scale, SpriteEffects.None);

        return false;
    }
}