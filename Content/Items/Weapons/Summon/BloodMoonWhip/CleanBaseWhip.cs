using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    /// <summary>
    /// A cleaned-up, easier-to-extend base class for whip projectiles.
    /// Subclasses override virtual properties/methods to change visuals/behavior.
    /// </summary>
    /// <remarks>
    /// Key improvements:
    /// - owner item type is captured at spawn time (safer than string compares)
    /// - control points are calculated only when needed
    /// - clearer FlyProgress and HitboxActive semantics
    /// - fewer allocations, clearer override points
    /// </remarks>
    public abstract class CleanBaseWhip : ModProjectile
    {
        #region Values
        public ref float Time => ref Projectile.ai[0];
        protected static int DefaultSegments => 12;
        protected static float DefaultRangeMult => 1.0f;
        protected virtual int DefaultHandleHeight => 20;
        protected virtual int DefaultSegHeight => 16;
        protected virtual int DefaultEndHeight => 20;
        protected virtual int DefaultSegTypes => 2;
        public virtual SoundStyle? WhipSound => SoundID.Item153;
        public virtual Color StringColor => Color.White;

        private bool _initialized;
        private int _ownerItemType = ItemID.None; // saved on spawn
        private float _flyTime; // in ticks (accounting for MaxUpdates below)
        private readonly List<Vector2> _controlPoints = new();

        public virtual int Segments
        {
            get;
            set;
        }
        public virtual float RangeMult
        {
            get;
            set;
        }
        protected virtual int handleHeight => DefaultHandleHeight;
        protected virtual int segHeight => DefaultSegHeight;
        protected virtual int endHeight => DefaultEndHeight;
        protected virtual int segTypes => DefaultSegTypes;

        #endregion
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Segments = DefaultSegments;
            RangeMult = DefaultRangeMult;
            Projectile.penetrate = -1;
            Projectile.DefaultToWhip();
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            if (Main.player[Projectile.owner].HeldItem != null)
                _ownerItemType = Main.player[Projectile.owner].HeldItem.type;

        }

        #region Helpers
        /// <summary>
        /// compute fly time once per relevant change
        /// </summary>
        /// <returns></returns>
        protected virtual float ComputeFlyTime()
        {
            Player p = Main.player[Projectile.owner];

            int baseAnim = p.itemAnimationMax > 0 ? p.itemAnimationMax : 20;
            return baseAnim * Projectile.MaxUpdates;
        }

        public static void FillWhipControlPointsBetter(Projectile proj, List<Vector2> controlPoints, int segments, float rangeMultiplier, float flyTime)
        {
            controlPoints.Clear();
            Player player = Main.player[proj.owner];

            Vector2 start = Main.GetPlayerArmPosition(proj);
            Vector2 dir = proj.velocity.SafeNormalize(Vector2.UnitX);

            float progress = Math.Clamp(proj.ai[0] / flyTime, 0f, 1f);
            float whipLength = proj.velocity.Length() * rangeMultiplier * progress;

            Vector2 tip = start + dir * whipLength;

            // Always add the base (arm)
            controlPoints.Add(start);

            // Add intermediate segments, including the tip
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments; // goes 0..1
                Vector2 point = Vector2.Lerp(start, tip, t);

                // Example sag/curve (optional)
                float sag = (float)Math.Sin(t * MathHelper.Pi) * 20f * (1f - progress);
                point += dir.RotatedBy(MathHelper.PiOver2) * sag;

                controlPoints.Add(point);
            }
        }

        public float FlyProgress => (_flyTime <= 0) ? 0f : (Time / _flyTime);
        protected virtual bool HitboxActive(float progress) => progress >= 0.1f && progress <= 0.7f;


        public static void GetWhipSettingsBetter(Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier)
        {

            timeToFlyOut = Main.player[proj.owner].itemAnimationMax * proj.MaxUpdates;
            segments = DefaultSegments;
            rangeMultiplier = DefaultRangeMult;

            if (proj.ModProjectile is CleanBaseWhip cbw)
            {
                segments = cbw.Segments;
                rangeMultiplier = cbw.RangeMult;
            }
        }

        /// <summary>
        /// Call me in PreAI to let subclasses alter control points before drawing/collision
        /// </summary>
        /// <param name="points"></param>
        public virtual void ModifyControlPoints(List<Vector2> points) { }

        /// <summary>
        /// kind of important???
        /// </summary>
        /// <param name="outFlyTime"></param>
        /// <param name="outSegments"></param>
        /// <param name="outRangeMult"></param>
        protected virtual void ModifyWhipSettings(ref float outFlyTime, ref int outSegments, ref float outRangeMult)
        {
            outSegments = Segments;
            outRangeMult = RangeMult;
            outFlyTime = ComputeFlyTime();
            GetWhipSettingsBetter(Projectile, out float fly, out int segs, out float range);
            Projectile.GetWhipSettings(Projectile, out outFlyTime, out outSegments, out outRangeMult);
            // Replace with values from our system


        }
        #endregion

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (!_initialized)
            {
                _initialized = true;



                float ft = 0; int segs = 0; float range = 0;
                ModifyWhipSettings(ref ft, ref segs, ref range);

                _flyTime = ft > 0 ? ft : ComputeFlyTime();


                Segments = segs > 0 ? segs : DefaultSegments;
                //Main.NewText(Segments);
                RangeMult = range > 0 ? range : DefaultRangeMult;
                //Main.NewText(RangeMult);
            }
            float toProjectile = player.MountedCenter.AngleTo(Projectile.Center) - MathHelper.PiOver2;
            float flyProgress = FlyProgress; // normalized 0→1
            var stretch = GetAnimatedArmStretch(flyProgress);

            player.SetCompositeArmFront(true, stretch, toProjectile);
            //Main.NewText(player.compositeFrontArm.stretch.ToString());


            Time++;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
            Projectile.spriteDirection = player.direction;

            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * FlyProgress;

            player.heldProj = Projectile.whoAmI;
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Time >= _flyTime)
            {
                Projectile.Kill();
                return false;
            }
            // Play sound at the tip of the whip
            //todo: make this a virtual thing so that the time at which the whipcrack sounds can be modified
            if (Math.Abs(Time - _flyTime / 2f) < 1f)
            {
                if (WhipSound != null && _controlPoints.Count > 0)
                    SoundEngine.PlaySound(WhipSound.Value, _controlPoints[^1]);
            }


            if (Projectile.ai[0] == (int)(_flyTime / 2f))
            {
                Projectile.WhipPointsForCollision.Clear();
                Projectile.FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
                Vector2 position = Projectile.WhipPointsForCollision[^1];
                if (WhipSound != null)
                {
                    SoundEngine.PlaySound(WhipSound.Value, position);
                }
            }


            if (HitboxActive(FlyProgress))
            {
                var half = new Vector2(Projectile.width * Projectile.scale * 0.5f, 0f);
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
                    Utils.PlotTileLine(_controlPoints[i] - half, _controlPoints[i] + half, Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
                }
            }

            WhipAI();

            return false;
        }
        private Player.CompositeArmStretchAmount GetAnimatedArmStretch(float t)
        {
            // t goes from 0 → 1 over the whip's flytime
            if (t < 0.2f)
                return Player.CompositeArmStretchAmount.None;               // 0%–20%

            if (t < 0.35f)
                return Player.CompositeArmStretchAmount.ThreeQuarters;            // 20%–35%

            if (t < 0.55f)
                return Player.CompositeArmStretchAmount.Full;      // 35%–55%

            if (t < 0.75f)
                return Player.CompositeArmStretchAmount.Full;               // 55%–75%

            if (t < 0.9f)
                return Player.CompositeArmStretchAmount.Full;      // 75%–90%

            return Player.CompositeArmStretchAmount.None;                   // 90%–100%
        }

        protected virtual void WhipAI() { }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // set player's target for minions — maintain old behavior
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            // If we have owner item type saved, use that as the tag identity (safer than comparing strings)
            if (_ownerItemType != ItemID.None)
            {
                if (Main.player[Projectile.owner].HeldItem == null)
                    return;

                /*
                // Example: apply a "whip tag" effect using owner item type
                var global = target.GetGlobalNPC<WhipDebuffNPC>();
                if (global != null)
                {
                    // Check existing tags for this item type
                    bool found = false;
                    foreach (var tp in global.Tags)
                    {
                        if (tp.ItemType == _ownerItemType) // using int ID
                        {
                            found = true;
                            // refresh time if needed (example assumes bw.TagTime accessible via item)
                            var maybeItem = Main.player[Projectile.owner].HeldItem.ModItem as BaseWhipItem;
                            if (maybeItem != null && tp.TimeLeft < maybeItem.TagTime)
                                tp.TimeLeft = maybeItem.TagTime;
                            break;
                        }
                    }
                    if (!found)
                    {
                        var maybeItem = Main.player[Projectile.owner].HeldItem.ModItem as BaseWhipItem;
                        if (maybeItem != null)
                        {
                            global.Tags.Add(new WhipTag(_ownerItemType, maybeItem.TagTime, maybeItem.TagDamage, maybeItem.TagDamageMult, maybeItem.TagCritChance, GetTagEffectName));
                        }
                    }
                }*/
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Build our custom control points
            List<Vector2> betterPoints = new();
            GetWhipSettingsBetter(Projectile, out float flyTime, out int segments, out float rangeMult);
            FillWhipControlPointsBetter(Projectile, betterPoints, segments, rangeMult, flyTime);

            // Check collision along each whip segment
            for (int i = 1; i < betterPoints.Count; i++)
            {
                Vector2 p1 = betterPoints[i - 1];
                Vector2 p2 = betterPoints[i];

                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(
                    targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    p1, p2,
                    Projectile.width, ref collisionPoint))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual string GetTagEffectName => "";

        #region DrawCode
        public override bool PreDraw(ref Color lightColor)
        {
            _controlPoints.Clear();
            FillWhipControlPointsBetter(Projectile, _controlPoints, Segments, RangeMult, _flyTime);
            ModifyControlPoints(_controlPoints);

            DrawStrings(_controlPoints);
            DrawSegs(_controlPoints, 4);

            return false; // we've drawn it
        }

        protected virtual void DrawStrings(List<Vector2> points)
        {
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 mid = Vector2.Lerp(points[i - 1], points[i], 0.5f);
                Color color = Color.Lerp(Lighting.GetColor((int)(mid.X / 16f), (int)(mid.Y / 16f), StringColor), StringColor, Projectile.light);
                Utils.DrawLine(Main.spriteBatch, points[i - 1], points[i], color, color, 2 * Projectile.scale);
                //Utils.DrawBorderString(Main.spriteBatch, i.ToString(), points[i] - Main.screenPosition, Color.Red, 0.4f);
            }
        }

        protected virtual void DrawSegs(List<Vector2> points, int FrameCount)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int frameY = 0, frameHeight = 0;
                Vector2 origin = Vector2.Zero;
                //GetFrame(i, points.Count, ref frameY, ref frameHeight, ref origin);
                float drawScale = Projectile.scale * GetSegScale(i, points.Count);

                Vector2 lightPos = i == 0 ? points[i] : Vector2.Lerp(points[i - 1], points[i], 0.5f);
                Color color = Color.Lerp(Lighting.GetColor((int)(lightPos.X / 16f), (int)(lightPos.Y / 16f)), StringColor, Projectile.light);

                float rot;
                if (i == points.Count - 1)
                    rot = (points[i] - points[i - 1]).ToRotation();
                else
                    rot = (points[i + 1] - points[i]).ToRotation();
                rot -= MathHelper.PiOver2;

                Texture2D whipTex = ModContent.Request<Texture2D>(Texture).Value;

                Main.EntitySpriteDraw(whipTex, points[i] - Main.screenPosition, new Rectangle(0, frameY, whipTex.Width, frameHeight), color, rot, origin, drawScale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
        }


        public virtual void GetFrame(int segIndex, int segCount, ref int frameY, ref int frameHeight, ref Vector2 origin)
        {
            Texture2D whipTex = ModContent.Request<Texture2D>(Texture).Value;
            if (segIndex == 0)
            {
                frameY = 0;
                frameHeight = handleHeight;
                origin = new Vector2(whipTex.Width, handleHeight) * 0.5f;
            }
            else if (segIndex == segCount - 1)
            {
                frameY = whipTex.Height - endHeight;
                frameHeight = endHeight;
                origin = new Vector2(whipTex.Width, endHeight) * 0.5f;
            }
            else
            {
                frameY = handleHeight + (segIndex - 1) % segTypes * segHeight;
                frameHeight = segHeight;
                origin = new Vector2(whipTex.Width, segHeight) * 0.5f;
            }
        }

        public virtual float GetSegScale(int segIndex, int segCount) => 1f;
        #endregion
    }
}
