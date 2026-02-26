using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
#pragma warning disable IDE1006

namespace HeavenlyArsenal.Common
{
    public abstract class BaseWhipProjectile : ModProjectile
    {
        #region Modular whip integration
        protected readonly List<Vector2> ControlPoints = new();
        /// <summary>
        /// sets up the modifiers for the whip via <see cref="WhipMotions"/>. 
        /// </summary>
        /// <param name="controller"> the WhipController of this Whip. </param>
        protected virtual void SetupModifiers(ModularWhipController controller)
        {
        }

        /// <summary>
        /// override to change the type of motion your whip exhibits.
        /// </summary>
        /// <returns></returns>
        protected virtual IWhipMotion CreateMotion() => new WhipMotions.VanillaWhipMotion();

        protected ModularWhipController WhipController;
        private void ModifyControlPoints(List<Vector2> points)
        {
            if (WhipController == null)
            {
                WhipController = new(CreateMotion());
            }

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int segments, out float rangeMultiplier);

            float progress = MathHelper.Clamp(Time / timeToFlyOut, 0f, 1f);


            SetupModifiers(controller: WhipController);
            WhipController.Apply(
                points,
                Projectile,
                segments,
                rangeMultiplier,
                progress
            );

            //Main.NewText(WhipController.ToString());
        }

