using Luminance.Assets;
using NoxusBoss.Assets;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls
{
    public class NeuronWormThrall : BloodThrallBase
    {

        public override ThrallType ThrallType => ThrallType.NerveWormThrall;

        public override void UpdateFromOvermind(OvermindContext context)
        {
            this.TryAutoJoinNearbyWorm();
            
            if(IsChainHead)
            if(Main.GameUpdateCount % 20 == 0)
            {
                Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PsychicShock_Proj>(), 300, 0);
            }
        }

        public const int MAX_SEGMENTS = 10;
        public const int MIN_SEGMENTS = 3;

        public Vector2[] Segments;

        public Rectangle[] SegmentHitboxes;



        private enum WormAIMode
        {
            Idle,
            SeekingEnemy,
            AttackingEnemy,
            OvermindControlled
        }

        private WormAIMode aiMode = WormAIMode.Idle;

      
        public struct WormJoiner : IEquatable<WormJoiner>
        {
            // Whether this worm is currently joined to another worm/segment.
            public bool IsJoined;
            /// <summary>
            ///  Owner player index of the leader projectile (the worm we join to).
            /// -1 when not set.
            /// </summary>
            public int LeaderOwnerPlayer;

            /// <summary>
            /// ProjectileWhoAmI of the leader projectile (identifier within Main.projectile).
            /// </summary>
            public int LeaderProjectileWhoAmI;

            /// <summary>
            /// Which segment index on the leader worm is being joined to.
            /// </summary>
            public int LeaderSegmentIndex;

            /// <summary>
            /// Which local segment index on this worm is participating in the join.
            /// </summary>
            public int LocalSegmentIndex;

            /// <summary>
            ///  True if the connection is oriented head-to-tail (head of this -> tail of leader),
            /// false if opposite. The exact meaning can be interpreted by AI/leader logic.
            /// </summary>
            
            public bool HeadToTail;

            public static WormJoiner Empty => new WormJoiner(false, -1, -1, -1, -1, false);

            public WormJoiner(bool isJoined, int leaderOwnerPlayer, int leaderProjectileWhoAmI, int leaderSegmentIndex, int localSegmentIndex, bool headToTail)
            {
                IsJoined = isJoined;
                LeaderOwnerPlayer = leaderOwnerPlayer;
                LeaderProjectileWhoAmI = leaderProjectileWhoAmI;
                LeaderSegmentIndex = leaderSegmentIndex;
                LocalSegmentIndex = localSegmentIndex;
                HeadToTail = headToTail;
            }

            public static WormJoiner CreateJoin(int leaderOwnerPlayer, int leaderProjectileWhoAmI, int leaderSegmentIndex, int localSegmentIndex, bool headToTail)
            {
                return new WormJoiner(true, leaderOwnerPlayer, leaderProjectileWhoAmI, leaderSegmentIndex, localSegmentIndex, headToTail);
            }

            // Clears the join
            public void Break()
            {
                IsJoined = false;
                LeaderOwnerPlayer = -1;
                LeaderProjectileWhoAmI = -1;
                LeaderSegmentIndex = -1;
                LocalSegmentIndex = -1;
                HeadToTail = false;
            }

            // Quick validity check
            public bool IsValid()
            {
                return IsJoined && LeaderOwnerPlayer >= 0 && LeaderProjectileWhoAmI >= 0 && LeaderSegmentIndex >= 0 && LocalSegmentIndex >= 0;
            }

            // Serialize to a compact int array: [IsJoined(0/1), LeaderOwnerPlayer, LeaderProjectileWhoAmI, LeaderSegmentIndex, LocalSegmentIndex, HeadToTail(0/1)]
            // This keeps the representation simple to persist by surrounding code.
            public int[] Serialize()
            {
                return new int[]
                {
                        IsJoined ? 1 : 0,
                        LeaderOwnerPlayer,
                        LeaderProjectileWhoAmI,
                        LeaderSegmentIndex,
                        LocalSegmentIndex,
                        HeadToTail ? 1 : 0
                };
            }

            // Deserialize from the compact int array produced by Serialize().
            // If the array is null or not long enough, returns Empty.
            public static WormJoiner Deserialize(int[] data)
            {
                if (data == null || data.Length < 6)
                    return Empty;

                bool isJoined = data[0] != 0;
                int leaderOwnerPlayer = data[1];
                int leaderProjectileWhoAmI = data[2];
                int leaderSegmentIndex = data[3];
                int localSegmentIndex = data[4];
                bool headToTail = data[5] != 0;

                return new WormJoiner(isJoined, leaderOwnerPlayer, leaderProjectileWhoAmI, leaderSegmentIndex, localSegmentIndex, headToTail);
            }

            public override string ToString()
            {
                if (!IsJoined) return "WormJoiner: <none>";
                return $"WormJoiner: LeaderOwner={LeaderOwnerPlayer}, LeaderProj={LeaderProjectileWhoAmI}, LeaderSeg={LeaderSegmentIndex}, LocalSeg={LocalSegmentIndex}, HeadToTail={HeadToTail}";
            }

            public bool Equals(WormJoiner other)
            {
                return IsJoined == other.IsJoined
                    && LeaderOwnerPlayer == other.LeaderOwnerPlayer
                    && LeaderProjectileWhoAmI == other.LeaderProjectileWhoAmI
                    && LeaderSegmentIndex == other.LeaderSegmentIndex
                    && LocalSegmentIndex == other.LocalSegmentIndex
                    && HeadToTail == other.HeadToTail;
            }

            public override bool Equals(object obj)
            {
                return obj is WormJoiner other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + IsJoined.GetHashCode();
                    hash = hash * 23 + LeaderOwnerPlayer.GetHashCode();
                    hash = hash * 23 + LeaderProjectileWhoAmI.GetHashCode();
                    hash = hash * 23 + LeaderSegmentIndex.GetHashCode();
                    hash = hash * 23 + LocalSegmentIndex.GetHashCode();
                    hash = hash * 23 + HeadToTail.GetHashCode();
                    return hash;
                }
            }
        }

        public bool HasFollower;

        public WormJoiner Join = WormJoiner.Empty;
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(20, 20);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Summon;
            int SegmentNum = Main.rand.Next(MIN_SEGMENTS, MAX_SEGMENTS);

            //subtract one since the default hitbox and position of the nerve trall already exists.
            Segments = new Vector2[SegmentNum - 1];
            for (int i = 0; i < Segments.Length; i++)
            {
                Segments[i] = Projectile.Center;
            }
            SegmentHitboxes = new Rectangle[SegmentNum - 1];
            for (int i = 0; i < SegmentHitboxes.Length; i++)
            {
                SegmentHitboxes[i] = new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, Projectile.width, Projectile.height);
            }

        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for(int i =0; i< SegmentHitboxes.Length; i++)
            {
                if (targetHitbox.Intersects(SegmentHitboxes[i]))
                {
                    return true;
                }
            }
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override void PostAI()
        {
            const float SearchRange = 700f;
            const float MoveSpeed = 10f;
            const float Inertia = 12f;
            const float IdleOrbitRadius = 120f;
            NPC target = Projectile.FindTargetWithinRange(SearchRange);

            if (target != null)
            {
                // Attack behavior
                MoveTowards(target.Center, MoveSpeed, Inertia);
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else
            {
                // Idle behavior: orbit owner

                Vector2 idleOffset = new Vector2(IdleOrbitRadius, 0f).RotatedBy(Main.GameUpdateCount / 50f);

                Vector2 idlePos = Owner.Center + idleOffset;
                if (IsChainHead)
                MoveTowards(idlePos, MoveSpeed , Inertia);
            }




            //Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * 10;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft++;
            float SegmentSpacing = 14f;




            if (Segments == null || SegmentHitboxes == null)
                return;


            Vector2 prevPos = Projectile.Center;

            for (int i = 0; i < Segments.Length; i++)
            {
                Vector2 current = Segments[i];

                Vector2 delta = prevPos - current;
                float dist = delta.Length();

                if (dist > SegmentSpacing)
                {
                    current += delta.SafeNormalize(Vector2.Zero) * (dist - SegmentSpacing);
                }

                Segments[i] = current;
                prevPos = current;
            }

            ApplyJoinConstraint();


            for (int i = 0; i < Segments.Length; i++)
            {
                Vector2 p = Segments[i];
                SegmentHitboxes[i] = new Rectangle(
                    (int)(p.X - Projectile.width * 0.5f),
                    (int)(p.Y - Projectile.height * 0.5f),
                    Projectile.width,
                    Projectile.height
                );
            }
        }
        private void MoveTowards(Vector2 target, float speed, float inertia)
        {
            Vector2 desired = Projectile.DirectionTo(target) * speed;
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;
        }


        public override bool PreDraw(ref Color lightColor)
        {

            if (SegmentHitboxes.Length < 0)
                return false;

            for (int i = 0; i < SegmentHitboxes.Length; i++)
            {
                //Utils.DrawRect(Main.spriteBatch, SegmentHitboxes[i], Color.White);
            }
            //Utils.DrawBorderString(Main.spriteBatch, this.HasFollower.ToString(), Projectile.Center - Main.screenPosition, Color.White) ;


            if (!IsChainHead)
                return false;

            for (int i = 0; i < 3; i++)
            {
                Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + new Vector2(40, 0).RotatedBy(i / 3f * MathHelper.PiOver4 * MathF.Sin(Main.GameUpdateCount / 10.1f) + Projectile.rotation), Color.White, Color.White, 4);
            }
            List<Vector2> raw = new(40);
            BuildChainPoints(raw);

            List<Vector2> smooth = SmoothPoints(raw, 10);

            DrawCoreSpineCustom(
                smooth,
                thickness: 10,
                color: Color.Yellow
            );

            return false;
        }


        #region RenderPrimtive
        bool IsChainHead => (Join.Equals(WormJoiner.Empty) || (!Join.IsJoined && HasFollower));
        public void AppendLocalPoints(List<Vector2> points)
        {
            // head
            points.Add(Projectile.Center);

            // body
            for (int i = 0; i < Segments.Length; i++)
                points.Add(Segments[i]);
        }
        public void BuildChainPoints(List<Vector2> points)
        {
            NeuronWormThrall current = this;

            while (current != null)
            {
                current.AppendLocalPoints(points);

                // Find follower
                NeuronWormThrall next = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.ModProjectile is not NeuronWormThrall w)
                        continue;

                    if (!w.Join.IsJoined)
                        continue;

                    if (w.Join.LeaderProjectileWhoAmI == current.Projectile.whoAmI)
                    {
                        next = w;
                        break;
                    }
                }

                current = next;
            }
        }
        public static List<Vector2> SmoothPoints(IReadOnlyList<Vector2> input, int subdivisions)
        {
            List<Vector2> result = new();

            if (input.Count < 2)
                return result;

            for (int i = 0; i < input.Count - 1; i++)
            {
                Vector2 p0 = input[Math.Max(i - 1, 0)];
                Vector2 p1 = input[i];
                Vector2 p2 = input[i + 1];
                Vector2 p3 = input[Math.Min(i + 2, input.Count - 1)];

                for (int j = 0; j < subdivisions; j++)
                {
                    float t = j / (float)subdivisions;
                    result.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                }
            }

            result.Add(input[^1]);
            return result;
        }
        private static Vector2[] ComputeTangents(IReadOnlyList<Vector2> points)
        {
            int count = points.Count;
            Vector2[] tangents = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                Vector2 prev = points[Math.Max(i - 1, 0)];
                Vector2 next = points[Math.Min(i + 1, count - 1)];
                Vector2 t = next - prev;

                if (t.LengthSquared() < 0.0001f)
                    t = Vector2.UnitX;

                tangents[i] = Vector2.Normalize(t);
            }

            return tangents;
        }
        private static Vector2[] ComputeStableNormals(Vector2[] tangents)
        {
            int count = tangents.Length;
            Vector2[] normals = new Vector2[count];

            // Initial normal
            normals[0] = new Vector2(-tangents[0].Y, tangents[0].X);

            for (int i = 1; i < count; i++)
            {
                Vector2 prevT = tangents[i - 1];
                Vector2 currT = tangents[i];

                float dot = Vector2.Dot(prevT, currT);
                dot = MathHelper.Clamp(dot, -1f, 1f);

                float angle = MathF.Acos(dot);

                if (angle < 0.001f)
                {
                    normals[i] = normals[i - 1];
                    continue;
                }

                float sign = MathF.Sign(prevT.X * currT.Y - prevT.Y * currT.X);
                normals[i] = normals[i - 1].RotatedBy(angle * sign);
            }

            return normals;
        }

        private static short[] BuildStripIndices(int pointCount)
        {
            // 2 verts per point → strip length = 2 * pointCount
            short[] indices = new short[(pointCount - 1) * 6];
            int idx = 0;

            for (int i = 0; i < pointCount - 1; i++)
            {
                short a = (short)(i * 2);
                short b = (short)(i * 2 + 1);
                short c = (short)(i * 2 + 2);
                short d = (short)(i * 2 + 3);

                // Triangle 1
                indices[idx++] = a;
                indices[idx++] = b;
                indices[idx++] = c;

                // Triangle 2
                indices[idx++] = b;
                indices[idx++] = d;
                indices[idx++] = c;
            }

            return indices;
        }

        private static VertexPositionColorTexture[] BuildStripVerticesStable(IReadOnlyList<Vector2> points, float thickness, Color color)
        {
            int count = points.Count;
            if (count < 2)
                return Array.Empty<VertexPositionColorTexture>();

            Vector2[] tangents = ComputeTangents(points);
            Vector2[] normals = ComputeStableNormals(tangents);

            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[count * 2];
            float time = Main.GlobalTimeWrappedHourly;//Main.GameUpdateCount/30.1f;




            const float BaseThickness = 8f;
            const float PulseAmplitude = -3f;
            float PulseFrequency = 6f * points.Count/100;
            const float PulseSpeed = 0.8f;


            for (int i = 0; i < count; i++)
            {
                float v = i / (float)(count - 1);

                float pulse = MathF.Sin(v * PulseFrequency * MathHelper.TwoPi
                       - time * PulseSpeed * MathHelper.TwoPi);

                pulse = MathF.Pow(MathF.Abs(pulse), 1.5f) * MathF.Sign(pulse);

                thickness = BaseThickness + pulse * PulseAmplitude;
                float halfWidth = thickness * 0.5f;

                
                float pulse01 = MathHelper.Clamp((pulse + 1f) * 0.5f, 0f, 1f);

                float whiteFactor = 1f - pulse01;
                Color baseColor = Color.Yellow;
                color = Color.Lerp(baseColor, Color.White, whiteFactor*0.7f);
                Vector2 n = normals[i];
                Vector2 p = points[i];


                Vector2 left = p + n * halfWidth;
                Vector2 right = p - n * halfWidth;

                verts[i * 2] = new VertexPositionColorTexture(
                    new Vector3(left - Main.screenPosition, 0f),
                    color,
                    new Vector2(0f, v)
                );

                verts[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(right - Main.screenPosition, 0f),
                    color,
                    new Vector2(1f, v)
                );
            }

            return verts;
        }

        public VertexPositionColorTexture[] verts;
        short[] indices;
        BasicEffect effect;
        private void DrawCoreSpineCustom(List<Vector2> smoothPoints, float thickness, Color color)
        {
            if (smoothPoints.Count < 2)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            verts = BuildStripVerticesStable(smoothPoints, thickness, color);

            indices = BuildStripIndices(smoothPoints.Count);

            if (effect == null)
            {
                effect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = false,
                    LightingEnabled = false,
                    World = Matrix.Identity,
                    View = Main.GameViewMatrix.ZoomMatrix,
                    Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1000f, 1000f)
                };
            }
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1);
            effect.View = Main.GameViewMatrix.ZoomMatrix;
            effect.World = Matrix.Identity;
            gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid   };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    verts,
                    0,
                    verts.Length,
                    indices,
                    0,
                    indices.Length - 2
                );
            }
        }


        #endregion
        private int TotalPoints => (Segments?.Length ?? 0) + 1; // head + tail array

        private Vector2 GetPointByIndex(int pointIndex)
        {
            if (pointIndex <= 0)
                return Projectile.Center;

            int segIndex = pointIndex - 1;
            if (Segments == null || segIndex < 0 || segIndex >= Segments.Length)
                return Projectile.Center;

            return Segments[segIndex];
        }

        private void SetPointByIndex(int pointIndex, Vector2 value)
        {
            if (pointIndex <= 0)
            {
                Projectile.Center = value;
                return;
            }

            int segIndex = pointIndex - 1;
            if (Segments == null || segIndex < 0 || segIndex >= Segments.Length)
                return;

            Segments[segIndex] = value;
        }

        private const float JoinSearchRadius = 160f;

        private void TryAutoJoinNearbyWorm()
        {
            if (Segments == null || Join.IsJoined)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.whoAmI == Projectile.whoAmI)
                    continue;

                if (proj.owner != Projectile.owner)
                    continue;

                if (proj.ModProjectile is not NeuronWormThrall other)
                    continue;

                if (other.Segments == null || other.Join.IsJoined)
                    continue;
                if (other.HasFollower)
                    continue;
                // Only join if we're close to THEIR butt 
                Vector2 otherButt = other.GetPointByIndex(other.Segments.Length);
                if (Vector2.Distance(Projectile.Center, otherButt) > JoinSearchRadius)
                    continue;

                // follower head -> leader butt
                Join = WormJoiner.CreateJoin(
                    leaderOwnerPlayer: proj.owner,
                    leaderProjectileWhoAmI: proj.whoAmI,
                    leaderSegmentIndex: other.Segments.Length,
                    localSegmentIndex: 0,
                    headToTail: true
                );

                other.HasFollower = true;
                return;
            }
        }


        private void ApplyJoinConstraint()
        {
            if (!Join.IsValid() || Segments == null)
                return;

            int leaderWho = Join.LeaderProjectileWhoAmI;
            if (leaderWho < 0 || leaderWho >= Main.maxProjectiles)
            {
                Join = WormJoiner.Empty;
                return;
            }

            Projectile leaderProj = Main.projectile[leaderWho];
            if (!leaderProj.active || leaderProj.ModProjectile is not NeuronWormThrall leader || leader.Segments == null)
            {
                Join = WormJoiner.Empty;
                return;
            }

            // Validate indices in unified scheme
            if (Join.LeaderSegmentIndex < 0 || Join.LeaderSegmentIndex >= leader.TotalPoints ||
                Join.LocalSegmentIndex < 0 || Join.LocalSegmentIndex >= TotalPoints)
            {
                Join = WormJoiner.Empty;
                return;
            }

            Vector2 anchor = leader.GetPointByIndex(Join.LeaderSegmentIndex);

            // Pin OUR head to leader butt
            SetPointByIndex(Join.LocalSegmentIndex, anchor);

            Projectile.velocity = Vector2.Zero;
        }


    }

    public class PsychicShock_Proj : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.ChromaticBurstPath;
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.timeLeft = 60;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.ArmorPenetration = 3000;
            Projectile.Size = new Vector2(50);

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }


        public override void SetStaticDefaults()
        {

        }

        public override void AI()
        {
            Projectile.Size = new Vector2(50) * (1 - LumUtils.InverseLerp(0, 60, Projectile.timeLeft));
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.ChromaticBurst;

            Vector2 scale = new Vector2(1) * (1 -LumUtils.InverseLerp(0, 60, Projectile.timeLeft));
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Purple with { A = 0 }, 0, tex.Size() / 2, scale, 0);
            
            return false;
        }
    }
}
