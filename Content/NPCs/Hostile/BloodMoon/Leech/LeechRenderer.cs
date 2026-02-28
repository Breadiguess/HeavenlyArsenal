using System.Linq;
using ReLogic.Content;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

partial class newLeech : BaseBloodMoonNPC
{
    public static RenderTarget2D leechTarget;

    private BasicEffect basicEffect;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            On_Main.CheckMonoliths += DrawLeech;
        }
    }

    public override void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            On_Main.CheckMonoliths -= DrawLeech;
        }
    }

    /// <summary>
    ///     Scouts each npc for
    /// </summary>
    /// <param name="orig"></param>
    private void DrawLeech(On_Main.orig_CheckMonoliths orig)
    {
        if (leechTarget == null || leechTarget.IsDisposed)
        {
            leechTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }
        else if (leechTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
        {
            Main.QueueMainThreadAction
            (
                () =>
                {
                    leechTarget.Dispose();
                    leechTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                }
            );

            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, default, null);

        Main.graphics.GraphicsDevice.SetRenderTarget(leechTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        foreach (var npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<newLeech>()))
        {
            ComposeLeech(npc);
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(null);
        Main.spriteBatch.End();
        orig();
    }

    private void ComposeLeech(NPC npc)
    {
        var slot = npc.GetGlobalNPC<leechSystemHelper>().StripSlot;
        var slotX = slot * LeechSystem.SlotWidth;
        var slotY = 0;

        if (slot < 0)
        {
            return;
        }

        var leech = npc.ModNPC as newLeech;

        var segCount = leech.AdjHitboxes.Length;

        var baseTex = ModContent.Request<Texture2D>
            (
                $"HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_{leech.variant}"
            )
            .Value;

        var frameWidth = baseTex.Width / 5;
        var frameHeight = baseTex.Height;

        var totalWidth = segCount * frameWidth;

        var offset = new Vector2(0, slot * frameHeight);

        for (var i = segCount - 1; i >= 0; i--)
        {
            Rectangle frame;

            if (i == 0)
            {
                frame = baseTex.Frame(5);
            }
            else if (i == 1)
            {
                frame = baseTex.Frame(5, 1, 1);
            }
            else if (i == segCount - 2)
            {
                frame = baseTex.Frame(5, 1, 3);
            }
            else if (i == segCount - 1)
            {
                frame = baseTex.Frame(5, 1, 4);
            }
            else
            {
                frame = baseTex.Frame(5, 1, 2);
            }

            var pos = offset + new Vector2(i * frameWidth, 0);

            var seg = segCount - i - 1;
            var color = Color.White * npc.Opacity;//Lighting.GetColor((leech.AdjHitboxes[seg].Center() / 16).ToPoint()) * npc.Opacity;

            Main.spriteBatch.Draw(baseTex, pos, frame, color);
        }
    }

    /// <summary>
    ///     Creates a primitive strip that
    /// </summary>
    /// <param name="drawColor"></param>
    private void renderLeechStrip(Color drawColor)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        var gd = Main.graphics.GraphicsDevice;

        if (basicEffect == null)
        {
            basicEffect = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                Alpha = NPC.Opacity
            };
        }

        var tex = ModContent.Request<Texture2D>(RealTexture).Value;
        var segCount = AdjHitboxes.Length;
        var subdivisions = 6;
        var firstDir = AdjHitboxes[1].Center() - AdjHitboxes[0].Center();

        if (firstDir.LengthSquared() < 1e-4f)
        {
            firstDir = Vector2.UnitX;
        }

        firstDir.Normalize();

        var lastDir = AdjHitboxes[segCount - 1].Center() - AdjHitboxes[segCount - 2].Center();

        if (lastDir.LengthSquared() < 1e-4f)
        {
            lastDir = Vector2.UnitX;
        }

        lastDir.Normalize();

        var preStart = AdjHitboxes[0].Center() - firstDir * (AdjHitboxes[0].Width * 1f);
        var postEnd = AdjHitboxes[segCount - 1].Center() + lastDir * (AdjHitboxes[segCount - 1].Width * 0f);

        var nodeCount = segCount + 1;
        var nodes = new Vector2[nodeCount];

        Vector2 GetNode(int i)
        {
            if (i < 0)
            {
                return nodes[0];
            }

            if (i >= nodes.Length)
            {
                return nodes[nodes.Length - 1];
            }

            return nodes[i];
        }

        nodes[0] = preStart;

        for (var i = 0; i < segCount; i++)
        {
            nodes[i + 1] = AdjHitboxes[i].Center();
        }

        nodes[nodeCount - 1] = postEnd;

        var totalLength = 0f;

        for (var i = 0; i < nodeCount - 1; i++)
        {
            totalLength += Vector2.Distance(nodes[i], nodes[i + 1]);
        }

        if (totalLength <= 1e-3f)
        {
            return;
        }

        //AAAAAAAAAAAAAAAAAA
        var pointsPerSegment = subdivisions + 1;
        var totalSegments = nodeCount - 1;
        var totalPoints = totalSegments * pointsPerSegment;
        var verts = new VertexPositionColorTexture[totalPoints * 2];

        var ribbonScale = 1.5f;
        var vIndex = 0;

        float widthToSample = tex.Frame(5).Width * segCount;
        float heightToSample = tex.Frame(5).Height;

        if (leechTarget == null)
        {
            return;
        }

        var slot = NPC.GetGlobalNPC<leechSystemHelper>().StripSlot;
        var aaaa = NPC.GetGlobalNPC<leechSystemHelper>().StripSlot;

        var y0 = slot * tex.Height / (float)leechTarget.Height;
        var y1 = (slot * tex.Height + tex.Height) / (float)leechTarget.Height;

        var uvTopY = y0;
        var uvBottomY = y1;
        var uvLeftX = widthToSample / leechTarget.Width;
        var uvRightX = 0f;

        for (var seg = totalSegments - 1; seg >= 0; seg--)
        {
            var p0 = GetNode(seg - 1);
            var p1 = GetNode(seg);
            var p2 = GetNode(seg + 1);
            var p3 = GetNode(seg + 2);

            var uSegStart = seg / (float)totalSegments;
            var uSegEnd = (seg + 1) / (float)totalSegments;

            for (var s = pointsPerSegment - 1; s >= 0; s--)
            {
                var t = subdivisions == 0 ? 0f : s / (float)subdivisions;

                var point = Vector2.CatmullRom(p0, p1, p2, p3, t);

                var dt = subdivisions >= 2 ? 1f / subdivisions : 0.5f;
                var posPrev = Vector2.CatmullRom(p0, p1, p2, p3, Math.Max(0f, t - dt));
                var posNext = Vector2.CatmullRom(p0, p1, p2, p3, Math.Min(1f, t + dt));

                var dir = posNext - posPrev;

                if (dir.LengthSquared() < 1e-4f)
                {
                    dir = Vector2.UnitX;
                }

                dir.Normalize();

                var normal = new Vector2(-dir.Y, dir.X);

                var hitboxIndex = Math.Clamp(seg, 0, segCount - 1);
                var width = AdjHitboxes[hitboxIndex].Width * 0.5f * ribbonScale;

                var left = point - normal * width - Main.screenPosition;
                var right = point + normal * width - Main.screenPosition;

                var horizontal0to1 = MathHelper.Lerp(uSegStart, uSegEnd, t);

                // remap horizontal 0=>1 to the proper size
                var uMapped = MathHelper.Lerp(uvLeftX, uvRightX, horizontal0to1);

                // final UVs
                var uvBottom = new Vector2(uMapped, uvBottomY);
                var uvTop = new Vector2(uMapped, uvTopY);


                Color thing = Lighting.GetColor(p1.ToTileCoordinates());

                verts[vIndex++] = new VertexPositionColorTexture
                (
                    new Vector3(left, -400),
                    thing,
                    uvBottom
                );

                verts[vIndex++] = new VertexPositionColorTexture
                (
                    new Vector3(right, 400 ),
                    thing,
                    uvTop
                );
            }
        }

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

        var adj = NPC.Center - Main.screenPosition;
        basicEffect.World = Matrix.Identity;

        //gd.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        gd.RasterizerState.CullMode = CullMode.None;
        basicEffect.TextureEnabled = true;

        if (leechTarget != null)
        {
            basicEffect.Texture = leechTarget;
        }
        else
        {
            basicEffect.Texture = tex;
        }

        gd.SamplerStates[0] = SamplerState.PointClamp;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            gd.DrawUserPrimitives
            (
                PrimitiveType.TriangleStrip,
                verts,
                0,
                verts.Length - 2
            );
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var debug = false; // NPC.ai[1] == 1 ? true : false;

        if (!NPC.IsABestiaryIconDummy)
        {
            //string debug2 = $"{variant}";
            //if (currentTarget != null)
            // {
            //    debug2 += $"\n"+ currentTarget.ToString();
            //}

            //Utils.DrawBorderString(spriteBatch, debug2, NPC.Center - screenPos, Color.AntiqueWhite, 1, anchory: -2);

            if (AdjHitboxes != null)
            {
                RenderSegments(drawColor * NPC.Opacity, screenPos);

                if (debug)
                {
                    for (var i = AdjHitboxes.Length - 1; i > -1; i--)
                    {
                        if (i == 0) { }

                        if (debug)
                        {
                            Utils.DrawRectangle(spriteBatch, AdjHitboxes[i].TopLeft(), AdjHitboxes[i].BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                            Utils.DrawBorderString(spriteBatch, i.ToString(), AdjHitboxes[i].Location.ToVector2() - Main.screenPosition, Color.AntiqueWhite);
                        }
                    }

                    Utils.DrawRectangle(spriteBatch, AdjHitboxes[0].TopLeft(), AdjHitboxes[0].BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                    // Utils.DrawBorderString(spriteBatch, SegmentCount.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

                    var debugArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;

                    Main.EntitySpriteDraw
                    (
                        debugArrow,
                        NPC.Center - Main.screenPosition,
                        null,
                        Color.AntiqueWhite,
                        NPC.rotation - MathHelper.PiOver2,
                        new Vector2(debugArrow.Width / 2, 0),
                        1,
                        SpriteEffects.FlipVertically
                    );

                    Utils.DrawBorderString(Main.spriteBatch, NPC.velocity.ToString(), NPC.Center - Main.screenPosition, Color.AntiqueWhite);
                }
                //Utils.DrawBorderString(spriteBatch, SegmentCount.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

                    for(int i = 0; i < _ExtraHitBoxes.Count; i++)
                {
                         //Utils.DrawRectangle(spriteBatch, _ExtraHitBoxes[i].Collider.TopLeft(), _ExtraHitBoxes[i].Collider.BottomRight(), Color.AntiqueWhite, Color.AntiqueWhite, 2);
                }
            }

            //Main.spriteBatch.Draw(leechTarget, NPC.Center - Main.screenPosition, null, Color.AntiqueWhite, 0, Vector2.Zero, 1, 0, 0f);
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

    #region Gores

    public static Asset<Texture2D>[] UmbralLeechGores { get; private set; }

    private void GetGoreInfo(out Texture2D texture, int SegmentInput, out int goreID)
    {
        texture = null;
        goreID = 0;

        if (Main.netMode != NetmodeID.Server)
        {
            var variant = SegmentInput;

            variant = (int)Math.Clamp(Utils.Remap(SegmentInput, 0, SegmentCount, 0, 6), 0, 6);

            variant = Math.Abs(6 - variant);
            texture = UmbralLeechGores[variant].Value;

            goreID = ModContent.Find<ModGore>(Mod.Name, $"UmbralLeechGore{variant + 1}").Type;
        }
    }

    private void createGore(Vector2 SpawnPos, int segment)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        //thanks lucille
        GetGoreInfo(out _, segment, out var goreID);

        Gore.NewGore(NPC.GetSource_FromThis(), SpawnPos, Vector2.Zero, goreID, NPC.scale);
    }

    #endregion

    #region Drawcode

    public string RealTexture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_" + variant;

    private void RenderTails(Vector2 screenPosition, Color drawColor)
    {
        if (Tail == null)
        {
            return;
        }

        // ManageTail();
        var tailtex = AssetDirectory.Textures.UmbralLeechTendril.Value;

        for (var i = 0; i < Tail.Count; i++)
        {
            var _tailPosition = Tail[i].Item1;

            for (var x = 0; x < _tailPosition.Length - 1; x++)
            {
                var DrawPos = _tailPosition[x] - screenPosition;
                var style = 0;

                if (x == _tailPosition.Length - 3)
                {
                    style = 1;
                }

                if (x > _tailPosition.Length - 3)
                {
                    style = 2;
                }

                var frame = tailtex.Frame(1, 3, 0, style);

                var rotation = _tailPosition[x].AngleTo(_tailPosition[x + 1]);

                var stretch = new Vector2
                              (
                                  0.25f + Utils.GetLerpValue(0, _tailPosition.Length, x, true),
                                  _tailPosition[x].Distance(_tailPosition[x + 1]) / (frame.Height - 5) * 1.2f
                              ) *
                              1.2f;

                var color = Lighting.GetColor(((DrawPos + screenPosition) / 16).ToPoint()) * NPC.Opacity;

                Main.EntitySpriteDraw(tailtex, DrawPos, frame, color, rotation - MathHelper.PiOver2, frame.Size() * 0.5f, stretch, 0);
                //Utils.DrawBorderString(Main.spriteBatch, i.ToString(),DrawPos,Color.AntiqueWhite, 0.3f);
            }
        }
    }

    private void renderWhiskers(float Rot, Color drawColor, Vector2 AnchorPos)
    {
        if (WhiskerAnchors == null)
        {
            return;
        }

        /*
        WhiskerAnchors = new Vector2[]
        {
            new Vector2(16, 0),
            new Vector2(16, 14),
            new Vector2(5, 0),
            new Vector2(5, 14)
        };*/
        var tex = AssetDirectory.Textures.UmbralLeechWhisker.Value;
        var DrawPos = AdjHitboxes[0].Center() - Main.screenPosition;
        var a = 0;

        foreach (var i in WhiskerAnchors)
        {
            DrawPos = AnchorPos + i.RotatedBy(Rot) - new Vector2(-12, 0).RotatedBy(Rot) - Main.screenPosition;
            var Frame = tex.Frame(1, 4, 0, a);

            var Origin = new Vector2(0, Frame.Height / 2);
            var Rotation = Rot + MathHelper.ToRadians(a * 2 + MathF.Sin(CosmeticTime / 10.1f + a * 10) * 30); //MathHelper.ToRadians(20 * accelerationInterp);
            Main.EntitySpriteDraw(tex, DrawPos, Frame, drawColor, Rotation, Origin, new Vector2(1), 0);

            a++;
        }
    }

    private void renderLegs(int segment, float Rot, Color drawColor)
    {
        if (segment == 0 || segment == SegmentCount - 1)
        {
            return;
        }

        var leechLegs = AssetDirectory.Textures.UmbralLeech_Legs.Value;
        var DrawPos = AdjHitboxes[segment].Center() + new Vector2(0, 18).RotatedBy(Rot) - Main.screenPosition;
        var Frame = leechLegs.Frame(3, 2, 0, 1);
        var Origin = new Vector2(Frame.Width / 2, 0); //Frame.Size() * 0.5f;

        var stridePhase = CosmeticTime / 10.1f - segment * 2 * 0.4f;
        var stride = MathF.Sin(stridePhase);
        var pushBack = MathHelper.ToRadians(35 * accelerationInterp);
        var strideSwing = MathHelper.ToRadians(stride * 10 * (1f - 0.5f * accelerationInterp));

        var Rotation = Rot + pushBack + strideSwing;
        var Scale = new Vector2(1 - 1.2f * segment / SegmentCount);

        if (Scale.Length() < 0.4f)
        {
            return;
        }

        var color = Lighting.GetColor((AdjHitboxes[segment].Center() / 16).ToPoint()) * NPC.Opacity;

        Main.EntitySpriteDraw(leechLegs, DrawPos, Frame, color, Rotation, Origin, Scale, 0);

        //Utils.DrawBorderString(Main.spriteBatch, Scale.Length().ToString(), DrawPos, Color.AntiqueWhite);
    }

    private void RenderBackLegs(int segment, float Rot, Color drawColor)
    {
        if (segment == 0 || segment == SegmentCount - 1)
        {
            return;
        }

        var leechLegs = AssetDirectory.Textures.UmbralLeech_Legs.Value; //ModContent.Request<Texture2D>(RealTexture + "_Legs").Value;
        var DrawPos = AdjHitboxes[segment].Center() + new Vector2(-10, 18).RotatedBy(Rot) - Main.screenPosition;
        var Frame = leechLegs.Frame(3, 2);
        var Origin = new Vector2(Frame.Width / 2, 0); //Frame.Size() * 0.5f;

        var stridePhase = CosmeticTime / 10.1f - segment * 2 * 0.4f + MathHelper.Pi;
        var stride = MathF.Sin(stridePhase);
        var pushBack = MathHelper.ToRadians(35 * accelerationInterp);
        var strideSwing = MathHelper.ToRadians(stride * 10 * (1f - 0.5f * accelerationInterp));

        var Rotation = Rot + pushBack + strideSwing;

        var Scale = new Vector2(1 - 1.2f * segment / SegmentCount);

        if (Scale.Length() < 0.4f)
        {
            return;
        }

        var color = Lighting.GetColor((AdjHitboxes[segment].Center() / 16).ToPoint()) * NPC.Opacity;

        Main.EntitySpriteDraw(leechLegs, DrawPos, Frame, color, Rotation, Origin, Scale, 0);
    }

    private void RenderSegments(Color drawColor, Vector2 screenPosition)
    {
        for (var i = 1; i < AdjHitboxes.Length; i++)
        {
            var rotation = i == 0 ? NPC.rotation : AdjHitboxes[i].Center().AngleTo(AdjHitboxes[i - 1].Center());
            RenderBackLegs(i, rotation, drawColor);

            if (i == SegmentCount - 1)
            {
                RenderTails(screenPosition, drawColor);
            }
        }

        Main.spriteBatch.End();
        renderLeechStrip(drawColor); // draw primitive ribbon

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

        for (var i = 0; i < SegmentCount; i++)
        {
            var rotation = i == 0
                ? NPC.rotation
                : AdjHitboxes[i].Center().AngleTo(AdjHitboxes[i - 1].Center());

            renderLegs(i, rotation, drawColor);

            if (i == 0)
            {
                renderWhiskers(rotation, drawColor, AdjHitboxes[i].Center());
            }
        }
    }

    private void RenderSpine()
    {
        var red = Color.Red;

        for (var i = 1; i < AdjHitboxes.Length; i++)
        {
            Utils.DrawLine(Main.spriteBatch, AdjHitboxes[i].Center(), AdjHitboxes[i - 1].Center(), red, red, 5);
        }
    }

    #endregion
}