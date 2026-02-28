using System.Collections.Generic;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar : BaseBloodMoonNPC
{
    private VertexBuffer isohedronBuffer;

    private BasicEffect isohedron;

    private BasicEffect basicEffect;

    private float currentYaw;

    private float currentPitch;

    private VertexPositionColorTexture[] maskVerts;

    private short[] maskIndices;

    private void DrawArm(ref RitualAltarLimb limb, Color drawColor, SpriteEffects effects)
    {
        var armTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
        var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
        var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);

        var currentFrame =  defaultForearmFrame;

        Vector2 StartPos;

        if (NPC.IsABestiaryIconDummy)
        {
            StartPos = limb.Skeleton.Position(0);
        }
        else
        {
            StartPos = limb.Skeleton.Position(0) - Main.screenPosition;
        }

        for(int i = 0; i< limb.Skeleton.PositionCount - 1; i++)
        {
            NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(Main.spriteBatch, limb.Skeleton.Position(i), limb.Skeleton.Position(i + 1), Color.Red, 3);
        }

        /*
        Main.spriteBatch.Draw
        (
            armTexture,
            StartPos,
            new Rectangle(94, 0, 48, 24),
            drawColor,
            (limb.Skeleton.Position(0) - limb.Skeleton.Position(1)).ToRotation(),
            new Vector2(134 - 94, 12),
            1f,
            effects,
            0f
        );

        if (NPC.IsABestiaryIconDummy)
        {
            StartPos = limb.Skeleton.Position(1);
        }
        else
        {
            StartPos = limb.Skeleton.Position(1) - Main.screenPosition;
        }

        Main.spriteBatch.Draw
        (
            armTexture,
            StartPos,
            currentFrame,
            drawColor,
            (limb.Skeleton.Position(1) - limb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
        */
    }

    public static VertexPositionColor[] Generate(float size, Color color)
    {
        var t = (1f + MathF.Sqrt(5f)) / 2f; // golden ratio φ
        var s = size / MathF.Sqrt(1f + t * t); // scale normalization

        // base icosahedron vertices (12 points)
        Vector3[] verts =
        {
            new(-s, t * s, 0),
            new(s, t * s, 0),
            new(-s, -t * s, 0),
            new(s, -t * s, 0),
            new(0, -s, t * s),
            new(0, s, t * s),
            new(0, -s, -t * s),
            new(0, s, -t * s),
            new(t * s, 0, -s),
            new(t * s, 0, s),
            new(-t * s, 0, -s),
            new(-t * s, 0, s)
        };

        // 20 faces (each as 3 vertex indices)
        int[] faces =
        {
            0,
            11,
            5,
            0,
            5,
            1,
            0,
            1,
            7,
            0,
            7,
            10,
            0,
            10,
            11,
            1,
            5,
            9,
            5,
            11,
            4,
            11,
            10,
            2,
            10,
            7,
            6,
            7,
            1,
            8,
            3,
            9,
            4,
            3,
            4,
            2,
            3,
            2,
            6,
            3,
            6,
            8,
            3,
            8,
            9,
            4,
            9,
            5,
            2,
            4,
            11,
            6,
            2,
            10,
            8,
            6,
            7,
            9,
            8,
            1
        };

        var result = new List<VertexPositionColor>(faces.Length);

        for (var i = 0; i < faces.Length; i++)
        {
            result.Add(new VertexPositionColor(verts[faces[i]], color));
        }

        return result.ToArray();
    }

    private void renderIsohedron(Vector2 AnchorPos, Color DrawColor)
    {
        if (Main.netMode == NetmodeID.Server || NPC.IsABestiaryIconDummy)
        {
            return;
        }

        var gd = Main.graphics.GraphicsDevice;

        // Ensure the effect exists
        isohedron ??= new BasicEffect(gd)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
            //TextureEnabled = true
        };

        var vertices = Generate(100f, DrawColor);

        isohedronBuffer ??= new VertexBuffer(gd, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        isohedronBuffer.SetData(vertices);

        isohedron.World =
            Matrix.CreateScale(0.5f) *
            Matrix.CreateRotationY(Main.GlobalTimeWrappedHourly * 1.2f) *
            Matrix.CreateRotationX(Main.GlobalTimeWrappedHourly * 0.8f) *
            Matrix.CreateTranslation(AnchorPos.X, AnchorPos.Y, 0);

        isohedron.View = Main.GameViewMatrix.ZoomMatrix;

        isohedron.Projection = Matrix.CreateOrthographicOffCenter
        (
            0,
            Main.screenWidth,
            Main.screenHeight,
            0,
            -1000f,
            1000f
        );

        gd.SetVertexBuffer(isohedronBuffer);

        gd.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            FillMode = FillMode.WireFrame
        };

        gd.DepthStencilState = DepthStencilState.None;
        gd.BlendState = BlendState.AlphaBlend;

        //isohedron.TextureEnabled = true;
        //isohedron.Texture = GennedAssets.Textures.GreyscaleTextures.Corona;
        // Draw
        foreach (var pass in isohedron.CurrentTechnique.Passes)
        {
            pass.Apply();
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Length / 3);
        }
    }

    private void DrawMask(Vector2 anchorScreenPos, Texture2D faceTex, float pixelSize = 96f)
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
       
        basicEffect.Projection = Matrix.CreateOrthographicOffCenter
        (
            0,
            Main.screenWidth,
            Main.screenHeight,
            0,
            -1000f,
            1000f
        );

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

            gd.DrawUserIndexedPrimitives
            (
                PrimitiveType.TriangleList,
                maskVerts,
                0,
                maskVerts.Length,
                maskIndices,
                0,
                maskIndices.Length / 3
            );
        }
    }

    private void RebuildMaskMesh(Color drawColor, int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f)
    {
        List<VertexPositionColorTexture> verts = new();
        List<short> indices = new();

        // Create concentric rings (center to rim)
        for (var ring = 0; ring <= ringSegments; ring++)
        {
            var r = ring / (float)ringSegments; // 0 -> 1
            var z = -curvature * (r * r); // gentle downward bend (−Z toward camera)

            for (var seg = 0; seg <= radialSegments; seg++)
            {
                var theta = MathHelper.TwoPi * (seg / (float)radialSegments);
                var x = MathF.Cos(theta) * r * 0.5f;
                var y = MathF.Sin(theta) * r * 0.5f;

                Vector2 uv = new((x + 0.5f) / 1f, (y + 0.5f) / 1f);
                verts.Add(new VertexPositionColorTexture(new Vector3(x, y, z), drawColor, uv));
            }
        }

        var stride = radialSegments + 1;

        for (var ring = 0; ring < ringSegments; ring++)
        {
            for (var seg = 0; seg < radialSegments; seg++)
            {
                var i0 = ring * stride + seg;
                var i1 = i0 + 1;
                var i2 = i0 + stride;
                var i3 = i2 + 1;

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

    private static float AngleTowards(float current, float target, float maxStep)
    {
        var delta = MathHelper.WrapAngle(target - current);

        if (delta > maxStep)
        {
            delta = maxStep;
        }

        if (delta < -maxStep)
        {
            delta = -maxStep;
        }

        return current + delta;
    }

    private void UpdateFaceAim(Vector2 toPlayer, int spriteDir, float yawMaxDeg = 28f, float pitchMaxDeg = 20f, float degPerSec = 180f)
    {
        yawMaxDeg = 50;
        pitchMaxDeg = 30;
        // Map screen delta to “desire” in [-1,1] then to limited angles
        var nx = MathHelper.Clamp(toPlayer.X / 180f, -1f, 1f);
        var ny = MathHelper.Clamp(toPlayer.Y / 180f, -1f, 1f);

        var yawMax = MathHelper.ToRadians(yawMaxDeg);
        var pitchMax = MathHelper.ToRadians(pitchMaxDeg);

        // Flip yaw with facing so “left/right” tracks the sprite
        var targetYaw = nx * yawMax * spriteDir;
        var targetPitch = ny * pitchMax;

        // Step with a speed cap (≈60 FPS)
        var maxStep = MathHelper.ToRadians(degPerSec) * (1f / 60f);
        currentYaw = AngleTowards(currentYaw, targetYaw, maxStep);
        currentPitch = AngleTowards(currentPitch, targetPitch, maxStep);

        // Clamp final angles just in case
        currentYaw = MathHelper.Clamp(currentYaw, -yawMax, yawMax);
        currentPitch = MathHelper.Clamp(currentPitch, -pitchMax, pitchMax);
    }

    private void mask(SpriteBatch spriteBatch, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            return;
        }

        spriteBatch.End();

        Main.spriteBatch.Begin
        (
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        RebuildMaskMesh(drawColor* NPC.Opacity, 20, 12, -0.2f);
        //RebuildMaskMesh_ColorDebug(20, 12, -0.2f);

        if (currentTarget != null)
        {
            var toPlayer = currentTarget.Center - NPC.Center;
            UpdateFaceAim(toPlayer, NPC.spriteDirection, 28f, 18f, 160f);
        }

        //Main.NewText("pitch: " + currentPitch + " yaw: " + currentYaw);
        Vector2 anchor;

        if (!NPC.IsABestiaryIconDummy)
        {
            anchor = NPC.Center - Main.screenPosition + new Vector2(NPC.height / 2, -2).RotatedBy(NPC.rotation) + new Vector2(-currentPitch, -currentYaw).RotatedBy(NPC.rotation) * 27.5f;
        }
        else
        {
            anchor = NPC.Center + new Vector2(-30, -90) + new Vector2(-currentPitch, -currentYaw).RotatedBy(-MathHelper.PiOver2) * 27.5f;
            UpdateFaceAim(Main.MouseWorld - Main.screenPosition - NPC.Center, NPC.spriteDirection, 28f, 18f, 160f);
        }

        var face = ModContent.Request<Texture2D>
            (
                "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarFace_" + Variant
            )
            .Value;

        DrawMask(anchor, face, 50f);
        //DrawMask_ColorDebug(anchor, 50);
    }

    private void drawLegs(Color drawColor)
    {
        if (!NPC.IsABestiaryIconDummy)
        {
            for (var i = 0; i < _limbs.Length; i++)
            {
                var a = NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;
                a = Math.Sign((_limbBaseOffsets[i] - NPC.Center).Length()) == 1 ? 0 : SpriteEffects.FlipVertically;
                DrawArm(ref _limbs[i], drawColor, a);
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // if (NPC.IsABestiaryIconDummy) return false;

        var BodyTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept").Value;
        var a = "";

        //if (CultistCoordinator.GetCultOfNPC(NPC) != null)
        {
        //    a += $"CultID: {CultistCoordinator.GetCultOfNPC(NPC).CultID}\n";
        }

        //a += $"{NPC.whoAmI}\n";
       // Utils.DrawBorderString(spriteBatch, a, NPC.Center - screenPos, Color.AntiqueWhite, 1, 0, -1);
        SpriteEffects sp = 0;
        var rot = NPC.rotation + MathHelper.PiOver2;
        var Origin = BodyTexture.Size() * 0.5f + new Vector2(0, 30);
        var DrawPos = NPC.Center - Main.screenPosition;

        if (NPC.IsABestiaryIconDummy)
        {
            DrawPos = NPC.Center;
            rot -= MathHelper.PiOver2;
        }

        Main.EntitySpriteDraw(BodyTexture, DrawPos, null, drawColor, rot, Origin, 1, sp);

        mask(spriteBatch, drawColor);

        drawLegs(drawColor * NPC.Opacity);
       // Utils.DrawBorderString(spriteBatch, currentAIState.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);
        Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;


        Vector2 anchor;

        if (!NPC.IsABestiaryIconDummy)
        {
            anchor = NPC.Center - Main.screenPosition + new Vector2(NPC.height * 2.0f, 0).RotatedBy(NPC.rotation);
        }
        else
        {
            anchor = NPC.Center + new Vector2(NPC.height * 2.0f, 0).RotatedBy(-MathHelper.PiOver2);
        }

        renderIsohedron(anchor, drawColor);
        DateTime date = DateTime.Now;
        if (date.Month == 12 || (date.Day == 24 || date.Day == 25))
        {
            Vector2 hatScale = new Vector2(0.2f, 0.4f);
            Vector2 hatDrawPosition = anchor;
            Texture2D santaHat = GennedAssets.Textures.NamelessDeity.SantaHat;

            Main.EntitySpriteDraw(santaHat, hatDrawPosition, null, Color.White * NPC.Opacity, NPC.rotation+MathHelper.PiOver2, santaHat.Size() * 0.5f, hatScale, 0);
        }

        var arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;

        // Main.EntitySpriteDraw(arrow, NPC.Center - screenPos, null, Color.AntiqueWhite, NPC.velocity.ToRotation() + MathHelper.PiOver2, new Vector2(arrow.Width / 2, arrow.Height), 1, 0);
        //Utils.DrawBorderString(spriteBatch, , NPC.Center - screenPos, Color.AntiqueWhite);
        return false;
    }

    #region Debug

    private VertexPositionColor[] maskVertsColor;

    private void RebuildMaskMesh_ColorDebug(int radialSegments = 24, int ringSegments = 12, float curvature = 0.4f)
    {
        List<VertexPositionColor> verts = new();
        List<short> indices = new();

        for (var ring = 0; ring <= ringSegments; ring++)
        {
            var r = ring / (float)ringSegments; // radius [0..1]
            var z = -curvature * (r * r); // negative Z = “toward camera”

            for (var seg = 0; seg <= radialSegments; seg++)
            {
                var theta = MathHelper.TwoPi * (seg / (float)radialSegments);
                var x = MathF.Cos(theta) * r * 0.5f;
                var y = MathF.Sin(theta) * r * 0.5f;

                // Color by height: red = top, blue = edges
                var t = (z - -curvature) / curvature; // normalized 0..1
                var c = Color.Lerp(Color.Blue, Color.Red, t);

                verts.Add(new VertexPositionColor(new Vector3(x, y, z), c));
            }
        }

        var stride = radialSegments + 1;

        for (var ring = 0; ring < ringSegments; ring++)
        {
            for (var seg = 0; seg < radialSegments; seg++)
            {
                var i0 = ring * stride + seg;
                var i1 = i0 + 1;
                var i2 = i0 + stride;
                var i3 = i2 + 1;

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

    private void DrawMask_ColorDebug(Vector2 anchorScreenPos, float pixelSize = 96f)
    {
        var gd = Main.graphics.GraphicsDevice;

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

        basicEffect.Projection = Matrix.CreateOrthographicOffCenter
        (
            0,
            Main.screenWidth,
            Main.screenHeight,
            0,
            -1000f,
            1000f
        );

        var wireframe = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None
        };

        gd.RasterizerState = wireframe;
        ;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            gd.DrawUserIndexedPrimitives
            (
                PrimitiveType.TriangleList,
                maskVertsColor,
                0,
                maskVertsColor.Length,
                maskIndices,
                0,
                maskIndices.Length / 3
            );
        }
    }

    #endregion
}