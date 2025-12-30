using HeavenlyArsenal.Common.utils;
using Luminance.Common.VerletIntergration;
using NoxusBoss.Assets;
using NoxusBoss.Core.Physics.VerletIntergration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    public class ScavSona_FloppyHair_Player : ModPlayer
    {
        #region DrawHairToTarget;
        public override void Load()
        {
            On_Main.CheckMonoliths += CheckRenderHair;
            On_Player.UpdateTouchingTiles += UpdateHair;
        }

        private void UpdateHair(On_Player.orig_UpdateTouchingTiles orig, Player self)
        {
            orig(self);
            var p = self.GetModPlayer<ScavSona_FloppyHair_Player>();
            Vector2 headPos = self.MountedCenter;
            headPos.Y -= self.height / 2f - 6f;

            // Offset relative to facing direction
            Vector2 helmetOffset = new Vector2(6 * -self.direction,0) + Main.OffsetsPlayerHeadgear[self.bodyFrame.Y / self.bodyFrame.Height];
            Vector2 root = headPos + helmetOffset;
            p.hairStrip.Simulate(root, self.velocity);
        }

        public static RenderTarget2D ScavSona_Hair_Target;
        private void CheckRenderHair(On_Main.orig_CheckMonoliths orig)
        {

            orig();
            return;
            if (ScavSona_Hair_Target == null || ScavSona_Hair_Target.IsDisposed)
                ScavSona_Hair_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            else if (ScavSona_Hair_Target.Size() != new Vector2(Main.screenWidth , Main.screenHeight))
            {
                Main.QueueMainThreadAction(() =>
                {
                    ScavSona_Hair_Target.Dispose();
                    ScavSona_Hair_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(ScavSona_Hair_Target);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Player player in Main.ActivePlayers)
            {
                RenderPlayerHair(player);
            }


            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();
        }

        public static void RenderPlayerHair(Player player)
        {

            ScavSona_FloppyHair_Player p = player.GetModPlayer<ScavSona_FloppyHair_Player>();
            player.GetModPlayer<ScavSona_FloppyHair_Player>().hairStrip.DrawHairStrip(player,
                p.hairStrip.Positions,
                p.hairStrip.Positions.Length,
                GennedAssets.Textures.GreyscaleTextures.WhitePixel,
                1f,
                i =>
                {
                    float t = i / (float)(p.hairStrip.Positions.Length - 1);

                    float baseWidth = MathHelper.Lerp(14f, 5f, MathF.Pow(t, 1.5f));

                    // First bulge (near root)
                    float bulge1 =
                        MathF.Exp(-MathF.Pow((t - 0.18f) * 6f, 2f)) * 12f;

                    // Second bulge (mid hair)
                    float bulge2 =
                        MathF.Exp(-MathF.Pow((t - 0.45f) * 6f, 2f)) * 22f;

                    // Fade bulges toward tip
                    float envelope = MathF.Pow(1f - t, 1.4f);

                    return baseWidth;// + (bulge1 + bulge2) * envelope;
                }

            );

        }

        #endregion
        public class VerletHairStrip
        {
            public Vector2[] Positions;
            public Vector2[] OldPositions;

            public float SegmentLength;
            public int Iterations = 4;

            public VerletHairStrip(int segments, float segmentLength)
            {
                SegmentLength = segmentLength;
                Positions = new Vector2[segments];
                OldPositions = new Vector2[segments];
            }
            public void Initialize(Vector2 root)
            {
                for (int i = 0; i < Positions.Length; i++)
                {
                    Positions[i] = root + Vector2.UnitY * i * SegmentLength;
                    OldPositions[i] = Positions[i];
                }
            }
            public void Simulate(Vector2 root, Vector2 playerVelocity)
            {
                for (int i = 1; i < Positions.Length; i++)
                {
                    Vector2 vel = Positions[i] - OldPositions[i];
                    OldPositions[i] = Positions[i];

                    vel *= 0.82f;              // strong damping
                    vel.Y += 0.25f;            // gravity

                    if (i == 1)
                        vel += playerVelocity * 0.25f; // impulse only

                    Positions[i] += vel;
                }

                // Pin root
                Positions[0] = root;

                // Root angular stiffness
                Vector2 restDir = Vector2.UnitY;
                Vector2 desired = Positions[0] + restDir * SegmentLength;
                Positions[1] = Vector2.Lerp(Positions[1], desired, 0.35f);

                // Constraints (biased)
                ApplyConstraints();

            }

            private void ApplyConstraints()
            {
                for (int it = 0; it < Iterations; it++)
                {
                    for (int i = 0; i < Positions.Length - 1; i++)
                    {
                        Vector2 delta = Positions[i + 1] - Positions[i];
                        float dist = delta.Length();

                        if (dist == 0f)
                            continue;

                        float diff = (dist - SegmentLength) / dist;

                        if (i != 0)
                            Positions[i] += delta * diff * 0.5f;

                        Positions[i + 1] -= delta * diff * 0.5f;
                    }

                    Positions[0] = Positions[0]; // keep pinned
                }
            }
            #region verlet hair primitive

            public void DrawHairStrip(Player player, Vector2[] points, int length, Texture2D texture, float opacity, Func<int, float> thicknessAt)
            {
                if (points == null || length < 2 || texture == null)
                    return;

                Vector2 headPos = player.MountedCenter;
                headPos.Y -= player.height / 2f - 6f;
                Vector2 headAnchor = headPos - Main.screenPosition;
                

                // Up direction (Terraria screen-space)
                Vector2 scalpUp = -Vector2.UnitY;


                List<VertexPositionColorTexture> verts = new(length * 2);

                float totalLength = 0f;
                float[] segLen = new float[length];

                // Precompute segment distances
                for (int i = 1; i < length; i++)
                {
                    if (points[i] == Vector2.Zero || points[i - 1] == Vector2.Zero)
                        break;

                    float d = Vector2.Distance(points[i], points[i - 1]);
                    segLen[i] = d;
                    totalLength += d;
                }

                float accumulated = 0f;


                for (int i = 0; i < length - 1; i++)
                {
                    Vector2 currentWorld = points[i];
                    if (currentWorld == Vector2.Zero)
                        break;

                    Vector2 current = currentWorld - Main.screenPosition;

                    int iPrev = Math.Max(i - 1, 0);
                    int iNext = Math.Min(i + 1, length - 1);

                    Vector2 prev = points[iPrev];
                    Vector2 next = points[iNext];

                    if (prev == Vector2.Zero || next == Vector2.Zero)
                        break;

                    float t = i / (float)(length - 1);

                    // --- DIRECTION / NORMAL ---
                    Vector2 dir = (next - prev).SafeNormalize(Vector2.UnitY);
                    Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                  
                    // --- WIDTH WITH ROOT COLLAPSE ---
                    float thickness = thicknessAt(i);
                    float rootCollapse = Utils.SmoothStep(0f, 1f, t * 6f);
                    float halfWidth = thickness * 0.5f * rootCollapse;

                    Vector2 left = current + normal * halfWidth;
                    Vector2 right = current - normal * halfWidth;

                    float v = accumulated / Math.Max(totalLength, 0.001f);


                    Color c = Color.White;// * opacity;

                    verts.Add(new VertexPositionColorTexture(
                        new Vector3(left, 0f),
                        c,
                        new Vector2(0f, v)
                    ));

                    verts.Add(new VertexPositionColorTexture(
                        new Vector3(right, 0f),
                        c,
                        new Vector2(1f, v)
                    ));

                    accumulated += segLen[i];
                }

                DrawTriangleStripClean(verts, texture);
            }


            BasicEffect HairEffect;

            void DrawTriangleStripClean(List<VertexPositionColorTexture> verts, Texture2D tex)
            {
                if (verts.Count < 4)
                    return;

                if (Main.netMode == NetmodeID.Server)
                    return;

                GraphicsDevice gfx = Main.graphics.GraphicsDevice;

                if (HairEffect == null)
                {
                    HairEffect = new BasicEffect(gfx)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = true
                    };
                }

                HairEffect.Texture = tex;
                HairEffect.World = Matrix.Identity;
                HairEffect.View = Matrix.Identity;
                HairEffect.Projection = Matrix.CreateOrthographicOffCenter(
                    0, Main.screenWidth,
                    Main.screenHeight, 0,
                    -1f, 1f
                );
                gfx.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

                int primitiveCount = verts.Count - 2;
                if (primitiveCount <= 0)
                    return;

                var vertArray = verts.ToArray();

                foreach (EffectPass pass in HairEffect.CurrentTechnique.Passes)
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

            Vector2 HairShapeCurve(
            Vector2 root,
            float t,           
            float length,
            float maxWidth,
            int lobes,         // 2 or 3
            int direction      // player.direction
)
            {
                // Vertical fall
                float y = t * length;

                // Envelope: strong at top, dies at bottom
                float envelope = MathF.Pow(1f - t, 1.6f);

                // Lobe curve (matches your sketch)
                float wave = MathF.Sin(t * lobes * MathHelper.Pi);

                // Slight inward pull near tip
                float tipPull = MathF.Pow(t, 2.4f) * 0.35f;

                float x =
                    (wave * envelope * maxWidth * direction) -
                    (tipPull * maxWidth * direction);

                return root + new Vector2(x, y);
            }

        }

        public VerletHairStrip hairStrip;

        public const int MAX_HAIR_LENGTH = 20;
        public override void Initialize()
        {
            Vector2 headPos = Player.Center;
            headPos.Y -= Player.height / 2f - 6f;

            // Offset relative to facing direction
            Vector2 helmetOffset = new Vector2(6 * Player.direction, 2);
            Vector2 root = headPos + helmetOffset;
           
            hairStrip = new VerletHairStrip(MAX_HAIR_LENGTH, 1);
            hairStrip.Initialize(root);
        }

        public override void PostUpdateMiscEffects()
        {
            
        }

    }
}
