using HeavenlyArsenal.Common.IK;
using NoxusBoss.Assets;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    internal class ScavSona_ArmManager : ModPlayer
    {
        #region IK Arm Rendering and stuff
        public override void Load()
        {
            On_Main.CheckMonoliths += CheckRenderArms;
            On_Player.UpdateTouchingTiles += UpdateArms;
        }

    
        private void CheckRenderArms(On_Main.orig_CheckMonoliths orig)
        {

            if (ScavSona_IKArm.ScavSona_IKArm_Target == null || ScavSona_IKArm.ScavSona_IKArm_Target.IsDisposed)
                ScavSona_IKArm.ScavSona_IKArm_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            else if (ScavSona_IKArm.ScavSona_IKArm_Target.Size() != new Vector2(Main.screenWidth / 2, Main.screenHeight / 2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    ScavSona_IKArm.ScavSona_IKArm_Target.Dispose();
                    ScavSona_IKArm.ScavSona_IKArm_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(ScavSona_IKArm.ScavSona_IKArm_Target);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Player player in Main.ActivePlayers)
                RenderIKArms(player);


            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();

        }
        static void GetIKArmSkeletonPoints(ScavSona_IKArm arm, List<Vector2> points)
        {
            points.Clear();

            int count = arm.ArmSkeleton.PositionCount;
            for (int i = 0; i < count; i++)
            {
                points.Add(arm.ArmSkeleton.Position(i));
            }
        }
        static void RenderIKArms(Player player)
        {
            var arm = player.GetModPlayer<ScavSona_ArmManager>();

            Texture2D tex =  ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Vanity/ScavSona/ScavSona_IKArm").Value;
            if (arm == null || arm._IKArms == null)
                return;
            foreach (ScavSona_IKArm Arm in arm._IKArms)
            {
                RenderIKArmPrimitive(player, Arm);

            }
        }

        static void BuildSmoothArmMesh(
            List<VertexPositionColorTexture> verts,
            List<short> indices,
            List<Vector2> spine,
            float baseThickness,
            Color baseColor
            )
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

                // Thicker near shoulder, thinner at hand
                float thickness =
                    MathHelper.Lerp(baseThickness, baseThickness * 0.8f, t);

                Color color = Color.White;// Lighting.GetColor(p.ToTileCoordinates());

                Vector2 left = p + normal * thickness;
                Vector2 right = p - normal * thickness;

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(left - Main.screenPosition, 0f),
                    color,
                    new Vector2(0f, t)
                ));

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(right - Main.screenPosition, 0f),
                    color,
                    new Vector2(1f, t)
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
        public BasicEffect ArmEffect;
        public List<VertexPositionColorTexture> _armVerts = new();
        public List<short> _armIndices = new();
        public List<Vector2> _armControl = new();
        public  List<Vector2> _armSmooth = new();

       static void RenderIKArmPrimitive(Player player, ScavSona_IKArm arm)
        {
            ScavSona_ArmManager guy = player.GetModPlayer<ScavSona_ArmManager>();
            if (arm == null)
                return;
            GetIKArmSkeletonPoints(arm, guy._armControl);
            SampleSmoothSpine(guy._armControl, guy._armSmooth, samplesPerSegment: 10);

            BuildSmoothArmMesh(guy._armVerts, guy._armIndices, guy._armSmooth, 6f, Color.White);

            if (guy._armIndices.Count == 0)
                return;

            var gd = Main.graphics.GraphicsDevice;

            guy.ArmEffect ??= new BasicEffect(gd)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                LightingEnabled = false,
                DiffuseColor = Vector3.One
            };
            guy.ArmEffect.Texture =  ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Vanity/ScavSona/ScavSona_IKArm").Value;


            guy.ArmEffect.View = Matrix.Identity;
            guy.ArmEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000f, 1000f
            );
            guy.ArmEffect.World = Matrix.Identity;
            
            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.SamplerStates[0] = SamplerState.PointClamp;

            foreach (var pass in guy.ArmEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    guy._armVerts.ToArray(),
                    0,
                    guy._armVerts.Count,
                    guy._armIndices.ToArray(),
                    0,
                    guy._armIndices.Count / 3
                );
            }
        }

        static void SampleSmoothSpine(List<Vector2> control, List<Vector2> sampled, int samplesPerSegment)
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
        #endregion
        public const int MAX_IK_ARMS = 6;
        public List<ScavSona_IKArm> _IKArms;
        public bool Active;

        public override void Initialize()
        {
            _IKArms = new List<ScavSona_IKArm>(MAX_IK_ARMS);
            for (int i = 0; i < MAX_IK_ARMS; i++)
            {
                
                //todo: make the middle arms (alternates) smaller than the other arms, with the longest on the bottom and the shortest near the top
                IKSkeleton skeleton =
                    new IKSkeleton((30, new IKSkeleton.Constraints
                    {
                        
                        
                    }),
                    (20, new IKSkeleton.Constraints
                    {
                    }));

                _IKArms.Add(new ScavSona_IKArm(Player, skeleton, i % 3 == 0));
            }
        }

      
        public override void PostUpdateMiscEffects()
        {
         
            if (!Active)
                return;

            if (_IKArms is null)
            {
                //safetey check
                _IKArms = new List<ScavSona_IKArm>(MAX_IK_ARMS);
                for (int i = 0; i < MAX_IK_ARMS; i++)
                {
                    IKSkeleton skeleton =
                        new IKSkeleton((30, new IKSkeleton.Constraints
                        {

                        }),
                        (20, new IKSkeleton.Constraints()));

                    _IKArms.Add(new ScavSona_IKArm(Player, skeleton, i % 3 == 0));
                }
            }
            //_IKArms = null;

          
        }
        private void UpdateArms(On_Player.orig_UpdateTouchingTiles orig, Player self)
        {

            ScavSona_ArmManager p = self.GetModPlayer<ScavSona_ArmManager>();

            if (p._IKArms != null)
            {
                for (int i = 0; i < MAX_IK_ARMS; i++)
                {
                    ScavSona_IKArm arm = p._IKArms[i];

                    int side = (i % 2 == 0) ? 1 : -1;

                    float idle = MathF.Sin(Main.GameUpdateCount * 0.05f + i) * 8f;


                    float yOffset = ComputeArmEndYOffset(i, MAX_IK_ARMS);


                    Vector2 target =
                        self.Center +
                        new Vector2(
                            side * 56f + idle * side, yOffset + MathF.Cos(Main.GameUpdateCount * 0.05f) * 14
                        );

                    ScavSona_IKArm.UpdateArmIK(
                        arm,
                        self.Center,
                        target
                    );

                    ScavSona_IKArm.UpdateArmString(arm);
                }
            }

            p = self.GetModPlayer<ScavSona_ArmManager>();
        }

        const float ArmVerticalSpan = 96f; 


        static float ComputeArmEndYOffset(int armIndex, int totalArms)
        {
            int pairs = totalArms / 2;
            int pairIndex = armIndex / 2;

            if (pairs <= 1)
                return 0f;

            // Normalized vertical position [0..1]
            float t = pairIndex / (float)(pairs - 1);

            // Center it around zero
            t = t * 2f - 1f;

            t = MathF.Sign(t) * MathF.Pow(MathF.Abs(t), 0.85f);

            return t * (ArmVerticalSpan * 0.5f);
        }

        public override void ResetEffects()
        {
            Active = true;
        }
    }
}
