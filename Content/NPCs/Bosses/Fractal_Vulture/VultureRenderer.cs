using HeavenlyArsenal.Common.IK;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    public partial class voidVulture
    {
        #region neck
        static VertexPositionColorTexture[] neckVerts = new VertexPositionColorTexture[256];
        static int neckVertCount = 0;
        BasicEffect neckEffect;
        void BuildNeckStripBezier(IKSkeleton skeleton, float startWidth, float midWidth, float endWidth, int smoothSegments, Color drawColor)
        {
            float WidthAt(float t)
            {
                if (t < 0.5f)
                {
                    // Lerp from start to mid
                    float nt = t / 0.5f;
                    return MathHelper.Lerp(startWidth, midWidth, nt);
                }
                else
                {
                    // Lerp from mid to end
                    float nt = (t - 0.5f) / 0.5f;
                    return MathHelper.Lerp(midWidth, endWidth, nt);
                }
            }

            Vector2 P0 = skeleton.Position(0);
            Vector2 P1 = skeleton.Position(1);
            Vector2 P2 = skeleton.Position(2);

            // Control points to curve the neck smoothly
            Vector2 C0 = P0 + (P1 - P0) * 0.5f;
            Vector2 C1 = P2 + (P1 - P2) * 0.5f;

            // Total samples including endpoints
            int count = smoothSegments + 1;
            neckVertCount = count * 2;

            if (neckVertCount > neckVerts.Length)
                Array.Resize(ref neckVerts, neckVertCount * 2);

            int idx = 0;

            Vector2 Bezier(float t)
            {
                float u = 1f - t;
                return
                    u * u * u * P0 +
                    3f * u * u * t * C0 +
                    3f * u * t * t * C1 +
                    t * t * t * P2;
            }

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);

                Vector2 p = Bezier(t) - Main.screenPosition;

                // Compute tangent (derivative of cubic Bézier)
                float u = 1f - t;
                Vector2 tangent =
                    3f * u * u * (C0 - P0) +
                    6f * u * t * (C1 - C0) +
                    3f * t * t * (P2 - C1);

                if (tangent.LengthSquared() <= 1e-6f)
                    tangent = Vector2.UnitY;
                tangent.Normalize();

                Vector2 perp = tangent.RotatedBy(MathHelper.PiOver2);


                float radiusMult = Math.Clamp(1f - MathF.Sin(t * MathF.PI), 1, 3);
                float localRadius = WidthAt(t);

                Vector2 left = p + perp * localRadius;
                Vector2 right = p - perp * localRadius;
                // UV along strip
                float v = t;

                neckVerts[idx++] = new VertexPositionColorTexture(
                    new Vector3(left, 0),
                    drawColor,
                    new Vector2(0, v));

                neckVerts[idx++] = new VertexPositionColorTexture(
                    new Vector3(right, 0),
                    drawColor,
                    new Vector2(1, v));
            }
        }
        void DrawNeckPrimitive(GraphicsDevice gd)
        {

            if (neckEffect == null)
                neckEffect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true
                };

            neckEffect.World = Matrix.Identity;
            neckEffect.View = Main.GameViewMatrix.ZoomMatrix;
            neckEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000f, 1000f);

            neckEffect.Texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/kneckAndFace").Value;
            foreach (var pass in neckEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    neckVerts,
                    0,
                    neckVertCount - 2
                );
            }
        }

        void RenderNeck(Color DrawColor)
        {

            if (Main.netMode == NetmodeID.Server)
                return;
            if (Neck == null || NeckStrands == null)
                return;
            var gd = Main.instance.GraphicsDevice;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
            BuildNeckStripBezier(Neck2.Skeleton, startWidth: 20, midWidth: 20, endWidth: 20, smoothSegments: 6, drawColor: DrawColor * NPC.Opacity);
            DrawNeckPrimitive(gd);
            //DrawArm(ref Neck2, DrawColor * NPC.Opacity, 0);
            Main.spriteBatch.ResetToDefault();
            //DrawArm(ref Neck2, DrawColor, 0);

            return;

            for (int e = 0; e < NeckStrands.Count; e++)
            {
                if (NeckStrands[e] == null)
                    continue;

                for (int i = 0; i < NeckStrands[e].segments.Length - 1; i++)
                {
                    Color a = Color.Lerp(Color.Purple, Color.Black, i / (float)Neck.segments.Length);
                    Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                    // Horizontal thickness (X) tapers from baseWidth to tipWidth
                    float width = 0.35f;

                    // Vertical stretch based on actual distance to next segment and texture height
                    float segmentDistance = NeckStrands[e].segments[i].position.Distance(NeckStrands[e].segments[i + 1].position);
                    float rot = NeckStrands[e].segments[i].position.AngleTo(NeckStrands[e].segments[i + 1].position);
                    float lengthFactor = 1.4f;
                    lengthFactor = (segmentDistance / 1);

                    Vector2 stretch = new Vector2(width, lengthFactor) * 1.6f;
                    Vector2 DrawPos = NeckStrands[e].segments[i].position - Main.screenPosition;

                    Main.EntitySpriteDraw(debug, DrawPos, null, a * NPC.Opacity, rot + MathHelper.PiOver2, debug.Size() / 2, stretch, 0);
                    //if (i == 0)
                    //    thing();
                }
            }
            for (int i = 0; i < Neck.segments.Length; i++)
            {
                Color a = Color.Lerp(Color.AntiqueWhite.MultiplyRGB(DrawColor), Color.Black, i / (float)Neck.segments.Length);
                Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
                Vector2 DrawPos = Neck.segments[i].position - Main.screenPosition;

                //Main.EntitySpriteDraw(debug, DrawPos, null, a, 0, debug.Size() / 2, 31, 0);
            }
        }
        #endregion
        #region head
        BasicEffect VultureMask;
        float currentYaw;
        float currentPitch;
        VertexPositionColorTexture[] maskVerts;
        short[] maskIndices;
        void RenderHead()
        {
            Vector2 DrawPos = HeadPos - Main.screenPosition;

            Texture2D Placeholder = AssetDirectory.Textures.BigGlowball.Value;
            //Main.EntitySpriteDraw(Placeholder, DrawPos, null, Color.AntiqueWhite * NPC.Opacity, 0, Placeholder.Size() / 2, 0.1f, 0);
        }

        void DrawMask(Vector2 anchorScreenPos, Texture2D faceTex, float pixelSize = 96f)
        {
            var gd = Main.graphics.GraphicsDevice;
            if (VultureMask == null)
                VultureMask = new BasicEffect(gd)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true
                };


            VultureMask.Texture = faceTex;
            VultureMask.View = Main.GameViewMatrix.ZoomMatrix;
            VultureMask.Projection = Matrix.CreateOrthographicOffCenter(
                                        0, Main.screenWidth,
                                        Main.screenHeight, 0,
                                        -1000f, 1000f);

            // Scale -> Yaw -> Pitch -> Translate

            VultureMask.World =
                Matrix.CreateScale(pixelSize) *
                Matrix.CreateRotationY(currentYaw) *
                Matrix.CreateRotationX(currentPitch) *

                Matrix.CreateTranslation(anchorScreenPos.X, anchorScreenPos.Y, 0f);

            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullClockwise;
            gd.SamplerStates[0] = SamplerState.PointClamp;

            foreach (var pass in VultureMask.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    maskVerts, 0, maskVerts.Length,
                    maskIndices, 0, maskIndices.Length / 3);
            }
        }
        void RebuildMaskMesh(Color drawColor, int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f)
        {
            List<VertexPositionColorTexture> verts = new();
            List<short> indices = new();

            // Create concentric rings (center to rim)
            for (int ring = 0; ring <= ringSegments; ring++)
            {
                float r = ring / (float)ringSegments; // 0→1
                float z = -curvature * (r * r);       // gentle downward bend (−Z toward camera)

                for (int seg = 0; seg <= radialSegments; seg++)
                {
                    float theta = MathHelper.TwoPi * (seg / (float)radialSegments);
                    float x = MathF.Cos(theta) * r * 0.5f;
                    float y = MathF.Sin(theta) * r * 0.5f;

                    Vector2 uv = new((x + 0.5f) / 1f, (y + 0.5f) / 1f);
                    verts.Add(new VertexPositionColorTexture(new Vector3(x, y, z), drawColor * NPC.Opacity, uv));
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

            // Flip yaw with facing so “left/right” tracks the sprite
            float targetYaw = (nx * yawMax) * spriteDir;
            float targetPitch = ny * pitchMax;

            // Step with a speed cap (≈60 FPS)
            float maxStep = MathHelper.ToRadians(degPerSec) * (1f / 60f);
            currentYaw = AngleTowards(currentYaw, targetYaw, maxStep);
            currentPitch = AngleTowards(currentPitch, targetPitch, maxStep);

            // Clamp final angles just in case
            currentYaw = MathHelper.Clamp(currentYaw, -yawMax, yawMax);
            currentPitch = MathHelper.Clamp(currentPitch, -pitchMax, pitchMax);



        }

        void mask(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                   DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);



            RebuildMaskMesh(Lighting.GetColor(HeadPos.ToTileCoordinates()) * NPC.Opacity, 27, 22, -1.2f);
            //RebuildMaskMesh_ColorDebug(20, 12, -0.2f);

            if (currentTarget != null)
            {
                Vector2 toPlayer = currentTarget.Center - NPC.Center;
                if (currentState == Behavior.VomitCone)
                {
                    toPlayer = NPC.Center.AngleTo(HeadPos).ToRotationVector2() * 140;
                }
                if (currentState != Behavior.Medusa)
                    UpdateFaceAim(HeadPos.DirectionFrom(NPC.Center) * 120, NPC.spriteDirection, yawMaxDeg: 60f, pitchMaxDeg: 60f, degPerSec: 160f);
                else
                    UpdateFaceAim(toPlayer, NPC.spriteDirection, yawMaxDeg: 28f, pitchMaxDeg: 18f, degPerSec: 160f);
            }
            //Main.NewText("pitch: " + currentPitch + " yaw: " + currentYaw);
            Vector2 anchor = Vector2.Zero;

            if (!NPC.IsABestiaryIconDummy)
                anchor = HeadPos - Main.screenPosition + new Vector2(-currentYaw, currentPitch) * 27.5f;


            Texture2D face = ModContent.Request<Texture2D>(this.GetPath().ToString() + "_Face").Value;

            DrawMask(anchor, face, pixelSize: 50f);
        }
        #endregion
        #region wing
        BasicEffect WingEffect;
        void RenderWings(SpriteBatch spriteBatch, voidVultureWing wing, int offsetID)
        {
            wing.EnsureBuffers();
            if (!wing.BuffersReady)
                return;


            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
            bool flipped = offsetID % 2 == 0;

            Vector2 drawCenter =
                 NPC.Center - Main.screenPosition - new Vector2(wingPos[offsetID].X, 0) + new Vector2(0, -60);

            wing.RegenerateVertices(Lighting.GetColor((NPC.Center + new Vector2(wingPos[offsetID].X, 0)).ToTileCoordinates()),
                drawCenter,
                -wing.WingRotation,
                flipped,
                NPC.Opacity
            );

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            gd.SetVertexBuffer(wing.WingVertexBuffer);
            gd.Indices = wing.WingIndexBuffer;

            Texture2D wingTex = ModContent.Request<Texture2D>(
                "HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/WingTexture"
            ).Value;

            if (WingEffect == null)
                WingEffect = new BasicEffect(gd)
                {

                    TextureEnabled = true,
                    Texture = wingTex,
                    VertexColorEnabled = true
                };

            WingEffect.World = Matrix.Identity;
            //gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            WingEffect.View = Main.GameViewMatrix.ZoomMatrix;
            WingEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000, 1000
            );

            foreach (EffectPass pass in WingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                gd.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    baseVertex: 0,
                    minVertexIndex: 0,
                    numVertices: voidVultureWing.WingSubdivisions * 2,
                    startIndex: 0,
                    primitiveCount: (voidVultureWing.WingSubdivisions - 1) * 2
                );

            }
        }


        void RenderWingsOld(SpriteBatch spriteBatch, voidVultureWing wing, int offsetID)
        {
            Texture2D wingtex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/WingTexture").Value;
            SpriteEffects flip = offsetID % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float offset = offsetID % 2 == 0 ? 0 : wingtex.Width;
            Vector2 Origin = new Vector2(offset, wingtex.Height);
            float rot = wing.WingRotation * (offsetID % 2 == 0 ? 1 : -1);
            Vector2 Scale = new Vector2(1.75f, 1) * 1f;
            Vector2 DrawPos = new Vector2(wingPos[offsetID].X, 0) + NPC.Center - Main.screenPosition;
            Main.EntitySpriteDraw(wingtex, DrawPos, null, Color.White * NPC.Opacity, rot, Origin, Scale, flip);
            //Utils.DrawBorderString(spriteBatch, wing.Time.ToString() + $"\n" + Math.Round(wing.WingFlapProgress%1, 3).ToString(), DrawPos, Color.AliceBlue); }
        }
        void RenderBody(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawColor *= NPC.Opacity;
            SpriteEffects a = NPC.direction != 0 ? (NPC.direction != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally) : 0;
            Texture2D body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/voidVulture_Body").Value;
            Texture2D bodyBehind = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/voidVulture_Body_Behind").Value;

            Main.EntitySpriteDraw(bodyBehind, NPC.Center + new Vector2(0, 50) - screenPos, null, drawColor, 0, bodyBehind.Size() / 2, 1f, a);
            DrawCore(screenPos);

            Main.EntitySpriteDraw(body, NPC.Center - screenPos, null, drawColor, 0, body.Size() / 2, 1, a);
            //Main.EntitySpriteDraw(body, NPC.Center + new Vector2(0, 40) - screenPos, null, Color.AntiqueWhite, MathHelper.ToRadians(90), body.Size() / 2, 2, 0);

        }
        #endregion
        void DrawCore(Vector2 screenPos)
        {
            if (CoreDeployed)
                return;

            Texture2D outline = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            Texture2D core = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/OtherworldlyCore_Anim").Value;
            Vector2 Offset = new Vector2(0, 30);

            Rectangle frame = core.Frame(1, 4, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 4);
            Vector2 DrawPos = NPC.Center - screenPos + Offset;

            Color GlowFlip = Color.Lerp(Color.Blue, Color.WhiteSmoke, Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly))) * 0.3f;
            Main.EntitySpriteDraw(core, DrawPos, frame, Color.White * NPC.Opacity, 0, frame.Size() / 2, 1, 0);
            Main.EntitySpriteDraw(outline, DrawPos, null, Color.White with { A = 0 } * NPC.Opacity, 0, outline.Size() / 2, 0.1f, 0);

            Main.EntitySpriteDraw(Glow, DrawPos, null, GlowFlip with { A = 0 } * NPC.Opacity, 0, Glow.Size() / 2, 1, 0);

            if (NPC.IsABestiaryIconDummy)
                return;
            float GravityWellInterpolant = LumUtils.InverseLerpBump(0, VomitCone_ShootStart, VomitCone_ShootStop, VomitCone_ShootEnd, Time);
            if (currentState != Behavior.VomitCone || !HasSecondPhaseTriggered)
                GravityWellInterpolant = 0;
            if (GravityWellInterpolant > 0)
            {
                Main.EntitySpriteDraw(outline, DrawPos, null, Color.White with { A = 0 }, 0, outline.Size() / 2, 4 * GravityWellInterpolant, 0);
                Main.EntitySpriteDraw(outline, DrawPos, null, Color.White with { A = 0 }, 0, outline.Size() / 2, 2 * GravityWellInterpolant, 0);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
          
          
            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            //RenderBody(spriteBatch, screenPos, drawColor);
            spriteBatch.ResetToDefault();
            DrawTailStrip(TailPos, TailLength, GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge, NPC.Opacity, i =>
            {
                float t = i / (float)(TailLength - 1);
                return MathHelper.Lerp(58f, 4f, t); // thick → thin
            });
            spriteBatch.ResetToDefault();
            drawTail();
            DrawLegs(drawColor);
            if (!NPC.IsABestiaryIconDummy)
                for (int i = 0; i < 2; i++)
                {
                    RenderWings(spriteBatch, wings[i], i);
                }

            spriteBatch.ResetToDefault();
            if (!NPC.IsABestiaryIconDummy)
                for (int i = 0; i < 2; i++)
                {
                    //RenderWingsOld(spriteBatch, wings[i], i);
                }
            RenderBody(spriteBatch, screenPos, drawColor);
            RenderNeck(drawColor);
            if (currentState == Behavior.Medusa)
            {
                Main.EntitySpriteDraw(debug, (NPC.Center + HeadPos) / 2 - screenPos, null, Color.Red, MathF.Sin(Time / 10.1f), debug.Size() / 2, 20, 0);
            }
            //RenderHead();
            mask(spriteBatch, drawColor);
            DateTime date = DateTime.Now;
            if (date.Month == 12 && (date.Day ==24 || date.Day == 25))
            {
                float hatScale = 0.1f;
                Vector2 hatDrawPosition = HeadPos - Main.screenPosition + new Vector2(0,-20);
                Texture2D santaHat = GennedAssets.Textures.NamelessDeity.SantaHat;

                Main.EntitySpriteDraw(santaHat, hatDrawPosition, null, Color.White * NPC.Opacity, 0f, santaHat.Size() * 0.5f, hatScale, 0);
            }

           
            string msg = "";
            msg += $"Time: {Time}\n Currenstate: {currentState.ToString()}\nprevious state: {previousState.ToString()}\n DashesUsed: {DashesUsed}\n Damage: {NPC.damage}\n dash timer: {DashTimer}\n second phase: {HasSecondPhaseTriggered}\n StaggerTimer: {StaggerTimer}\n Direction: {NPC.direction}";
            //Utils.DrawBorderString(spriteBatch, msg, NPC.Center - screenPos, Color.AliceBlue, anchory: -1);

            //Utils.DrawLine(spriteBatch, FlyAwayOffset + NPC.Center, NPC.Center, Color.Red);
            //Main.EntitySpriteDraw(debug, StoredSolynPos - screenPos, null, Color.Red, 0, debug.Size() / 2, 10, 0);
            //Main.EntitySpriteDraw(debug, SolynChosenShield - screenPos, null, Color.Green, 0, debug.Size() / 2, 10, 0);

            return false;// base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        #region tail primitive
        void DrawTailStrip(Vector2[] tail, int length, Texture2D texture, float opacity, Func<int, float> thicknessAt)
        {

            if (NPC.IsABestiaryIconDummy)
                return;
            if (tail == null || length < 2 || texture == null)
                return;

            List<VertexPositionColorTexture> verts = new(length * 2);

            float totalLength = 0f;
            float[] segLen = new float[length];

            // Precompute segment distances
            for (int i = 1; i < length; i++)
            {
                float d = Vector2.Distance(tail[i], tail[i - 1]);
                segLen[i] = d;
                totalLength += d;
            }

            float accumulated = 0f;

            for (int i = 0; i < length - 2; i++)
            {
                // Ignore invalid tail entries
                if (tail[i] == Vector2.Zero)
                    break; //NUH UH

                Vector2 current = tail[i];

                int iPrev = Math.Max(i - 1, 0);
                int iNext = Math.Min(i + 1, length - 1);

                Vector2 prev = tail[iPrev];
                Vector2 next = tail[iNext];

                // If the next or prev point is invalid, stop early
                if (prev == Vector2.Zero || next == Vector2.Zero)
                    break;

                Vector2 dir = (next - prev).SafeNormalize(Vector2.UnitY);
                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float thickness = thicknessAt(i);
                float halfWidth = thickness * 0.5f;

                // Build segment endpoint positions
                Vector2 left = current + normal * halfWidth - Main.screenPosition;
                Vector2 right = current - normal * halfWidth - Main.screenPosition;

                float v = accumulated / Math.Max(totalLength, 0.001f);

                Color c = Color.White with { A = 0 } * opacity;

                verts.Add(new VertexPositionColorTexture(new Vector3(left, 0f), c, new Vector2(0f, v)));
                verts.Add(new VertexPositionColorTexture(new Vector3(right, 0f), c, new Vector2(1f, v)));

                accumulated += segLen[i];
            }

            DrawTriangleStripClean(verts, texture);
        }
        BasicEffect TailThing;
        void DrawTriangleStripClean(List<VertexPositionColorTexture> verts, Texture2D tex)
        {
            if (verts.Count < 7)
                return;

            if (Main.netMode == NetmodeID.Server)
                return;
            GraphicsDevice gfx = Main.graphics.GraphicsDevice;
            if (TailThing == null)
                TailThing = new BasicEffect(gfx)
                {

                    VertexColorEnabled = true,
                    View = Matrix.Identity,
                    World = Matrix.Identity,

                };
            TailThing.TextureEnabled = true;
            TailThing.Texture = tex;
            TailThing.World = Matrix.Identity;
            TailThing.View = Main.GameViewMatrix.ZoomMatrix;
            TailThing.Projection = Matrix.CreateOrthographicOffCenter(
                                            0, Main.screenWidth,
                                            Main.screenHeight, 0, -1, 1);
            int primitiveCount = verts.Count - 2;

            if (primitiveCount <= 0)
                return;

            var vertArray = verts.ToArray();

            foreach (EffectPass pass in TailThing.CurrentTechnique.Passes)
            {
                pass.Apply();
                gfx.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    vertArray,
                    0,
                    primitiveCount
                );
            }
        }

        #endregion

        void drawTail()
        {
            if (TailPos != null)
            {
                Color t = Color.White * NPC.Opacity;
                for (int i = 0; i < TailLength - 2; i++)
                {
                    Utils.DrawLine(Main.spriteBatch, TailPos[i], TailPos[i + 1], t, t, 3f);

                }

            }
        }


        void DrawLegs(Color DrawColor)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 0; i < 2; i++)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin();

                BuildLegStripBezier(i, i % 3 == 0 ? _LeftLeg.Skeleton : _rightLeg.Skeleton, 15, 10, 10, 10, DrawColor * NPC.Opacity);

                var gd = Main.instance.GraphicsDevice;
                DrawLegsPrimitive(i, gd);
                Main.spriteBatch.ResetToDefault();
            }
        }
        #region leg primitive

        static List<VertexPositionColorTexture[]> LegVerticies;

        List<int> legsVertCount = new List<int>(2);
        List<BasicEffect> legEffects;
        void BuildLegStripBezier(int Index, IKSkeleton skeleton, float startWidth, float midWidth, float endWidth, int smoothSegments, Color drawColor)
        {


            float WidthAt(float t)
            {
                if (t < 0.5f)
                {
                    // Lerp from start to mid
                    float nt = t / 0.5f;
                    return MathHelper.Lerp(startWidth, midWidth, nt);
                }
                else
                {
                    // Lerp from mid to end
                    float nt = (t - 0.5f) / 0.5f;
                    return MathHelper.Lerp(midWidth, endWidth, nt);
                }
            }

            Vector2 P0 = skeleton.Position(0);
            Vector2 P1 = skeleton.Position(1);
            Vector2 P2 = skeleton.Position(2);
            Vector2 P3 = skeleton.Position(3);

            // Control points to curve the neck smoothly
            Vector2 C0 = P0 + (P1 - P0) * 0.5f;
            Vector2 C1 = P2 + (P1 - P2) * 0.5f;
            Vector2 C2 = P3 + (P2 - P3) * 0.5f;

            // Total samples including endpoints
            int count = smoothSegments + 1;

            var thing = LegVerticies[Index];
            var legCount = legsVertCount[Index];
            legCount = count * 2;
            if (legCount > thing.Length)
                Array.Resize(ref thing, legCount * 2);

            int idx = 0;


            Vector2 CubicBezier(float t)
            {
                float u = 1f - t;
                float tt = t * t;
                float uu = u * u;

                return uu * u * P0 +
                    3f * uu * t * P1 +
                    3f * u * tt * P2 +
                    tt * t * P3;
            }
            var vert = LegVerticies[Index];
            if (vert == null)
                vert = new VertexPositionColorTexture[256];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);

                Vector2 p = CubicBezier(t) - Main.screenPosition;

                // Compute tangent (derivative of cubic Bézier)
                float u = 1f - t;
                Vector2 tangent =
                    3f * u * u * (C0 - P0) +
                    6f * u * t * (C1 - C0) +
                    3f * t * t * (P2 - C1) +
                    3f * t * t * (P3 - C2);

                if (tangent.LengthSquared() <= 1e-6f)
                    tangent = Vector2.UnitY;
                tangent.Normalize();

                Vector2 perp = tangent.RotatedBy(MathHelper.PiOver2);


                float radiusMult = Math.Clamp(1f - MathF.Sin(t * MathF.PI), 1, 3);
                float localRadius = WidthAt(t);

                Vector2 left = p + perp * localRadius;
                Vector2 right = p - perp * localRadius;
                // UV along strip
                float v = t;

                vert[idx++] = new VertexPositionColorTexture(
                    new Vector3(left, 0),
                    drawColor,
                    new Vector2(0, v));

                vert[idx++] = new VertexPositionColorTexture(
                    new Vector3(right, 0),
                    drawColor,
                    new Vector2(1, v));
            }
            legsVertCount[Index] = legCount;
        }
        void DrawLegsPrimitive(int index, GraphicsDevice gd)
        {
            var effect = legEffects[index];

            if (effect == null)
                effect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true
                };

            effect.World = Matrix.Identity;
            effect.View = Main.GameViewMatrix.ZoomMatrix;
            effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000f, 1000f);
            effect.TextureEnabled = true;
            effect.Texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/voidVulture_Leg").Value;

            //neckEffect.Texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/kneckAndFace").Value;
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    LegVerticies[index],
                    0,
                    legsVertCount[index] - 2
                );
            }
            legEffects[index] = effect;
        }


        #endregion
    }

}
