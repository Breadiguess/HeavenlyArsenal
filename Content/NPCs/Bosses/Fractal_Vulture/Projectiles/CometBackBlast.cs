using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles
{
    internal class CometBackBlast : ModProjectile
    {
        public NPC Owner;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;


        public override void OnSpawn(IEntitySource source)
        {
            Projectile.Center = Projectile.Center + new Vector2(0, 36).RotatedBy(Projectile.velocity.ToRotation());
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(14);
            Projectile.velocity *= 0;
            
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
        }
        public override void SetDefaults()
        {
            Projectile.scale = 0;
            Projectile.timeLeft = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.Size = new Vector2(10, 20);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.scale = LumUtils.InverseLerp(0, 10, Time);
            Time++;
        }

        #region cone
        BasicEffect Cone;
        public List<VertexPositionColorTexture> ConeVerts;
        void DrawCone()
        {
            if (ConeVerts.Count < 3)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            if(Cone == null)
            Cone = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
            Cone.World = Matrix.Identity;
            Cone.View = Main.GameViewMatrix.ZoomMatrix;

            Cone.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000f, 1000f);

            foreach (var pass in Cone.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    ConeVerts.ToArray(),
                    0,
                    ConeVerts.Count - 2
                );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="Center">converts to world position, so all you need to do is place this in the spot you want your cone to originate from</param>
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
            Vector2 dir = rotation.ToRotationVector2();

            // Create arc segment points
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                float ang = MathHelper.Lerp(-halfAngle, halfAngle, t);
                Vector2 edgeDir = dir.RotatedBy(ang);

                Vector2 p = Center + edgeDir * length;

                float radiusFade = 1f;
                float edgeFade = 0f;
                float sideFade = MathF.Cos((Math.Abs(ang) / halfAngle) * MathHelper.PiOver2);

                // combine fades:
                float apexAlpha = radiusFade * sideFade;
                float edgeAlpha = edgeFade * sideFade;

                Color apexColor = color * apexAlpha;
                Color edgeColor = color * edgeAlpha;

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(Center, 0f),
                    apexColor,
                    new Vector2(0f, 0f)
                ));

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(p, 0f),
                    edgeColor,
                    new Vector2(t, 1f)
                ));
            }
        }
        #endregion 
        private void prepCone()
        {
            if (ConeVerts == null)
                ConeVerts = new();
            BuildCone(ConeVerts, Projectile.Center, Projectile.rotation, MathHelper.ToRadians(76)*Projectile.scale, 800 * Projectile.scale, 8, Color.White);
            DrawCone();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            prepCone();
            return base.PreDraw(ref lightColor);
        }


        public override bool? CanDamage()
        {
            return base.CanDamage();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, 800 * Projectile.scale, Projectile.rotation, MathHelper.ToRadians(70) * Projectile.scale);
        }
    }
}
