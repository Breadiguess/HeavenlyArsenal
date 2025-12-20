using CalamityMod.Projectiles;
using HeavenlyArsenal.Common.IK;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    public class LeechScarf_TendrilProjectile : ModProjectile
    {
        #region tendrilStruct
        public struct Tendril
        {
            public IKSkeleton IKSkeleton;

            public Vector2 EndPos;

            public Vector2 DesiredPos;


            public Tendril(IKSkeleton skeleton)
            {
                this.IKSkeleton = skeleton;

            }

            public static void UpdateTendril(ref Tendril tendril, Vector2 StartPos, Vector2 endpos, float LerpInterp = 0.2f)
            {
                tendril.DesiredPos = endpos;
                tendril.EndPos = Vector2.Lerp(tendril.EndPos, tendril.DesiredPos, LerpInterp);

                tendril.IKSkeleton.Update(StartPos, tendril.DesiredPos);


            }
        }
        #endregion
        public Tendril tendril;
        public const int MAX_HITS = 3;
        public int HitsLeft
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public int Slot
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
          
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
        }

        public ref Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void SetDefaults()
        {
            int jointCount = 8;
            int totalLength = 120;
            int minPerJoint = 120/8;

            int[] lengths = RandomJointLengths(
                jointCount,
                totalLength,
                minPerJoint,
                Main.rand
            );

            var segments = new (float, IKSkeleton.Constraints)[jointCount];
            for (int i = 0; i < jointCount; i++)
            {
                segments[i] = (lengths[i], new IKSkeleton.Constraints());
            }

            tendril.IKSkeleton = new IKSkeleton(segments);
            HitsLeft = MAX_HITS;
            Projectile.Size = new Vector2(20, 20);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 50;
            Projectile.penetrate = -1;
        }
        /// <summary>
        /// helper method to create an amount of joints with differing lengths
        /// </summary>
        /// <param name="jointCount"></param>
        /// <param name="total"></param>
        /// <param name="minPerJoint"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int[] RandomJointLengths(int jointCount, int total, int minPerJoint, UnifiedRandom rand)
        {
            if (jointCount <= 0)
                throw new ArgumentException("jointCount must be > 0");

            int minTotal = jointCount * minPerJoint;
            if (minTotal > total)
                throw new ArgumentException("minPerJoint * jointCount exceeds total");

            int remaining = total - minTotal;

            // Generate cut points
            int[] cuts = new int[jointCount + 1];
            cuts[0] = 0;
            cuts[^1] = remaining;

            for (int i = 1; i < jointCount; i++)
                cuts[i] = rand.Next(remaining + 1);

            Array.Sort(cuts);

            int[] lengths = new int[jointCount];
            for (int i = 0; i < jointCount; i++)
                lengths[i] = minPerJoint + (cuts[i + 1] - cuts[i]);

            return lengths;
        }

        public override void OnSpawn(IEntitySource source)
        {

            int jointCount = 8;
            int totalLength = 120;
            int minPerJoint = 120/8;

            int[] lengths = RandomJointLengths(
                jointCount,
                totalLength,
                minPerJoint,
                Main.rand
            );

            var segments = new (float, IKSkeleton.Constraints)[jointCount];
            for (int i = 0; i < jointCount; i++)
            {
                segments[i] = (lengths[i], new IKSkeleton.Constraints());
            }

            tendril.IKSkeleton = new IKSkeleton(segments);
            HitsLeft = MAX_HITS;
        }
        float WiggleStrength => MathHelper.Clamp(HitsLeft / (float)MAX_HITS, 0f, 1f);

        public Vector2 Offset
        {
            get
            {
                float wiggle = WiggleStrength;

                return new Vector2(
                    -Owner.direction * (70 + 30 * MathF.Sin(Time / 10.1f + Slot * 10) * wiggle),
                    (Slot * -43 + (10 * (MathF.Cos(Time / 10.1f) + 1) + 40) * wiggle)
                ) + Owner.velocity;
            }
        }

        public override void AI()
        {
            Vector2 idlePos = Owner.Center + Offset;

            // Gentle seeking
            Vector2 seek = ComputeSeekOffset(18f) * WiggleStrength;

            Vector2 desiredPos = idlePos + seek;

            Projectile.Center = Vector2.Lerp(
                Projectile.Center,
                desiredPos,
                0.5f
            );

            if (HitsLeft <= 0)
                Projectile.damage = Projectile.originalDamage * 10;

            Time++;
        }


        NPC FindTargetBehindPlayer(float maxRange)
        {
            NPC best = null;
            float bestScore = float.MaxValue;

            Vector2 root = Owner.Center;
            int dir = Owner.direction;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(this))
                    continue;

                Vector2 toNPC = npc.Center - root;

                // Must be behind the player
                if (toNPC.X * dir > 0f)
                    continue;

                float dist = toNPC.Length();
                if (dist > maxRange)
                    continue;

                // Prefer closer + more centered targets
                float lateral = Math.Abs(toNPC.Y);
                float score = dist + lateral * 0.5f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = npc;
                }
            }

            return best;
        }

        Vector2 ComputeSeekOffset(float maxStrength)
        {
            NPC target = FindTargetBehindPlayer(220f);
            if (target == null)
                return Vector2.Zero;

            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();

            if (dist < 1f)
                return Vector2.Zero;

            // Normalize and scale gently
            Vector2 dir = toTarget / dist;

            // Stronger when closer, weaker when far
            float strength = MathHelper.Clamp(1f - dist / 220f, 0f, 1f);

            return dir * maxStrength * strength;
        }

        public override void PostAI()
        {
            CheckConditions();
            Projectile.timeLeft++;


            //Tendril.UpdateTendril(ref tendril, Owner.Center + new Vector2(14 * Owner.direction, -8), Projectile.Center, 0.8f);


            Vector2 root = Owner.Center - Main.OffsetsPlayerHeadgear[Owner.bodyFrame.Y / Owner.bodyFrame.Height] + new Vector2(-9 * Owner.direction, -2);


            //Dust.NewDustPerfect(root, DustID.Cloud, Vector2.Zero);
            Vector2 desired = Projectile.Center;


            NPC target = FindTargetBehindPlayer(220f);
            if (target != null)
            {
                Vector2 toTarget = target.Center - root;

                // Clamp reach so it doesn’t overextend
                float maxReach = tendril.IKSkeleton._maxDistance * 0.9f;
                if (toTarget.Length() > maxReach)
                    toTarget = toTarget.SafeNormalize(Vector2.Zero) * maxReach;

                Vector2 targetPos = root + toTarget;

                // Blend between idle endpoint and target
                float seekStrength = 0.35f * WiggleStrength; // fades out as it hardens
                desired = Vector2.Lerp(desired, targetPos, seekStrength);
            }

            Tendril.UpdateTendril(ref tendril, root, desired, 0.8f);
            Projectile.Center = desired;
        }
     
        void CheckConditions()
        {
            if (Owner == null || Owner.dead || Owner.GetModPlayer<LeechScarfPlayer>().Active == false)
            {
                Projectile.active = false;
                return;
            }
        }


  
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Use the same root point that the tendril uses for motion/rendering.
            Vector2 root = Owner.Center + new Vector2(14 * Owner.direction, -8);

            // Line thickness used for hit detection (kept from previous code).
            float lineWidth = 30f;

            // Dummy float required by CheckAABBvLineCollision.
            float _ = 0f;

            // Cast a line from the player (tendril root) to the projectile center.
            if (Collision.CheckAABBvLineCollision(
                targetHitbox.Location.ToVector2(),
                targetHitbox.Size(),
                root,
                Projectile.Center,
                lineWidth,
                ref _
            ))
            {
                return true;
            }

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HitsLeft <= 0)
            {
                
                Owner.GetModPlayer<LeechScarfPlayer>().KillTendril(Slot);
            }

            int heal = (int)Math.Round(damageDone * 0.015f);
            {
                //stupid calamity balancing wtf
                if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(1f, heal))
                    CalamityGlobalProjectile.SpawnLifeStealProjectile(Projectile, Owner, heal, ProjectileID.VampireHeal, 0, 0.15f); //BalancingConstants.LifeStealRange, BalancingConstants.LifeStealAccessoryCooldownMultiplier);

            }
            /*
            if (vampiricTalisman && proj.CountsAsClass<RogueDamageClass>() && crit)
            {
                int heal = (int)Math.Round(damage * 0.015);
                if (heal > BalancingConstants.LifeStealCap)
                    heal = BalancingConstants.LifeStealCap;

                if (CalamityGlobalProjectile.CanSpawnLifeStealProjectile(1f, heal))
                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileID.VampireHeal, BalancingConstants.LifeStealRange, BalancingConstants.LifeStealAccessoryCooldownMultiplier);
            }*/
            HitsLeft--;
        }


        public override void Load()
        {
            On_Main.CheckMonoliths += DrawSword;
        }
        public static RenderTarget2D SwordTarget { get; set; }
        private void DrawSword(On_Main.orig_CheckMonoliths orig)
        {
            if (SwordTarget == null || SwordTarget.IsDisposed)
                SwordTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth/2, Main.screenHeight/2);
            else if (SwordTarget.Size() != new Vector2(Main.screenWidth/2, Main.screenHeight/2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    SwordTarget.Dispose();
                    SwordTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth/2, Main.screenHeight/2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(SwordTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Projectile projectile in Main.projectile.Where(n => n.active && n.ai[0] > 0 && n.type == ModContent.ProjectileType<LeechScarf_TendrilProjectile>()))
            {
                Drawtendril(projectile);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();

        }

        void Drawtendril(Projectile proj)
        {

            RenderTendrils(proj);
        }

        public override bool PreDraw(ref Color lightColor)
            {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);
            var  OutlineShader = ShaderManager.GetShader("HeavenlyArsenal.Outline");
            OutlineShader.SetTexture(SwordTarget, 0);
            OutlineShader.TrySetParameter("uSize", Vector2.One);
            OutlineShader.TrySetParameter("uColor", Color.White);
            OutlineShader.TrySetParameter("uThreshold", 10f);
            OutlineShader.Apply();
            Main.spriteBatch.Draw(
               SwordTarget,
               Owner.Center - Main.screenPosition,
               null,
               Color.White,
               Projectile.rotation,
               SwordTarget.Size() / 2f,
               2,
               0,
               0f
           );

            Main.spriteBatch.ResetToDefault();


            //Utils.DrawBorderString(Main.spriteBatch, Slot.ToString(), Projectile.Center - Main.screenPosition, Color.AntiqueWhite);
            return false;
        }

        public BasicEffect TendrilEffect;
        public List<VertexPositionColorTexture> _verts = new();
        public List<short> _indices = new();
        public List<Vector2> _control = new();
        public List<Vector2> _smooth = new();
        void BuildSmoothTendrilMesh(List<VertexPositionColorTexture> verts, List<short> indices, List<Vector2> spine, float baseThickness, Color color)
        {
            verts.Clear();
            indices.Clear();

            int count = spine.Count;
            if (count < 2)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 p = spine[i];

                Vector2 dir =
                    i < count - 1 ? spine[i + 1] - p :
                    p - spine[i - 1];

                if (dir.LengthSquared() < 0.0001f)
                    dir = Vector2.UnitX;

                dir.Normalize();
                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float t = i / (float)(count - 1);
                float thickness =  Math.Clamp(baseThickness * t * 1.4f + 3f ,0, baseThickness);

                Vector2 left = p + normal * thickness;
                Vector2 right = p - normal * thickness;



                color = Lighting.GetColor((p).ToTileCoordinates());

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(left - Main.screenPosition, 0f),
                    color,
                    new Vector2(0f, t)
                ));

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(right - Main.screenPosition, 0f),
                    color,
                    new Vector2(1, t)
                ));
            }

            for (int i = 0; i < count - 1; i++)
            {
                short i0 = (short)(i * 2);
                short i1 = (short)(i * 2 + 1);
                short i2 = (short)(i * 2 + 2);
                short i3 = (short)(i * 2 + 3);

                indices.Add(i0); indices.Add(i2); indices.Add(i1);
                indices.Add(i1); indices.Add(i2); indices.Add(i3);
            }
        }

        Vector2 GetRenderPos(Projectile proj,int index)
        {
            Vector2 root = proj.As<LeechScarf_TendrilProjectile>().Owner.Center;
            Vector2 p = proj.As<LeechScarf_TendrilProjectile>().tendril.IKSkeleton.Position(index);

            int dir = proj.As<LeechScarf_TendrilProjectile>().Owner.direction;
            float frontness = (p.X - root.X) * dir;

            if (frontness > 0f)
                p.X -= 2f * frontness * dir;

            return p;
        }

        void SampleSmoothSpine(List<Vector2> control, List<Vector2> sampled, int samplesPerSegment)
        {
            sampled.Clear();

            if (control.Count < 2)
                return;

            for (int i = 0; i < control.Count - 1; i++)
            {
                Vector2 p0 = i > 0 ? control[i - 1] : control[i];
                Vector2 p1 = control[i];
                Vector2 p2 = control[i + 1];
                Vector2 p3 = (i + 2 < control.Count) ? control[i + 2] : p2;

                for (int j = 0; j < samplesPerSegment; j++)
                {
                    float t = j / (float)samplesPerSegment;
                    sampled.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                }
            }

            // Ensure the tip is included
            sampled.Add(control[^1]);
        }
        void GetSkeletonPoints(Projectile proj ,List<Vector2> points)
        {
            points.Clear();
            int count = proj.As<LeechScarf_TendrilProjectile>().tendril.IKSkeleton.PositionCount;
            for (int i = 0; i < count; i++)
                points.Add(GetRenderPos(proj,i));
        }

        void RenderTendrils(Projectile proj)
        {
            Color boneColor = Color.White;
                //Color.Lerp(Color.Crimson, Color.AntiqueWhite, proj.As<LeechScarf_TendrilProjectile>().HitsLeft / (float)LeechScarf_TendrilProjectile.MAX_HITS) ;
            GetSkeletonPoints(proj, _control);
            SampleSmoothSpine(_control, _smooth, samplesPerSegment: 12);

            BuildSmoothTendrilMesh(_verts, _indices, _smooth, 6f, boneColor);

            if (_indices.Count == 0)
                return;

            var gd = Main.graphics.GraphicsDevice;

            TendrilEffect ??= new BasicEffect(gd)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                LightingEnabled = false,
                DiffuseColor = Vector3.One
            };

            TendrilEffect.Texture = ModContent.Request<Texture2D>($"{Mod.Name}/Content/Items/Accessories/BloodyLeechScarf/LeechTendril").Value;// GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
            TendrilEffect.View = Matrix.Identity;//Main.GameViewMatrix.ZoomMatrix;

            TendrilEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0,
                Main.screenWidth,
                Main.screenHeight,
                0,
                -1000f,
                1000f
            );

            TendrilEffect.World = Matrix.Identity;

            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.SamplerStates[0] = SamplerState.PointClamp;

            foreach (var pass in TendrilEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _verts.ToArray(),
                    0,
                    _verts.Count,
                    _indices.ToArray(),
                    0,
                    _indices.Count / 3
                );
            }
        }

    }
}
