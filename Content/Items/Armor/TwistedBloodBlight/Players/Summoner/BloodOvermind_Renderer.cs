using CalRemix.UI.ElementalSystem;
using HeavenlyArsenal.Common.utils;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System.Collections.Generic;
using System.Linq;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    public partial class BloodOvermind
    {
        public static RenderTarget2D OvermindTarget { get; set; }
        public override void Load()
        {
            On_Main.CheckMonoliths += RenderOvermindToTarget;
        }

        private void RenderOvermindToTarget(On_Main.orig_CheckMonoliths orig)
        {
            if (OvermindTarget == null || OvermindTarget.IsDisposed)
                OvermindTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            else if (OvermindTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
            {
                Main.QueueMainThreadAction(() =>
                {
                    OvermindTarget.Dispose();
                    OvermindTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(OvermindTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Projectile projectile in Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<BloodOvermind>()))
            {

                DrawOvermind(projectile.ModProjectile as BloodOvermind);

            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();
        }

        static void DrawOvermind(BloodOvermind proj)
        {
            mask(proj, Main.spriteBatch, Color.White);

          
        }











        #region the same primitive code copied over again
        public BasicEffect OvermindFace;
        float currentYaw;
        float currentPitch;
        public VertexPositionColorTexture[] maskVerts;
        short[] maskIndices;
        

       
        void RebuildMaskMesh(Color drawColor, int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f)
        {
            List<VertexPositionColorTexture> verts = new();
            List<short> indices = new();

            // Create concentric rings (center to rim)
            for (int ring = 0; ring <= ringSegments; ring++)
            {
                float r = ring / (float)ringSegments; 
                float z = -curvature * (r * r);       

                for (int seg = 0; seg <= radialSegments; seg++)
                {
                    float theta = MathHelper.TwoPi * (seg / (float)radialSegments);
                    float x = MathF.Cos(theta) * r * 0.4f;
                    float y = MathF.Sin(theta) * r * 0.54f;

                    Vector2 uv = new((x + 0.5f) / 1f, (y + 0.5f) / 1f);
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, y, z), drawColor * Projectile.Opacity, uv));
                }
            }

            int stride = radialSegments + 1;
            for (int ring = 0; ring < ringSegments; ring++)
            {
                for (int seg = 0; seg < radialSegments; seg++)
                {
                    int i0 = ring * stride + seg;
                    int i1 = i0 + 1;
                    int i2 = i0 + stride;
                    int i3 = i2 + 1;

                    indices.Add((short)i0);
                    indices.Add((short)i1);
                    indices.Add((short)i2);

                    indices.Add((short)i1);
                    indices.Add((short)i3); 
                    indices.Add((short)i2);
                }
            }

            maskVerts = verts.ToArray();
            maskIndices = indices.ToArray();
        }


        static float AngleTowards(float current, float target, float maxStep)
        {
            float delta = MathHelper.WrapAngle(target - current);
            if (delta > maxStep) delta = maxStep;
            if (delta < -maxStep) delta = -maxStep;
            return current + delta;
        }

        void UpdateFaceAim(Vector2 toPlayer, int spriteDir, float yawMaxDeg = 28f, float pitchMaxDeg = 20f, float degPerSec = 180f)
        {

            // Map screen delta to “desire” in [-1,1] then to limited angles
            float nx = MathHelper.Clamp(toPlayer.X / 180f, -1f, 1f);
            float ny = MathHelper.Clamp(toPlayer.Y / 180f, -1f, 1f);

            float yawMax = MathHelper.ToRadians(yawMaxDeg);
            float pitchMax = MathHelper.ToRadians(pitchMaxDeg);

            float targetYaw = (nx * yawMax) * spriteDir;
            float targetPitch = ny * pitchMax;

            float maxStep = MathHelper.ToRadians(degPerSec) * (1f / 60f);
            currentYaw = AngleTowards(currentYaw, targetYaw, maxStep);
            currentPitch = AngleTowards(currentPitch, targetPitch, maxStep);

            // Clamp final angles just in case
            currentYaw = MathHelper.Clamp(currentYaw, -yawMax, yawMax);
            currentPitch = MathHelper.Clamp(currentPitch, -pitchMax, pitchMax);

            //Main.NewText("pitch: "+currentPitch);
            //Main.NewText("Yaw: "+currentYaw);

        }

        static void mask(BloodOvermind mind, SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                   DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);



            mind.RebuildMaskMesh(Color.White, 22, 4, -1.2f);
            //RebuildMaskMesh_ColorDebug(20, 12, -0.2f);
            Vector2 toPlayer = mind.OvermindHeadPos.DirectionTo(mind.Owner.Center) * 10;
            toPlayer.X *= -1;
            //mind.UpdateFaceAim(toPlayer, mind.Projectile.spriteDirection, yawMaxDeg: 38f, pitchMaxDeg: 28f, degPerSec: 160f);


            Vector2 anchor = Vector2.Zero;
            anchor = mind.OvermindHeadPos - Main.screenPosition + new Vector2(-mind.currentYaw, mind.currentPitch) * 27.5f * 10;


            Texture2D face = ModContent.Request<Texture2D>(mind.GetPath().ToString() + "_Face").Value;




            Matrix a = 
                Matrix.CreateScale(30) *
                Matrix.CreateRotationY(mind.currentYaw * 4) *
                Matrix.CreateRotationX(mind.currentPitch * 5) *
                Matrix.CreateTranslation(anchor.X, anchor.Y, 0f);
            HAUtils.BasicEffectRenderer.DrawTexturedMesh(Main.graphics.GraphicsDevice, ref mind.OvermindFace, face, mind.maskVerts, mind.maskIndices, a);
            //DrawMask(mind,anchor, face, pixelSize: 30f);
        }

        #endregion













        public override bool PreDraw(ref Color lightColor)
        {

          
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Summoner/OvermindFace").Value;


            Texture2D Mushroom = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Summoner/OvermindTemporaryMushroom").Value;

            var mind = this;
            Main.EntitySpriteDraw(Mushroom, mind.Projectile.Center - Main.screenPosition + new Vector2(0, -45), null, Color.White, 0, Mushroom.Size() / 2, 0.4f, 0);

            if (mind.limb != null)
            {
                for (int i = 0; i < mind.limb.Skeleton.PositionCount - 1; i++)
                {
                    Utils.DrawLine(Main.spriteBatch, mind.limb.Skeleton.Position(i), mind.limb.Skeleton.Position(i + 1), Color.White,new Color(130, 70,72), 4);
                }
            }


            if (mind.Segments != null)
            {
                for (int i = 0; i < mind.Segments.Count; i++)
                {
                    for (int x = 0; x < mind.Segments[i].Count - 1; x++)
                    {

                        Color color = Color.Lerp(new Color(130, 70, 72), new Color(120, 92, 93), x / ((float)mind.Segments[i].Count - 1f));
                        Utils.DrawLine(Main.spriteBatch, mind.Segments[i][x].Position, mind.Segments[i][x + 1].Position, color, color, 3);
                    }
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);


            Main.EntitySpriteDraw(OvermindTarget, Projectile.Center - Main.screenPosition , null, Color.White, Projectile.rotation, OvermindTarget.Size() / 2, 1, 0);
            Main.spriteBatch.ResetToDefault();

            return false;
        }
    }
}
