using System.Collections.Generic;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

internal class ConeVomit : ModProjectile
{
    public NPC Owner;

    public List<VertexPositionColorTexture> ConeVerts;

    private BasicEffect Cone;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.timeLeft = 12800;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.Size = new Vector2(30, 30);
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0.0f;
    }

    public override void AI()
    {
        var ow = Owner.ModNPC as voidVulture;

        if (ow.currentState != voidVulture.Behavior.VomitCone || !Owner.active)
        {
            Projectile.active = false;
        }

        Projectile.rotation = Owner.Center.AngleTo(ow.HeadPos);
        Projectile.Center = ow.HeadPos + Projectile.rotation.ToRotationVector2();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (voidVulture.Myself.As<voidVulture>().HasSecondPhaseTriggered)
        {
            if (DirectionalSolynForcefield3.Myself != null)
            {
                //todo: allow the shield to prevent hits from the cone
            }
        }
        else if (targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, 400 * Projectile.scale, Projectile.rotation, MathHelper.ToRadians(10)))
        {
            return true;
        }

        return false;
    }

    private void DrawCone()
    {
        if (ConeVerts.Count < 3)
        {
            return;
        }

        var gd = Main.graphics.GraphicsDevice;

        if (Cone == null)
        {
            Cone = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        Cone.World = Matrix.Identity;
        Cone.View = Main.GameViewMatrix.ZoomMatrix;

        Cone.Projection = Matrix.CreateOrthographicOffCenter
        (
            0,
            Main.screenWidth,
            Main.screenHeight,
            0,
            -1000f,
            1000f
        );

        foreach (var pass in Cone.CurrentTechnique.Passes)
        {
            pass.Apply();

            gd.DrawUserPrimitives
            (
                PrimitiveType.TriangleStrip,
                ConeVerts.ToArray(),
                0,
                ConeVerts.Count - 2
            );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="Center">
    ///     converts to world position, so all you need to do is place this in the spot
    ///     you want your cone to originate from
    /// </param>
    /// <param name="rotation"></param>
    /// <param name="halfAngle"></param>
    /// <param name="length"></param>
    /// <param name="resolution"></param>
    /// <param name="color"></param>
    public static void BuildCone(List<VertexPositionColorTexture> verts, Vector2 Center, float rotation, float halfAngle, float length, int resolution, Color color)
    {
        verts.Clear();
        //offset the coordinates so they're in screen coords
        Center -= Main.screenPosition;
        // Direction the cone points in
        var dir = rotation.ToRotationVector2();

        // Create arc segment points
        for (var i = 0; i <= resolution; i++)
        {
            var t = i / (float)resolution;
            var ang = MathHelper.Lerp(-halfAngle, halfAngle, t);
            var edgeDir = dir.RotatedBy(ang);

            var p = Center + edgeDir * length;

            var radiusFade = 1f;
            var edgeFade = 0f;
            var sideFade = MathF.Cos(Math.Abs(ang) / halfAngle * MathHelper.PiOver2);

            // combine fades:
            var apexAlpha = radiusFade * sideFade;
            var edgeAlpha = edgeFade * sideFade;

            var apexColor = color * apexAlpha;
            var edgeColor = color * edgeAlpha;

            verts.Add
            (
                new VertexPositionColorTexture
                (
                    new Vector3(Center, 0f),
                    apexColor,
                    new Vector2(0f, 0f)
                )
            );

            verts.Add
            (
                new VertexPositionColorTexture
                (
                    new Vector3(p, 0f),
                    edgeColor,
                    new Vector2(t, 1f)
                )
            );
        }
    }

    private void prepCone()
    {
        if (ConeVerts == null)
        {
            ConeVerts = new List<VertexPositionColorTexture>();
        }

        BuildCone(ConeVerts, Projectile.Center, Projectile.rotation, MathHelper.ToRadians(30) * Projectile.scale, 1000, 300, Color.White * Projectile.scale);
        DrawCone();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        prepCone();

        return false; // base.PreDraw(ref lightColor);
    }
}