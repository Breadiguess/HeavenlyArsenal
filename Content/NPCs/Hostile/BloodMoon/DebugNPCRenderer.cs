using HeavenlyArsenal.Common.Players;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    partial class DebugNPC
    {
        BasicEffect Cone;
        public List<VertexPositionColorTexture> ConeVerts;
        void DrawCone()
        {
            if (ConeVerts.Count < 3)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            Cone = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = true
            };
            Cone.World = Matrix.Identity;
            Cone.View = Main.GameViewMatrix.ZoomMatrix;

            Cone.Texture = GennedAssets.Textures.Noise.SwirlNoise2;
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
        private void prepCone()
        {
            if (ConeVerts == null)
                ConeVerts = new();
            //BuildCone(ConeVerts, NPC.Center, NPC.Center.AngleTo(Main.MouseWorld), MathHelper.ToRadians(30), 1000, 300, Color.OrangeRed);
            //DrawCone();
        }
        void DrawArm(ref DebugNPCLimb DebugNPCLimb, Color drawColor, SpriteEffects effects)
        {
            Texture2D DebugTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            var armTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
            var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
            var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);

            var currentFrame = DebugNPCLimb.IsAnchored ? anchoredForearmFrame : defaultForearmFrame;


            Utils.DrawLine(Main.spriteBatch, DebugNPCLimb.Skeleton.Position(0), DebugNPCLimb.Skeleton.Position(1), Color.Red, Color.Red, 2);

            
            Utils.DrawLine(Main.spriteBatch, DebugNPCLimb.Skeleton.Position(1), DebugNPCLimb.Skeleton.Position(2), Color.Pink, Color.Pink, 3);
            Utils.DrawLine(Main.spriteBatch, DebugNPCLimb.Skeleton.Position(2), DebugNPCLimb.Skeleton.Position(3), Color.Green, Color.Green, 5);
            for(int i = 1; i < 3; i++)
            Main.EntitySpriteDraw(DebugTex, DebugNPCLimb.Skeleton.Position(i) - Main.screenPosition, null, Color.AntiqueWhite, 0, DebugTex.Size() / 2, 4, 0);
            //Utils.DrawBorderString(Main.spriteBatch, DebugNPCLimb.Skeleton.Position(0).Distance(DebugNPCLimb.EndPosition).ToString(), DebugNPCLimb.Skeleton.Position(1) - Main.screenPosition, Color.AntiqueWhite, scale: 0.3f);

            if (DebugNPCLimb.GrabPosition.HasValue)
            {
                Vector2 start = DebugNPCLimb.Skeleton.Position(0);
                Vector2 end = DebugNPCLimb.GrabPosition.Value;

                // The right-angle corner of the triangle
                Vector2 corner = new Vector2(start.X, end.Y);

                // Legs of the right triangle
                Vector2 verticalLeg = start - corner;  // vertical segment
                Vector2 horizontalLeg = end - corner;  // horizontal segment

                float cosTheta = 0f;

                if (verticalLeg.LengthSquared() > 0.0001f &&
                    horizontalLeg.LengthSquared() > 0.0001f)
                {
                    Vector2 v = Vector2.Normalize(verticalLeg);
                    Vector2 h = Vector2.Normalize(horizontalLeg);

                    cosTheta = Vector2.Dot(v, h);
                }
                // vectors forming the angle at the corner
                Vector2 legH = end - corner;   // horizontal
                Vector2 hypo = start - corner;   // hypotenuse

                cosTheta = 0f;

                if (legH.LengthSquared() > 0.0001f && hypo.LengthSquared() > 0.0001f)
                {
                    Vector2 nH = Vector2.Normalize(legH);
                    Vector2 nHyp = Vector2.Normalize(hypo);

                    cosTheta = Vector2.Dot(nH, nHyp);
                }
                Utils.DrawLine(Main.spriteBatch, start, end, Color.Blue, Color.Blue, 2); // hypotenuse
                Utils.DrawLine(Main.spriteBatch, start, corner, Color.Blue, Color.Blue, 2); // vertical
                Utils.DrawLine(Main.spriteBatch, corner, end, Color.Blue, Color.Blue, 2); // horizontal



            }


        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            for(int i = 0; i< LimbCount; i++)
            {
                //if (_limbs[i].GrabPosition.HasValue)
                //Utils.DrawLine(spriteBatch, _limbs[i].GrabPosition.Value, NPC.Center + _limbBaseOffsets[i], Color.AntiqueWhite);
            }

            for (int i = 0; i < LimbCount; i++)
            {
                DrawArm(ref _limbs[i], drawColor, 0);
                
            }
            //Main.NewText(_limbs[0].GrabPosition.Value.Distance(NPC.Center + _limbBaseOffsets[0]));
            for (int i = 0; i < LimbCount; i++)
            {
                // Main.EntitySpriteDraw(Debug, _limbs[i].EndPosition - screenPos, null, Color.Lime, 0, Debug.Size() / 2, 5, 0);
                 if (_limbs[i].GrabPosition.HasValue)
                    //grab position
                    Main.EntitySpriteDraw(Debug, _limbs[i].GrabPosition.Value - screenPos, null, Color.SkyBlue, 0, Debug.Size() / 2, 5, 0);
                //if (_limbs[i].PreviousGrabPosition.HasValue)
                //    Main.EntitySpriteDraw(Debug, _limbs[i].PreviousGrabPosition.Value - screenPos, null, Color.Purple, 0, Debug.Size() / 2, 5, 0);


                Main.EntitySpriteDraw(Debug, NPC.Center + _limbBaseOffsets[i] - screenPos, null, Color.Red, 0, Debug.Size() / 2, 10, 0);
                //Utils.DrawBorderString(spriteBatch, i.ToString(), NPC.Center + _limbBaseOffsets[i] - screenPos, Color.AntiqueWhite, anchory:1);
                //if(_limbs[i].GrabPosition.HasValue)
                //Utils.DrawBorderString(spriteBatch, _limbs[i].GrabPosition.Value.Distance(_limbBaseOffsets[i] + NPC.Center).ToString(), NPC.Center + _limbBaseOffsets[i] - screenPos, Color.Red, 2, 0, -2);
                Utils.DrawBorderString(spriteBatch, i.ToString(), NPC.Center + _limbBaseOffsets[i] - screenPos, Color.Green, scale: 0.5f);

            }
            Texture2D arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;
            Main.EntitySpriteDraw(arrow, NPC.Center - screenPos, null, Color.AntiqueWhite, NPC.velocity.ToRotation() * 10, new Vector2(arrow.Width/2, arrow.Height / 2), 1, 0);
            Utils.DrawBorderString(spriteBatch, MathHelper.ToDegrees(NPC.rotation).ToString(), NPC.Center - screenPos, Color.AntiqueWhite);
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab").Value;

            int frameHeight = texture.Height / 13;
            Vector2 DrawPos = NPC.Center - Main.screenPosition;

            SpriteEffects Direction = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : 0;

            Rectangle CrabFrame = texture.Frame(1, 13, 0, 0);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight - 30);
            //Main.EntitySpriteDraw(texture, NPC.Center - screenPos, CrabFrame, drawColor, NPC.rotation+MathHelper.PiOver2, origin, 1, Direction, 0);
            Main.LocalPlayer.GetModPlayer<HidePlayer>().ShouldHide = true;
            prepCone();
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }

        
    }
}
