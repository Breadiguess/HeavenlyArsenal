using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_RealityTear : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        struct TearSegment
        {
            public Vector2 Start;
            public Vector2 End;
            public float Thickness;
        }
        List<TearSegment> segments = new();

        void GenerateSpine(Vector2 origin, Vector2 direction)
        {
            Vector2 dir = direction.SafeNormalize(Vector2.Zero);
            Vector2 pos = origin;

            float thickness = 6f;

            for (int i = 0; i < 10; i++)
            {
                float length = Main.rand.NextFloat(40f, 80f);

                Vector2 next =
                    pos +
                    dir * length +
                    Main.rand.NextVector2Circular(12f, 12f); // slight bend

                segments.Add(new TearSegment
                {
                    Start = pos,
                    End = next,
                    Thickness = thickness
                });

                // Chance to branch
                if (Main.rand.NextBool(2))
                    GenerateBranch(pos, dir, thickness * 0.7f, 0);

                pos = next;
                thickness *= 0.85f; 
                dir = dir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            }
        }
        void GenerateBranch(Vector2 origin, Vector2 parentDir, float thickness, int depth)
        {
            if (depth > 2 || thickness < 1.2f)
                return;

            Vector2 dir =
                parentDir.RotatedBy(Main.rand.NextFloat(0, MathHelper.PiOver2));

            Vector2 pos = origin;

            int segmentsInBranch = Main.rand.Next(2, 5);

            for (int i = 0; i < segmentsInBranch; i++)
            {
                float length = Main.rand.NextFloat(25f, 50f);

                Vector2 next =
                    pos +
                    dir * length +
                    Main.rand.NextVector2Circular(8f, 8f);

                segments.Add(new TearSegment
                {
                    Start = pos,
                    End = next,
                    Thickness = thickness
                });

                pos = next;
                thickness *= 0.75f;
                dir = dir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
            }

            // Optional sub-branch
            if (Main.rand.NextBool(3))
                GenerateBranch(pos, dir, thickness * 0.7f, depth + 1);
        }

        public int TearPositionCounts;
        public Vector2[] TearPositions;
        public override void OnSpawn(IEntitySource source)
        {

            segments.Clear();
            GenerateSpine(Projectile.Center, Projectile.velocity);
            Projectile.velocity *= 0;
        }
        public override void SetDefaults()
        {
            TearPositionCounts = Main.rand.Next(20, 40);
            TearPositions = new Vector2[TearPositionCounts];
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(14, 14);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.damage = 40000;
            Projectile.timeLeft = 180;
        }


        public override void AI()
        {
            Projectile.Opacity = LumUtils.InverseLerp(0, 180, Projectile.timeLeft);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Aoe_Rifle_RealityTorn_Buff>(), 60*8);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 aabbPos = targetHitbox.TopLeft();
            Vector2 aabbSize = targetHitbox.Size();
            
            const float lineThickness = 6f;

            for (int i = 0; i < TearPositions.Length - 1; i++)
            {
                Vector2 start = TearPositions[i];
                Vector2 end = TearPositions[i + 1];
                float collisionPoint = 0;
                if (Collision.CheckAABBvLineCollision(
                    aabbPos,
                    aabbSize,
                    start,
                    end,
                    lineThickness, ref collisionPoint))
                {
                    return true;
                }
            }

            return base.Colliding(projHitbox, targetHitbox);
        }


        public override bool PreDraw(ref Color lightColor)
        {

            Color color = Color.White * Projectile.Opacity;
            foreach (var s in segments)
            {
                Utils.DrawLine(
                    Main.spriteBatch,
                    s.Start,
                    s.End,
                   color,
                    color,
                    s.Thickness*3
                );
            }
            return false;
        }

    }
}
