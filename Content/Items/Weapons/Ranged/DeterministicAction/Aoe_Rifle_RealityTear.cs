using Luminance.Assets;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_RealityTear : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        private struct TearSegment
        {
            public Vector2 Start;
            public Vector2 End;
            public float Thickness;

            // When this segment begins growing.
            public int StartTime;

            // How long this specific segment takes to fully extend.
            public int GrowTime;
        }

        private readonly List<TearSegment> segments = [];

        // AI timer.
        ref float Timer => ref Projectile.ai[0];

        private const int MaxSpineSegments = 10;
        private const int BaseSegmentGrowTime = 5;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(14, 14);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.damage = 40000;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            segments.Clear();

            Vector2 spawnDirection = Projectile.velocity;
            if (spawnDirection == Vector2.Zero)
                spawnDirection = -Vector2.UnitY;

            GenerateSpine(Projectile.Center, spawnDirection, 0);
            Projectile.velocity = Vector2.Zero;
        }

        public override void AI()
        {
            Timer++;

            // Fade out only near the end instead of across the entire lifetime.
            Projectile.Opacity = Utils.GetLerpValue(0f, 20f, Projectile.timeLeft, true);
        }

        private void GenerateSpine(Vector2 origin, Vector2 direction, int startTime)
        {
            Vector2 dir = direction.SafeNormalize(Vector2.UnitY);
            Vector2 pos = origin;
            float thickness = 6f;

            int segmentStartTime = startTime;

            for (int i = 0; i < MaxSpineSegments; i++)
            {
                float length = Main.rand.NextFloat(40f, 80f);
                Vector2 next = pos + dir * length + Main.rand.NextVector2Circular(12f, 12f);

                segments.Add(new TearSegment
                {
                    Start = pos,
                    End = next,
                    Thickness = thickness,
                    StartTime = segmentStartTime,
                    GrowTime = BaseSegmentGrowTime
                });

                // Make branches appear after this section has started growing.
                if (Main.rand.NextBool(2))
                    GenerateBranch(pos, dir, thickness * 0.7f, 0, segmentStartTime + 2);

                pos = next;
                thickness *= 0.85f;
                dir = dir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));

                // Next spine piece starts a bit after the previous one.
                segmentStartTime += 3;
            }
        }

        private void GenerateBranch(Vector2 origin, Vector2 parentDir, float thickness, int depth, int startTime)
        {
            if (depth > 2 || thickness < 1.2f)
                return;

            Vector2 dir = parentDir.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2));
            Vector2 pos = origin;

            int segmentsInBranch = Main.rand.Next(2, 5);
            int segmentStartTime = startTime;

            for (int i = 0; i < segmentsInBranch; i++)
            {
                float length = Main.rand.NextFloat(25f, 50f);
                Vector2 next = pos + dir * length + Main.rand.NextVector2Circular(8f, 8f);

                segments.Add(new TearSegment
                {
                    Start = pos,
                    End = next,
                    Thickness = thickness,
                    StartTime = segmentStartTime,
                    GrowTime = BaseSegmentGrowTime
                });

                pos = next;
                thickness *= 0.75f;
                dir = dir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                segmentStartTime += 2;
            }

            if (Main.rand.NextBool(3))
                GenerateBranch(pos, dir, thickness * 0.7f, depth + 1, segmentStartTime);
        }

        private float GetSegmentCompletion(TearSegment s)
        {
            float growProgress = Utils.GetLerpValue(s.StartTime, s.StartTime + s.GrowTime, Timer, true);

            // Smooth it a little so it feels organic instead of linear.
            growProgress = MathF.Pow(growProgress, 2f);
            return growProgress;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 aabbPos = targetHitbox.TopLeft();
            Vector2 aabbSize = targetHitbox.Size();

            foreach (var s in segments)
            {
                float completion = GetSegmentCompletion(s);
                if (completion <= 0f)
                    continue;

                Vector2 currentEnd = Vector2.Lerp(s.Start, s.End, completion);
                float thickness = s.Thickness * 3f * completion;

                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(
                    aabbPos,
                    aabbSize,
                    s.Start,
                    currentEnd,
                    Math.Max(2f, thickness),
                    ref collisionPoint))
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Aoe_Rifle_RealityTorn_Buff>(), 60 * 8);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color color = Color.Crimson * Projectile.Opacity;

            foreach (var s in segments)
            {
                float completion = GetSegmentCompletion(s);
                if (completion <= 0f)
                    continue;

                Vector2 currentEnd = Vector2.Lerp(s.Start, s.End, completion);

                // Slight thickness ramp so the line "flares" into existence.
                float currentThickness = s.Thickness * 3f * MathHelper.Lerp(0.35f, 1f, completion);

                NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(
                    Main.spriteBatch,
                    s.Start,
                    currentEnd,
                    color,
                    currentThickness
                );
            }

            return false;
        }
    }
}