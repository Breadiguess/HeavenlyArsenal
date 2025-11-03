using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    partial class newLeech : BloodmoonBaseNPC
    {
        VertexBuffer vertexBuffer;
 
        BasicEffect basicEffect;
       
      

        void apple(Color drawColor)
        {

            if (Main.netMode == NetmodeID.Server)
                return;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            if (basicEffect == null)
            {
                basicEffect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    Alpha = NPC.Opacity
   

                };
                
            }
            int segCount = AdjHitboxes.Length;
            int subdivisions = 5; // increase for smoother bends

            // Each segment will have (subdivisions + 1) points
            int totalPoints = (segCount - 1) * subdivisions + 1;
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[totalPoints * 2];

            int index = 0;


            // Correct head extension
            Vector2 firstDir = AdjHitboxes[0].Center() - AdjHitboxes[1].Center();
            if (firstDir.LengthSquared() < 0.0001f)
                firstDir = Vector2.UnitX;
            firstDir.Normalize();
            Vector2 preStart = AdjHitboxes[0].Center() + firstDir * AdjHitboxes[0].Width / 2f;

            // Correct tail extension
            Vector2 lastDir = AdjHitboxes[segCount - 1].Center() - AdjHitboxes[segCount - 2].Center();
            if (lastDir.LengthSquared() < 0.0001f)
                lastDir = Vector2.UnitX;
            lastDir.Normalize();
            Vector2 postEnd = AdjHitboxes[segCount - 1].Center() + lastDir * AdjHitboxes[segCount - 1].Width / 2f;

            float totalLength = 0f;
            for (int i = 0; i < segCount - 1; i++)
                totalLength += Vector2.Distance(AdjHitboxes[i].Center(), AdjHitboxes[i + 1].Center());

            float accumulatedLength = 0f;


            int frameCount = 5;
            float frameWidth = 1f / frameCount;

            for (int i = 0; i < segCount - 1; i++)
            {
                int frameIndex;
                if (i == 0)
                    frameIndex = 0;
                else if (i == segCount - 2)
                    frameIndex = 4; // tail1
                else if (i == segCount - 1)
                    frameIndex = 5; // tail2
                else
                    frameIndex = ((i - 1) % 2) + 1;

                float uStart = frameIndex * frameWidth;
                float uEnd = uStart + frameWidth;


                Vector2 curr = (i == 0) ? preStart : AdjHitboxes[i].Center();

                Vector2 next = (i == segCount - 2) ? postEnd : AdjHitboxes[i + 1].Center();


                for (int s = 0; s < (i == segCount - 2 ? subdivisions + 1 : subdivisions); s++)
                {

                    float segLength = Vector2.Distance(curr, next);

                    float t = s / (float)subdivisions;
                    Vector2 point = Vector2.Lerp(curr, next, t);

                    Vector2 dir = next - curr;
                    if (dir.LengthSquared() < 0.0001f)
                        dir = Vector2.UnitY;
                    dir.Normalize();

                    Vector2 normal = new Vector2(-dir.Y, dir.X);

                 
                    float u = MathHelper.Lerp(uStart, uEnd, t);

                    Color c = drawColor;
                    float ribbonScale = 1.4f;

                    float width = AdjHitboxes[i].Width * 0.5f * ribbonScale;
                    Vector2 left = point - normal * width - Main.screenPosition;
                    Vector2 right = point + normal * width - Main.screenPosition;

                    float v0 = 0.5f - 0.5f / ribbonScale;
                    float v1 = 0.5f + 0.5f / ribbonScale;

                    verts[index * 2] = new VertexPositionColorTexture(new Vector3(left, 0f), c, new Vector2(1-u, v1));
                    verts[index * 2 + 1] = new VertexPositionColorTexture(new Vector3(right, 0f), c, new Vector2(1-u, v0));

                    index++;
                    accumulatedLength += segLength / subdivisions;
                }
            }

            basicEffect.World = Matrix.Identity;
            basicEffect.View = Main.GameViewMatrix.ZoomMatrix;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                0, 1);    
            
            gd.RasterizerState = RasterizerState.CullNone;


            basicEffect.TextureEnabled = true;
            basicEffect.Texture = ModContent.Request<Texture2D>(RealTexture).Value; gd.SamplerStates[0] = SamplerState.PointClamp; // crisp pixel-perfect scaling


            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    verts, 0,
                    verts.Length - 2
                );
            }

        }
        


        #region Gores
        public static Asset<Texture2D>[] UmbralLeechGores
        {
            get;
            private set;
        }
        private void GetGoreInfo(out Texture2D texture, int SegmentInput,out int goreID)
        {
            texture = null;
            goreID = 0;
            if (Main.netMode != NetmodeID.Server)
            {
                int variant = SegmentInput;


                variant = (int)Math.Clamp((Utils.Remap(SegmentInput, 0, SegmentCount, 0, 6)),0,6);

                variant = Math.Abs(6-variant);
                texture = UmbralLeechGores[variant].Value;
               
                goreID = ModContent.Find<ModGore>(Mod.Name, $"UmbralLeechGore{variant + 1}").Type;
            }
        }
        private void createGore(Vector2 SpawnPos, int segment)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            //thanks lucille
            GetGoreInfo(out _, segment, out int goreID);

            Gore.NewGore(NPC.GetSource_FromThis(), SpawnPos, Vector2.Zero, goreID, NPC.scale);
        }
        #endregion
        #region Drawcode
        public string RealTexture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_" + variant;
        void RenderTails(Vector2 screenPosition, Color drawColor)
        {
            if (Tail == null)
                return;

            // ManageTail();
            Texture2D tailtex = AssetDirectory.Textures.UmbralLeechTendril.Value;

            for (int i = 0; i < Tail.Count; i++)
            {
                var _tailPosition = Tail[i].Item1;


                for (int x = 0; x < _tailPosition.Length - 1; x++)
                {

                    Vector2 DrawPos = _tailPosition[x] - screenPosition;
                    int style = 0;
                    if (x == _tailPosition.Length - 3)
                    {
                        style = 1;
                    }
                    if (x > _tailPosition.Length - 3)
                    {
                        style = 2;
                    }


                    Rectangle frame = tailtex.Frame(1, 3, 0, style);

                    float rotation = _tailPosition[x].AngleTo(_tailPosition[x + 1]);
                    Vector2 stretch = new Vector2(0.25f + Utils.GetLerpValue(0, _tailPosition.Length, x, true),
                        _tailPosition[x].Distance(_tailPosition[x + 1]) / (frame.Height - 5) * 1.2f
                    ) * 1.2f;

                    Main.EntitySpriteDraw(tailtex, DrawPos, frame, drawColor, rotation - MathHelper.PiOver2, frame.Size() * 0.5f, stretch, 0);
                    //Utils.DrawBorderString(Main.spriteBatch, i.ToString(),DrawPos,Color.AntiqueWhite, 0.3f);
                }
            }

        }
        void renderWhiskers(float Rot, Color drawColor, Vector2 AnchorPos)
        {
            if (WhiskerAnchors == null)
                return;
            /*
            WhiskerAnchors = new Vector2[]
            {
                new Vector2(16, 0),
                new Vector2(16, 14),
                new Vector2(5, 0),
                new Vector2(5, 14)
            };*/
            Texture2D tex = AssetDirectory.Textures.UmbralLeechWhisker.Value;
            Vector2 DrawPos = AdjHitboxes[0].Center() - Main.screenPosition;
            int a = 0;
            foreach (var i in WhiskerAnchors)
            {
                DrawPos = AnchorPos + i.RotatedBy(Rot) - new Vector2(10, -3).RotatedBy(Rot) - Main.screenPosition;
                Rectangle Frame = tex.Frame(1, 4, 0, a);

                Vector2 Origin = new Vector2(0, Frame.Height / 2);
                float Rotation = Rot + MathHelper.ToRadians(a * 2 + MathF.Sin(CosmeticTime / 10.1f + a * 10) * 30);//MathHelper.ToRadians(20 * accelerationInterp);
                Main.EntitySpriteDraw(tex, DrawPos, Frame, drawColor, Rotation, Origin, new Vector2(1), 0);


                a++;
            }


        }

        

        void renderLegs(int segment, float Rot, Color drawColor)
        {
            if (segment == 0 || segment == SegmentCount - 1)
                return;
            Texture2D leechLegs = AssetDirectory.Textures.UmbralLeech_Legs.Value;
            Vector2 DrawPos = AdjHitboxes[segment].Center() + new Vector2(0, 18).RotatedBy(Rot) - Main.screenPosition;
            Rectangle Frame = leechLegs.Frame(3, 2, 0, 1, 0);
            Vector2 Origin = new Vector2(Frame.Width / 2, 0);//Frame.Size() * 0.5f;

            float stridePhase = CosmeticTime / 10.1f - segment*2 * 0.4f;
            float stride = MathF.Sin(stridePhase);
            float pushBack = MathHelper.ToRadians(35 * accelerationInterp);
            float strideSwing = MathHelper.ToRadians(stride * 10 * (1f - 0.5f * accelerationInterp));

            float Rotation = Rot + pushBack + strideSwing;
            Vector2 Scale = new Vector2(1 - 1.2f * segment / (float)SegmentCount);
            if (Scale.Length() < 0.4f)
                return;
            Main.EntitySpriteDraw(leechLegs, DrawPos, Frame, drawColor, Rotation, Origin, Scale, 0);

            //Utils.DrawBorderString(Main.spriteBatch, Scale.Length().ToString(), DrawPos, Color.AntiqueWhite);
        }
        void RenderBackLegs(int segment, float Rot, Color drawColor)
        {
            if (segment == 0 || segment == SegmentCount - 1)
                return;
            Texture2D leechLegs = AssetDirectory.Textures.UmbralLeech_Legs.Value;//ModContent.Request<Texture2D>(RealTexture + "_Legs").Value;
            Vector2 DrawPos = AdjHitboxes[segment].Center() + new Vector2(-10, 18).RotatedBy(Rot) - Main.screenPosition;
            Rectangle Frame = leechLegs.Frame(3, 2, 0, 0, 0);
            Vector2 Origin = new Vector2(Frame.Width / 2, 0);//Frame.Size() * 0.5f;

            float stridePhase = CosmeticTime / 10.1f - segment*2 * 0.4f + MathHelper.Pi; // optional alternation
            float stride = MathF.Sin(stridePhase);
            float pushBack = MathHelper.ToRadians(35 * accelerationInterp);
            float strideSwing = MathHelper.ToRadians(stride * 10 * (1f - 0.5f * accelerationInterp));

            float Rotation = Rot + pushBack + strideSwing;

            Vector2 Scale = new Vector2(1 - 1.2f * segment / (float)SegmentCount);
            if (Scale.Length() < 0.4f)
                return;
            Main.EntitySpriteDraw(leechLegs, DrawPos, Frame, drawColor, Rotation, Origin, Scale, 0);

        }
        void RenderSegments(Color drawColor, Vector2 screenPosition)
        {

            for (int i = 0; i < SegmentCount; i++)
            {
                float rotation = i == 0 ? NPC.rotation: AdjHitboxes[i].Center().AngleTo(AdjHitboxes[i - 1].Center());
                RenderBackLegs(i, rotation, drawColor);
                if (i == SegmentCount - 1)
                    RenderTails(screenPosition, drawColor);
            }

            Main.spriteBatch.End();
            apple(drawColor); // draw primitive ribbon
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // --- 3️⃣ Draw FRONT LEGS + WHISKERS (in front)
            for (int i = 0; i < SegmentCount; i++)
            {
                float rotation = i == 0
                    ? NPC.rotation
                    : AdjHitboxes[i].Center().AngleTo(AdjHitboxes[i - 1].Center());

                renderLegs(i, rotation, drawColor);
                if (i == 0)
                    renderWhiskers(rotation, drawColor, AdjHitboxes[i].Center());
            }

        }


        void RenderSpine()
        {
            Color red = Color.Red;
            for (int i = 1; i < AdjHitboxes.Length; i++)
            {
                Utils.DrawLine(Main.spriteBatch, AdjHitboxes[i].Center(), AdjHitboxes[i - 1].Center(), red,red, 5);
            }
        }
        #endregion
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            bool debug =false;// NPC.ai[1] == 1 ? true : false;
            if (!NPC.IsABestiaryIconDummy)
            {
                /*
                string debug2 = $"{variant}";
                if (currentTarget != null)
                {
                    debug2 += $"\n"+ currentTarget.ToString();
                }
                
                Utils.DrawBorderString(spriteBatch, debug2, NPC.Center - screenPos, Color.AntiqueWhite, 1, anchory: -2);
                */
                if (AdjHitboxes != null)
                {
                    //RenderSpine();
                    for (int i = AdjHitboxes.Length - 1; i > -1; i--)
                    {
                       // if(i == SegmentCount -1)
                        RenderSegments(drawColor * NPC.Opacity, screenPos);
                        if (debug)
                        {
                            Utils.DrawRectangle(spriteBatch, AdjHitboxes[i].TopLeft(), AdjHitboxes[i].BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                            Utils.DrawBorderString(spriteBatch, i.ToString(), AdjHitboxes[i].Location.ToVector2() - Main.screenPosition, Color.AntiqueWhite);

                        }

                    }
                    if (debug)
                    {
                        Utils.DrawRectangle(spriteBatch, AdjHitboxes[0].TopLeft(), AdjHitboxes[0].BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                        // Utils.DrawBorderString(spriteBatch, SegmentCount.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

                        Texture2D debugArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;
                        Main.EntitySpriteDraw(debugArrow, NPC.Center - Main.screenPosition, null, Color.AntiqueWhite, NPC.rotation - MathHelper.PiOver2, new Vector2(debugArrow.Width / 2, 0), 1, SpriteEffects.FlipVertically);
                        Utils.DrawBorderString(Main.spriteBatch, NPC.velocity.ToString(), NPC.Center - Main.screenPosition, Color.AntiqueWhite);

                    }


                    for(int i = 0; i < _ExtraHitBoxes.Count; i++)
                    {
                       // Utils.DrawRectangle(spriteBatch, _ExtraHitBoxes[i].Hitbox.TopLeft(), _ExtraHitBoxes[i].Hitbox.BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                    }
                }

             
                return false;
            }
            //   else
            // {
            //      Texture2D leech = ModContent.Request<Texture2D>(Texture + "_Bestiary").Value;
            //      Vector2 DrawPos = NPC.Center - Main.screenPosition;

            //      Main.EntitySpriteDraw(leech, DrawPos, null, drawColor, 0, leech.Size() * 0.5f, 1, 0);
            //}


            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}