        #endregion
        #region GlobalProjectile and setstaticDefaults
        internal class BaseWhipGlobal : GlobalProjectile
        {
            public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
            {
                lateInstantiation = true;
                return entity.ModProjectile is not null && entity.ModProjectile is BaseWhipProjectile;
            }
            public override void OnSpawn(Projectile projectile, IEntitySource source)
            {
                if (projectile.ModProjectile != null)
                {
                    if (projectile.ModProjectile is BaseWhipProjectile)
                    {
                        BaseWhipProjectile whip = projectile.ModProjectile as BaseWhipProjectile;
                        projectile.WhipSettings.Segments = (int)Math.Ceiling(40f * Main.player[projectile.owner].whipRangeMultiplier);
                        projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                        projectile.spriteDirection = Math.Sign(projectile.velocity.X);
                        projectile.aiStyle = -1;
                        whip.Prepare();
                    }
                }
            }
        }
        public sealed override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        #endregion
        #region SoundStuff
        /// <summary>
        /// default sound.
        /// </summary>
        private SoundStyle _WhipHitSFX => SoundID.Item153;
        /// <summary>
        /// override to adjust the sound that plays when the whip is cracked. 
        /// set to null to prevent the whip from making any sound by default.
        /// </summary>
        public virtual SoundStyle? WhipCrack_SFX => _WhipHitSFX; 
        #endregion
        #region useful things
        public int Time
        {
            get => (int)Projectile.ai[0];
        }
        public ref Player Owner => ref Main.player[Projectile.owner];
        /// <summary>
        /// A general use timer, for visual effects.
        /// </summary>
        public int GeneralTime
        {
            get=> (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        #endregion


        /// <summary>
        /// called directly after onSpawn usually would be.
        /// </summary>
        public virtual void Prepare() { }
        /// <summary>
        /// use as a replacement for <see cref="Projectile.AI()"/>, as that is busy being used to control the whipcrack and other things. 
        /// </summary>
        /// <remarks>
        /// if you want to replace the default behavior of the whip for whatever reason, then you should override <see cref="Projectile.PreAI()"/> and return false.
        /// </remarks>
        public virtual void AI2() { }


        #region New AI

        protected float _WhipProgress;
        protected float _FlyTime;
        protected int SegmentCount;
        protected float RangeMultiplier;

    
        public sealed override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;

            Projectile.GetWhipSettings(
                Projectile,
                out float timeToFlyOut,
                out int segments,
                out float rangeMultiplier
            );
         
            _FlyTime = timeToFlyOut;
            SegmentCount = segments;
            RangeMultiplier = rangeMultiplier;

            if (Time == 0)
                Projectile.timeLeft = (int)_FlyTime;

            Projectile.ai[0]++;
            _WhipProgress = Projectile.ai[0] / _FlyTime;

            UpdateWhip(_WhipProgress);

            if (Projectile.ai[0] >= _FlyTime)
                Projectile.Kill();

            AI2();
        }


        private bool cracked;
        protected virtual float GetExtensionFactor(float progress)
        {
            if (progress < 0.5f)
                return progress * 2f;
            else
                return 2f - progress * 2f;
        }

        protected float GetCurrentReach()
        {
            float extension = GetExtensionFactor(_WhipProgress);

            float baseReach = 160f;

            return baseReach * RangeMultiplier * extension;
        }
        /// <summary>
        /// override this to create effects during the swing that are dependent on <see cref="_WhipProgress"/>
        /// </summary>
        /// <param name="progress">a float that represents the progress of the whip (from 0 to 1). </param>
        protected virtual void UpdateWhip(float progress)
        {
            float extension = GetExtensionFactor(progress);

            if (!cracked && extension >= 1f)
            {
                PlayCrackSound();
                cracked = true;
            }
        }

        public virtual void PlayCrackSound()
        {
            if(WhipCrack_SFX != null)
            SoundEngine.PlaySound(WhipCrack_SFX, Projectile.Center);
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
            _WhipProgress = Projectile.ai[0] / _FlyTime;
            float thing = GetExtensionFactor(_WhipProgress);
            if (thing !> 0.75f && thing !< 0.85f)
                return;


            List<Vector2> collisionPoints = Projectile.WhipPointsForCollision;

            if (collisionPoints == null || collisionPoints.Count < 2)
                return;

            int tipIndex = collisionPoints.Count - 1;

            // Check which segment hit by comparing distances
            int closestIndex = -1;
            float closestDist = float.MaxValue;

            for (int i = 0; i < collisionPoints.Count; i++)
            {
                float dist = Vector2.DistanceSquared(collisionPoints[i], target.Center);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            bool tipHit = closestIndex >= tipIndex - 1;

            if (tipHit)
            {
                modifiers.SourceDamage *= TipDamageMulti;

                OnTipHit(target);
            }
        }
        public virtual float TipDamageMulti => 1.5f;
        /// <summary>
        /// Allows easy ability to do special stuff on any entity hit with just the tip. 
        /// </summary>
        /// <param name="target">   </param>
        public virtual void OnTipHit(Entity target  )
        {

        }

        #endregion
        #region Detours
        public sealed override void Load()
        {
            On_Projectile.TryDoingOnHitEffects += On_Projectile_TryDoingOnHitEffects;
            On_Projectile.SetDefaults += ApplyExtraStuff;

        }


        private void ApplyExtraStuff(On_Projectile.orig_SetDefaults orig, Projectile self, int Type)
        {


            orig(self, Type);
            if (self.ModProjectile is not null && self.ModProjectile is BaseWhipProjectile)
            {
                self.WhipSettings.Segments = (int)Math.Ceiling(40f * Main.player[self.owner].whipRangeMultiplier);
                self.DefaultToWhip();
            }
        }
        #endregion
        #region OnHit Effects
        private static int _whipEffectAmounts;
        /// <summary>
        /// struct that just holds the duration and ID of any buffs that you want to apply to enemies, since it would kind of suck if you could only have one of them.
        /// </summary>
        private readonly struct OnHitEffects
        {
            public readonly int OnHitEffectID;
            public readonly int OnHitDuration;

            public OnHitEffects(int ID, int Duration)
            {
                OnHitEffectID = ID;
                OnHitDuration = Duration;
            }
        }

        private List<OnHitEffects> _OnHitEffects = new();

        /// <summary>
        /// Adds an onhit effect to the projectile. this will run regardless of what you put in onhiteffects, so joyous.
        /// </summary>
        /// <param name="EffectID">The BuffType or BuffID that the whip should apply when hitting an enemy.</param>
        /// <param name="Duration">the duration of the applied buff.</param>
        public void AddHitEffects(int EffectID, int Duration = 40 * 6)
        {
            OnHitEffects hitEffects = new OnHitEffects(EffectID, Duration);
            if (!_OnHitEffects.Contains(hitEffects))
            {
                _OnHitEffects.Add(hitEffects);
                _whipEffectAmounts++;
                _OnHitEffects.EnsureCapacity(_whipEffectAmounts);
                return;
            }
        }
        private void On_Projectile_TryDoingOnHitEffects(On_Projectile.orig_TryDoingOnHitEffects orig, Projectile self, Entity entity)
        {
            orig(self, entity);

            if (entity is NPC)
            {
                NPC target = (NPC)entity;


                if (self.ModProjectile is not null && self.ModProjectile is BaseWhipProjectile)
                {
                    BaseWhipProjectile projectile = (BaseWhipProjectile)self.ModProjectile;
                    if (projectile._OnHitEffects.Count > 0)
                    {
                        for (int i = 0; i < projectile._OnHitEffects.Count; i++)
                        {
                            target.AddBuff(projectile._OnHitEffects[i].OnHitEffectID, projectile._OnHitEffects[i].OnHitDuration);
                        }
                    }

                }

            }
            else if (entity is Player)
            {
                Player target = (Player)entity;


                if (self.ModProjectile is not null && self.ModProjectile is BaseWhipProjectile)
                {
                    BaseWhipProjectile projectile = (BaseWhipProjectile)self.ModProjectile;
                    if (projectile._OnHitEffects.Count > 0)
                    {
                        for (int i = 0; i < _OnHitEffects.Count; i++)
                        {
                            target.AddBuff(_OnHitEffects[i].OnHitEffectID, _OnHitEffects[i].OnHitDuration);
                        }
                    }

                }
            }
        }



        #endregion

        #region Drawing
        protected virtual Texture2D texture => null;
        public bool _ShouldDrawNormal = true;
        public bool _ShouldDrawHandle = true;
        public bool _HeadShouldFlip = true;


        #region Primitive

        private BasicEffect _whipEffect;
        public bool _ShouldDrawPrimitive = true;
        public virtual bool PrimitiveGlows => false;

        public virtual SamplerState PrimSampler => SamplerState.PointWrap;

        /// <summary>
        /// the texture to be used by the primitive.
        /// whether it is scrolling or not is controlled by <see cref="_PrimitiveIsScrollingTexture"/>, and the scroll rate is controlled by <see cref="_PrimitiveScrollRate"/>. 
        /// </summary>
        protected virtual Texture2D PrimitiveTex => null;
        /// <summary>
        /// if you set this, make sure that <see cref="PrimSampler"/> is set to <see cref="SamplerState.PointWrap"/> otherwise the texture might not scroll properly.
        /// </summary>
        public virtual bool _PrimitiveIsScrollingTexture => false;
        public virtual float _PrimitiveScrollRate() => 0.0f;


        private float _Alpha => Projectile.alpha;

        public virtual Color WhipColor => Color.White;

        /// <summary>
        /// Returns the color of the whip at a normalized position t (0..1).
        /// Default implementation returns <see cref="WhipColor"/>, preserving existing behavior.
        /// Override this to provide color gradients / time-based coloration along the primitive.
        /// </summary>
        /// <param name="t">Normalized position along the whip (0 = handle, 1 = tip).</param>
        public virtual Color GetWhipColor(float t) => WhipColor;

        /// <summary>
        /// Returns the color at a point along and across the whip.
        /// t = position along whip (0..1)
        /// w = position across width (-1..1), where:
        ///     0 = center
        ///    -1 = left edge
        ///     1 = right edge
        /// </summary>
        public virtual Color GetWhipColor(float t, float w)
        {
            return GetWhipColor(t);
        }
        /// <summary>
        /// Allows overriding the thickness of the primitive strip via a function.
        /// - baseWidth: the base width value passed by the caller (preserves existing calls)
        /// - t: normalized position along the whip (0..1)
        /// Default behavior reproduces prior tapering: baseWidth * Lerp(1.2f, 0.4f, t)
        /// </summary>
        public virtual float GetWhipWidth(float baseWidth, float t)
        {
            return baseWidth * MathHelper.Lerp(1.2f, 0.4f, t);
        }




        /// <summary>
        /// Draws a textured primitive strip along the provided polyline.
        /// </summary>
        /// <param name="points">Resampled polyline in world coordinates (screen offset applied inside).</param>
        /// <param name="BaseColor">Base color applied to vertices (then multiplied by lighting).</param>
        /// <param name="baseWidth">Base half-width in pixels used by the GetWhipWidth function.</param>
        /// <param name="textureRepeats">
        /// How many times the texture should repeat along the entire whip length (default 1f).
        /// Increase to tile the texture multiple times across the full whip.
        /// </param>
        /// <param name="uOffset">
        /// Offset added to the computed U coordinate (useful to align handle texels). Default 0f.
        /// </param>
        private void DrawWhipPrimitive(List<Vector2> points, Color BaseColor, float Alpha = 0, float baseWidth = 6f, float pixelsPerRepeat = 32f, float uOffset = 0f)
        {

            if (points.Count < 3)
                return;

            if (Main.netMode == NetmodeID.Server)
                return;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            _whipEffect ??= new BasicEffect(gd)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                LightingEnabled = false
            };


            Texture2D tex = PrimitiveTex == null ? TextureAssets.MagicPixel.Value : PrimitiveTex;

            _whipEffect.Texture = tex;
            _whipEffect.View = Main.GameViewMatrix.TransformationMatrix;
            _whipEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1f, 1f
            );
            _whipEffect.World = Matrix.Identity;

            List<VertexPositionColorTexture> verts = new();

            float totalLength = 0f;
            for (int i = 0; i < points.Count - 1; i++)
                totalLength += Vector2.Distance(points[i], points[i + 1]);

            float accumulated = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                Vector2 dir;

                if (i == 0)
                    dir = points[1] - p;
                else if (i == points.Count - 1)
                    dir = p - points[i - 1];
                else
                    dir = points[i + 1] - points[i - 1];

                if (dir.LengthSquared() < 0.001f)
                    continue;

                dir.Normalize();

                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float t = (totalLength > 0f) ? (accumulated / totalLength) : 0f;
                // Use the overrideable GetWhipWidth function to compute the width for this sample.
                float width = GetWhipWidth(baseWidth, t);
                float ScrollOffset =
                _PrimitiveIsScrollingTexture
                ? _PrimitiveScrollRate() * Main.GlobalTimeWrappedHourly
                : 0f;

                float u = (accumulated / pixelsPerRepeat) + uOffset + ScrollOffset;
                // Use per-sample color provider if overridden.
                Color lightingColor = !PrimitiveGlows ? Lighting.GetColor(p.ToTileCoordinates()) : Color.White;

                float w = (width / 2f) / width;  // gives -1 to +1
                float dist = MathF.Abs(w);       // gives 0 center -> 1 edge
                float wRight = -1f * dist;
                float wLeft = 1f * dist;
                Color rightColor = GetWhipColor(t, wRight).MultiplyRGB(lightingColor);
                Color leftColor = GetWhipColor(t, wLeft).MultiplyRGB(lightingColor);

                Alpha = Math.Clamp(Alpha, 0f, 255f);

                leftColor = leftColor with { A = (byte)Alpha };
                rightColor = rightColor with { A = (byte)Alpha };

                Vector2 screen = p - Main.screenPosition;

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(screen + normal * width, 0f),
                    rightColor,
                    new Vector2(u, 0f)
                ));

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(screen - normal * width, 0f),
                    leftColor,
                    new Vector2(u, 1f)
                ));

                if (i < points.Count - 1)
                    accumulated += Vector2.Distance(points[i], points[i + 1]);
            }

            if (verts.Count < 4)
                return;

            gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid };
            gd.SamplerStates[0] = PrimSampler;

            foreach (EffectPass pass in _whipEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    verts.ToArray(),
                    0,
                    verts.Count - 2
                );
            }
        }

        private static List<Vector2> ResamplePolyline(List<Vector2> points, float spacing)
        {
            List<Vector2> result = new();
            if (points.Count < 2)
                return result;

            result.Add(points[0]);

            Vector2 prev = points[0];
            float carry = 0f;

            for (int i = 1; i < points.Count; i++)
            {
                Vector2 curr = points[i];
                Vector2 delta = curr - prev;
                float length = delta.Length();

                if (length <= 0.0001f)
                    continue;

                Vector2 dir = delta / length;

                float dist = spacing - carry;
                while (dist <= length)
                {
                    Vector2 sample = prev + dir * dist;
                    result.Add(sample);
                    dist += spacing;
                }

                carry = length - (dist - spacing);
                prev = curr;
            }

            // Ensure the tip is included
            if (result[^1] != points[^1])
                result.Add(points[^1]);

            return result;
        }


        #endregion
        #region head

        /// <summary>
        /// the texture to use as the head of the whip.
        /// </summary>
        protected virtual Texture2D WhipHead => null;

        public bool _ShouldDrawHead = true;



        public float _HeadScaleAmount = 0.15f;

        public int _Head_HorizontalFrames = 1;
        public int _Head_VerticalFrames = 1;
        public int _Head_x = 0;
        public int _Head_y = 0;

        /// <summary>
        /// creates the rectangle for the head, this allows for animated head sprites.
        /// </summary>
        /// <param name="horizontalFrames">the amount of horizontal frames in this rectangle. </param>
        /// <param name="veritcalFrames">the amount of vertical frames in this rectangle. </param>
        /// <param name="x">the x of the frame you are currently on. </param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected Rectangle HeadRect(int horizontalFrames = 1, int veritcalFrames = 1, int x = 1, int y = 1)
        {
            if (WhipHead is not null)
            {
                _Head_HorizontalFrames = horizontalFrames;
                _Head_VerticalFrames = veritcalFrames;
                return WhipHead.Frame(horizontalFrames, veritcalFrames, x, y);
            }
            else return new();
        }

        public Rectangle _HeadRectangle => HeadRect(_Head_HorizontalFrames, _Head_VerticalFrames, this._Head_x, this._Head_y);

        public Vector2 _HeadOffset = Vector2.Zero;

        protected virtual Vector2 HeadOrigin(Vector2 Offset)
        {

            if (!_HeadRectangle.IsEmpty)
                return _HeadRectangle.Size() / 2f + Offset;

            if (WhipHead != null)
                return WhipHead.Size() / 2 + Offset;

            return Offset;
        }

        protected virtual float CalculateHeadScale()
        {
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float rangeMultiplier);

            float normalized = timeToFlyOut > 0f ? Math.Clamp((float)Time / timeToFlyOut, 0f, 1f) : 1f;
            float apex = MathF.Sin(normalized * MathF.PI);
            float apexAmplitude = _HeadScaleAmount;


            float minScale = 0.5f;
            float maxScale = 1f + apexAmplitude * rangeMultiplier;

            float scaleAmount = MathHelper.Lerp(minScale, maxScale, apex);

            return scaleAmount;
        }

        protected virtual void drawHead(Vector2 HeadPos, float BaseRotation, List<Vector2> list, SpriteEffects flip, Color LightColor)
        {
            Vector2 headpos = list[list.Count-2];
            Rectangle HeadFrame = _HeadRectangle;
          
            Color HeadColor = Lighting.GetColor(list[^1].ToTileCoordinates());
            SpriteEffects _flip = _HeadShouldFlip ? flip : 0;


            Vector2 Origin = HeadOrigin(_HeadOffset);
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float rangeMultiplier);

            Vector2 Scale = new Vector2(CalculateHeadScale());
            
            Main.EntitySpriteDraw(WhipHead, headpos - Main.screenPosition, HeadFrame, HeadColor, BaseRotation, Origin, Scale, _flip);
        }
        #endregion
        #region Normal

        protected virtual void DrawNormal(List<Vector2> list, SpriteEffects flip)
        {               //Main.DrawWhip_WhipBland(Projectile, renderPoints);
                        // The code below is for custom drawing.
                        // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
                        // However, you must adhere to how they draw if you do.

            Vector2 pos = list[0];


            float handlerot = 0;

            for (int i = 0; i < list.Count - 1; i++)
            {
                // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
                // You can change them if they don't!
                Rectangle frame = new Rectangle(0, 0, 18, 20); // The size of the Handle (measured in pixels)
                Vector2 origin = new Vector2(8, 4f);
                if (Projectile.spriteDirection > 0) //trying to keeping the sprite lined up with the string as best as possible
                    origin.X += 2;
                float scale = 1;

                // These statements determine what part of the spritesheet to draw for the current segment.
                // They can also be changed to suit your sprite.

                Vector2 element = list[i];

                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

                if (i < 1)
                {
                    pos += diff;
                    continue;
                }
                else if (i == 1)
                {
                    handlerot = rotation;
                    origin = new Vector2(frame.Width / 2, frame.Height - 4);
                    pos += diff;
                    continue;
                }
                else if (i == list.Count - 2)
                {
                    // This is the head of the whip. You need to measure the sprite to figure out these values.
                    frame.Y = 20 + 16 * 3; // Distance from the top of the sprite to the start of the frame.
                    frame.Height = 16; // Height of the frame.

                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                }
                else if (i % 6 == 0)
                {
                    // Third segment
                    frame.Y = 20 + 32;
                    frame.Height = 8;
                }
                else if (i % 5 == 0)
                {
                    // Third segment
                    frame.Y = 20 + 40;
                    frame.Height = 8;
                }
                else if (i % 4 == 0)
                {
                    // Third segment
                    frame.Y = 20 + 24;
                    frame.Height = 8;
                }
                else if (i % 3 == 0)
                {
                    // Third segment
                    frame.Y = 20 + 8;
                    frame.Height = 8;
                }
                else if (i % 2 == 0)
                {
                    // Second Segment
                    frame.Y = 20 + 16;
                    frame.Height = 8;
                }
                else
                {
                    // First Segment
                    frame.Y = 20;
                    frame.Height = 8;
                }

                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.spriteBatch.Draw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }


        }
        #endregion
        #region OverPrimitive
        protected virtual void DrawOverPrimitive(List<Vector2> points)
        {
            // draw stuff
        }

        #endregion
        #region Handle

        protected virtual Texture2D WhipHandle => null;
        /// <summary>
        /// stores the values created for the purposes of animation later
        /// </summary>
        private int _Handle_HorizontalFrames = 1;
        private int _Handle_VerticalFrames = 1;
        private int _Handle_x;
        private int _Handle_y;
        /// <summary>
        /// creates the rectangle for the handle, this allows for animated handle sprites.
        /// </summary>
        /// <param name="horizontalFrames">the amount of horizontal frames in this rectangle. </param>
        /// <param name="veritcalFrames">the amount of vertical frames in this rectangle. </param>
        /// <param name="x">the x of the frame you are currently on. </param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected virtual Rectangle HandleRect(int horizontalFrames=1, int veritcalFrames=1, int x = 1, int y = 1)
        {
            if (WhipHandle is not null)
            {
                _Handle_HorizontalFrames = horizontalFrames;
                _Handle_VerticalFrames = veritcalFrames;
                this._Handle_x = x;
                this._Handle_y = y;
                return WhipHandle.Frame(horizontalFrames, veritcalFrames, x, y);
            }
            else return new();
        }

        public Rectangle _HandleRectangle => HandleRect(_Handle_HorizontalFrames, _Handle_VerticalFrames, _Handle_x, _Handle_y);

        public Vector2 _Offset = Vector2.Zero;
        protected virtual Vector2 HandleOrigin(Vector2 Offset)
        {
            _Offset = Offset;
            if (!_HandleRectangle.IsEmpty)
            {
                return _HandleRectangle.Size() / 2f + Offset;
            }

            if (WhipHandle != null)
            {
                return WhipHandle.Size() / 2 + Offset;
            }

            return Offset;
        }

        protected virtual void DrawHandle(List<Vector2> list, SpriteEffects flip)
        {

            Vector2 handlepos = list[0];
            Rectangle handleframe = _HandleRectangle;
            Vector2 element = list[0];

            Vector2 diff = list[1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            float handlerot = rotation;
            Color handlecolor = Lighting.GetColor(list[1].ToTileCoordinates());
            Vector2 Origin = HandleOrigin(_Offset);
            Main.EntitySpriteDraw(WhipHandle, handlepos - Main.screenPosition, handleframe, handlecolor, handlerot, Origin, 1, flip);

        }
        #endregion



        protected virtual float RenderSpacing => 4f;

        public bool _DebugMode;
        //change me to affect how often your texture repeats!
        public float _PixelsPerRepeat => 32;
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            this.ModifyControlPoints(list);

            Texture2D texture = TextureAssets.Projectile[Type].Value;

            SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            
            List<Vector2> controlPoints = list;

            List<Vector2> renderPoints = ResamplePolyline(controlPoints, RenderSpacing);
            if (_DebugMode)
            {

                for (int i = 0; i < renderPoints.Count - 1; i++)
                {
                    NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(Main.spriteBatch, renderPoints[i], renderPoints[i + 1], Color.White,2);
                }
            }

            foreach (WhipDrawPass pass in DrawOrder)
            {
                switch (pass)
                {
                    case WhipDrawPass.Primitive:
                        if (_ShouldDrawPrimitive)
                        {
                            float lodSpacing = 3f;
                            List<Vector2> dense = ResamplePolyline(list, lodSpacing);

                            // Use default textureRepeats = 1 and uOffset = 0 to preserve prior behavior,
                            // but allow overrides in derived classes via calling the overload if desired.
                            DrawWhipPrimitive(dense, WhipColor, Alpha: _Alpha, baseWidth: 2f, uOffset: 0.2f, pixelsPerRepeat:_PixelsPerRepeat);
                        }
                        break;

                    case WhipDrawPass.Normal:
                        if (_ShouldDrawNormal)
                        {
                            DrawNormal(list, flip);

                        }
                        break;

                    case WhipDrawPass.Handle:  
                        if (_ShouldDrawHandle && WhipHandle is not null)
                        {
                            DrawHandle(list, flip);
                        }
                        break;

                    case WhipDrawPass.OverPrimtive:

                        DrawOverPrimitive(renderPoints);
                        break;

                    case WhipDrawPass.Head:
                        if (_ShouldDrawHead && WhipHead is not null)
                        {

                            Vector2 headpos = list[list.Count - 2];
                            Color HeadColor = Lighting.GetColor(list[^1].ToTileCoordinates());
                            Vector2 element = list[list.Count - 3];

                            Vector2 diff = headpos - element;

                            float rotation = diff.ToRotation() - MathHelper.PiOver2;
                            drawHead(HeadPos: headpos, rotation, list, flip, LightColor: HeadColor);

                        }
                        break;
                }
            }
            return false;
        }

        protected Vector2 GetPointAlongWhip(List<Vector2> points, float t)
        {
            if (points.Count == 0)
                return Vector2.Zero;

            float totalLength = 0f;

            for (int i = 0; i < points.Count - 1; i++)
                totalLength += Vector2.Distance(points[i], points[i + 1]);

            float targetLength = totalLength * MathHelper.Clamp(t, 0f, 1f);

            float accumulated = 0f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[i + 1];

                float segmentLength = Vector2.Distance(a, b);

                if (accumulated + segmentLength >= targetLength)
                {
                    float localT = (targetLength - accumulated) / segmentLength;
                    return Vector2.Lerp(a, b, localT);
                }

                accumulated += segmentLength;
            }

            return points[^1];
        }

        protected float GetRotationAlongWhip(List<Vector2> points, float t)
        {
            float dt = 0.001f;

            Vector2 a = GetPointAlongWhip(points, Math.Max(0f, t - dt));
            Vector2 b = GetPointAlongWhip(points, Math.Min(1f, t + dt));

            return (b - a).ToRotation() + MathHelper.PiOver2;
        }
        protected Vector2 GetNormalAlongWhip(List<Vector2> points, float t)
        {
            float dt = 0.001f;

            Vector2 a = GetPointAlongWhip(points, Math.Max(0f, t - dt));
            Vector2 b = GetPointAlongWhip(points, Math.Min(1f, t + dt));

            Vector2 dir = b - a;

            if (dir.LengthSquared() < 0.0001f)
                return Vector2.Zero;

            dir.Normalize();

            return new Vector2(-dir.Y, dir.X);
        }

        protected enum WhipDrawPass
        {
            Primitive,
            Normal,
            Handle,
            OverPrimtive,
            Head
        }
        /// <summary>
        /// people can override this to adjust what is actually drawn on the whip.
        /// </summary>
        protected virtual IReadOnlyList<WhipDrawPass> DrawOrder =>
            new[]
            {
                WhipDrawPass.Primitive,
                WhipDrawPass.Normal,
                WhipDrawPass.Handle,
                WhipDrawPass.OverPrimtive,
                WhipDrawPass.Head,
            };


        #endregion
    }
}
