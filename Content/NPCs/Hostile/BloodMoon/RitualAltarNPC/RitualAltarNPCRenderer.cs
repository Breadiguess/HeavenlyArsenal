using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar : BloodmoonBaseNPC
{
    static void DrawArm(ref RitualAltarLimb RitualAltarLimb, Color drawColor, SpriteEffects effects)
    {
        var armTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
        var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
        var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);

        var currentFrame = RitualAltarLimb.IsAnchored ? anchoredForearmFrame : defaultForearmFrame;

        Main.spriteBatch.Draw(
            armTexture,
            RitualAltarLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(94, 0, 48, 24),
            drawColor,
            (RitualAltarLimb.Skeleton.Position(0) - RitualAltarLimb.Skeleton.Position(1)).ToRotation(),
            new Vector2(134 - 94, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            armTexture,
            RitualAltarLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (RitualAltarLimb.Skeleton.Position(1) - RitualAltarLimb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
    }
    VertexBuffer isohedronBuffer;
    public static VertexPositionColor[] Generate(float size, Color color)
    {
        float t = (1f + MathF.Sqrt(5f)) / 2f; // golden ratio φ
        float s = size / MathF.Sqrt(1f + t * t); // scale normalization

        // base icosahedron vertices (12 points)
        Vector3[] verts =
        {
            new(-s,  t*s, 0),
            new( s,  t*s, 0),
            new(-s, -t*s, 0),
            new( s, -t*s, 0),
            new(0, -s,  t*s),
            new(0,  s,  t*s),
            new(0, -s, -t*s),
            new(0,  s, -t*s),
            new( t*s, 0, -s),
            new( t*s, 0,  s),
            new(-t*s, 0, -s),
            new(-t*s, 0,  s),
        };

        // 20 faces (each as 3 vertex indices)
        int[] faces =
        {
            0,11,5,  0,5,1,  0,1,7,  0,7,10,  0,10,11,
            1,5,9,   5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4,   3,4,2,  3,2,6,  3,6,8,  3,8,9,
            4,9,5,   2,4,11, 6,2,10, 8,6,7, 9,8,1
        };

        List<VertexPositionColor> result = new List<VertexPositionColor>(faces.Length);
        for (int i = 0; i < faces.Length; i++)
        {
            result.Add(new VertexPositionColor(verts[faces[i]], color));
        }
        return result.ToArray();
    }
    void renderIsohedron(Vector2 AnchorPos, Color DrawColor)
    {
        if (Main.netMode == NetmodeID.Server)
            return;
        var gd = Main.graphics.GraphicsDevice;

        // Ensure the effect exists
        isohedron ??= new BasicEffect(gd)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        // Generate vertex data (size = 100f, color = white)
        var vertices = Generate(100f, DrawColor);

        isohedronBuffer ??= new VertexBuffer(gd, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        isohedronBuffer.SetData(vertices);


        isohedron.World =
            Matrix.CreateScale(0.5f) *
            Matrix.CreateRotationY(Main.GlobalTimeWrappedHourly * 1.2f) *
            Matrix.CreateRotationX(Main.GlobalTimeWrappedHourly * 0.8f) *
            Matrix.CreateTranslation(AnchorPos.X, AnchorPos.Y, 0);

        // Let Terraria handle zoom/panning automatically
        isohedron.View = Main.GameViewMatrix.ZoomMatrix;

        isohedron.Projection = Matrix.CreateOrthographicOffCenter(
            0, Main.screenWidth,
            Main.screenHeight, 0,
            -1000f, 1000f);

        gd.SetVertexBuffer(isohedronBuffer);
        gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        gd.DepthStencilState = DepthStencilState.None;
        gd.BlendState = BlendState.AlphaBlend;
        // Draw
        foreach (EffectPass pass in isohedron.CurrentTechnique.Passes)
        {
            pass.Apply();
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Length / 3);
        }
    }



    BasicEffect isohedron;
    BasicEffect basicEffect;
    float currentYaw;
    float currentPitch;
    VertexPositionColorTexture[] maskVerts;
    short[] maskIndices;
    #region Debug
    VertexPositionColor[] maskVertsColor;
    void RebuildMaskMesh_ColorDebug(int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f)
    {
        List<VertexPositionColor> verts = new();
        List<short> indices = new();

        for (int ring = 0; ring <= ringSegments; ring++)
        {
            float r = ring / (float)ringSegments; // radius [0..1]
            float z = -curvature * (r * r);       // negative Z = “toward camera”

            for (int seg = 0; seg <= radialSegments; seg++)
            {
                float theta = MathHelper.TwoPi * (seg / (float)radialSegments);
                float x = MathF.Cos(theta) * r * 0.5f;
                float y = MathF.Sin(theta) * r * 0.5f;

                // Color by height: red = top, blue = edges
                float t = (z - (-curvature)) / curvature; // normalized 0..1
                Color c = Color.Lerp(Color.Blue, Color.Red, t);

                verts.Add(new VertexPositionColor(new Vector3(x, y, z), c));
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

        maskVertsColor = verts.ToArray();
        maskIndices = indices.ToArray();
    }

    void DrawMask_ColorDebug(Vector2 anchorScreenPos, float pixelSize = 96f)
    {
        GraphicsDevice gd = Main.graphics.GraphicsDevice;

        if (basicEffect == null)
        {
            basicEffect = new BasicEffect(gd)
            {
                TextureEnabled = false,
                VertexColorEnabled = true,
                LightingEnabled = false,
                DiffuseColor = Vector3.One
            };
        }

        basicEffect.World =
            Matrix.CreateScale(pixelSize) *
            Matrix.CreateRotationY(currentYaw) *
            Matrix.CreateRotationX(currentPitch) *
            Matrix.CreateTranslation(anchorScreenPos.X, anchorScreenPos.Y, 0f);

        basicEffect.View = Main.GameViewMatrix.ZoomMatrix;
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, Main.screenWidth,
            Main.screenHeight, 0,
            -1000f, 1000f);

        RasterizerState wireframe = new RasterizerState()
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None
        };
        gd.RasterizerState = wireframe; ;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply(); gd.DrawUserIndexedPrimitives(
    PrimitiveType.TriangleList,
    maskVertsColor, 0, maskVertsColor.Length,
    maskIndices, 0, maskIndices.Length / 3);

        }
    }
    #endregion
    void DrawMask(Vector2 anchorScreenPos, Texture2D faceTex, float pixelSize = 96f)
    {
        var gd = Main.graphics.GraphicsDevice;

        if (basicEffect == null)
        {
            basicEffect = new BasicEffect(gd)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                LightingEnabled = false,
                Alpha = NPC.Opacity,
                DiffuseColor = Vector3.One
            };
        }

        basicEffect.Texture = faceTex;
        basicEffect.View = Main.GameViewMatrix.ZoomMatrix;
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                                    0, Main.screenWidth,
                                    Main.screenHeight, 0,
                                    -1000f, 1000f); // allow Z wiggle safely

        // Order: Scale -> Yaw -> Pitch -> Translate

        basicEffect.World =
            Matrix.CreateScale(pixelSize) *
            Matrix.CreateRotationY(currentYaw) *
            Matrix.CreateRotationX(currentPitch) *
          
            Matrix.CreateTranslation(anchorScreenPos.X, anchorScreenPos.Y, 0f);

        gd.BlendState = BlendState.AlphaBlend;
        gd.DepthStencilState = DepthStencilState.None;
        gd.RasterizerState = RasterizerState.CullNone;
        gd.SamplerStates[0] = SamplerState.PointClamp;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            gd.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                maskVerts, 0, maskVerts.Length,
                maskIndices, 0, maskIndices.Length / 3);
        }
    }
    void RebuildMaskMesh(Color drawColor, int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f )
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
                verts.Add(new VertexPositionColorTexture(new Vector3(x, y, z), drawColor, uv));
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

    static void DrawLeg(ref RitualAltarLimb nightgauntLimb, Color drawColor, SpriteEffects effects)
    {
        var legTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
        var defaultLowerLegFrame = new Rectangle(0, 0, 80, 26);
        var anchoredLowerLegFrame = new Rectangle(0, 26, 80, 26);

        var currentFrame = nightgauntLimb.IsAnchored ? anchoredLowerLegFrame : defaultLowerLegFrame;

        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(84, 0, 66, 26),
            drawColor,
            (nightgauntLimb.Skeleton.Position(0) - nightgauntLimb.Skeleton.Position(1)).ToRotation(),
            new Vector2(134 - 74, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (nightgauntLimb.Skeleton.Position(1) - nightgauntLimb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
    }
    void mask(SpriteBatch spriteBatch, Color drawColor)
    {
        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
               DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        
        RebuildMaskMesh(drawColor, 20, 12, -0.2f);
        //RebuildMaskMesh_ColorDebug(20, 12, -4.2f);
        
        if(currentTarget != null)
        {
            Vector2 toPlayer = currentTarget.Center - NPC.Center;
            UpdateFaceAim(toPlayer, NPC.spriteDirection, yawMaxDeg: 28f, pitchMaxDeg: 28f, degPerSec: 160f);

        }
        //Main.NewText("pitch: " + currentPitch + " yaw: " + currentYaw);
        Vector2 anchor = NPC.Center - Main.screenPosition + new Vector2(NPC.height / 2, -2).RotatedBy(NPC.rotation) + new Vector2(-currentPitch, -currentYaw).RotatedBy(NPC.rotation)*27.5f;


        Texture2D face = ModContent.Request<Texture2D>(
            "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarFace").Value;

        DrawMask(anchor, face, pixelSize: 50f);
        //DrawMask_ColorDebug(anchor, 50);

    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy) return false;

        Texture2D BodyTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept").Value;


        SpriteEffects sp = 0;
        float rot = NPC.rotation + MathHelper.PiOver2;
        Vector2 Origin = BodyTexture.Size() * 0.5f + new Vector2(0, 30);
        Main.EntitySpriteDraw(BodyTexture, NPC.Center - Main.screenPosition, null, drawColor, rot, Origin, 1, sp);
        
        mask(spriteBatch, drawColor);
        for (int i = 0; i < _limbs.Length; i++)
        {

            SpriteEffects a = NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;
            a = Math.Sign((_limbs[i].EndPosition - _limbs[i].TargetPosition).Length()) == 1 ? 0 : SpriteEffects.FlipVertically;
            DrawArm(ref _limbs[i], drawColor, a);
        }
        // Utils.DrawBorderString(spriteBatch, currentAIState.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);
        /*
         for (int i = 0; i < _limbs.Length; i++)
         {
             Vector2 DrawPos = _limbs[i].TargetPosition - screenPos;
             //spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.WhitePixel, DrawPos, new Rectangle(0, 0, 5, 5), Color.Lime);
             //string msgd = $"{i + 1}\n {_limbs[i].IsTouchingGround}";
             // Utils.DrawBorderString(Main.spriteBatch, msgd, DrawPos, Color.AntiqueWhite);

         }*/
        Vector2 anchor = NPC.Center - Main.screenPosition + new Vector2(NPC.height * 2.0f, 0).RotatedBy(NPC.rotation);

        renderIsohedron(anchor, drawColor);
       
        return false;
    }
}