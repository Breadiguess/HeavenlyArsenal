using CalamityMod;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using Luminance.Common.Easings;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Whip
{
    public class FleshkinAcolyte_Whip : BloodMoonBaseNPC
    {
        #region tendril
        private Vector2 tendrilTarget;


        private bool shouldTendrilDraw => TendrilRope != null;
        private bool shouldTendrilExist = false;
        public Rope TendrilRope;
        public Rectangle TendrilTipHitbox;

        private float t;
        public PiecewiseCurve AttackCurve;
        float TendrilAttackInterpolant
        {
            get => AttackCurve == null ? 0 : AttackCurve.Evaluate(t);
        }
        void ManageTendril()
        {
            if (shouldTendrilExist)
            {
                if (TendrilRope == null)
                {
                    TendrilTipHitbox = new Rectangle(0, 0, 70, 70);
                    TendrilTipHitbox.Location = (NPC.position+ new Vector2(1*NPC.spriteDirection, 0)).ToPoint();
                    TendrilRope = new Rope(NPC.Center, TendrilTipHitbox.Center(), 40, 1, Vector2.Zero);
                    TendrilRope.damping = 0.4f;
                }
                
                    TendrilRope.segments[0].position = NPC.Center;
                    TendrilRope.segments[^1].position = TendrilTipHitbox.Center();
                    TendrilRope.Update();
                
            }
            else
            {
                TendrilRope = null;
            }
        }
        #endregion

        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodCult/FleshkinAcolyte_Whip/FleshkinAcolyte_Whip";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 31;
        }
        public override void SetDefaults()
        {
            NPC.friendly = false;
            NPC.lifeMax = 40;
            NPC.defense = 500694;
            NPC.Size = new Vector2(60, 70);

            NPC.damage = 30;
        }

        public override bool PreAI()
        {
            ManageTendril();

            if(Main.LocalPlayer.controlUseItem)
            shouldTendrilExist = true;

            return base.PreAI();
        }
        public override void AI()
        {
            if (shouldTendrilExist)
            {
                // Initialize once at the start of the attack.
                if (Time == 0)
                {
                    tendrilTarget = Main.MouseWorld;


                }
                AttackCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Cubic, EasingType.InOut, 1f, 0.55f)
                    //.Add(EasingCurves.Linear, EasingType.InOut, 0.56f, 0.58f,1)
                    .Add(EasingCurves.Quartic, EasingType.In, 0.01f, 1f);
                Time++;

                const float attackDuration = 30f;
                t = Math.Clamp(Time / attackDuration, 0f, 1f);

                float interp = TendrilAttackInterpolant;



                for (int i = 0; i < 100; i++)
                {
                    Dust.NewDustPerfect(Main.LocalPlayer.Center + new Vector2(i * 2, AttackCurve.Evaluate(i / 100f) * 100), DustID.Cloud, Vector2.Zero);
                }


                // Drive the tip with the curve (retraction happens automatically as interp returns to 0).
                Vector2 start = NPC.Center - TendrilTipHitbox.Size() / 2f;
                Vector2 end = tendrilTarget - TendrilTipHitbox.Size() / 2f;
                TendrilTipHitbox.Location = Vector2.Lerp(start, end, interp).ToPoint();

                // Direction (optional): lock once it starts extending to avoid jitter.
                if (interp < 0.2f)
                    NPC.direction = Math.Sign(tendrilTarget.X - NPC.Center.X);

                NPC.spriteDirection = NPC.direction;

                // End after full curve completes (i.e., after retraction finishes).
                if (t >= 1f)
                {
                    Time = 0;
                    t = 0f;
                    shouldTendrilExist = false;
                }
            }

            else
            {
                NPC.velocity.X = NPC.DirectionTo(Main.LocalPlayer.Center).X;

            }
            Collision.StepUp(ref NPC.position, ref NPC.velocity, (int)NPC.Size.X, (int)NPC.Size.Y, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }

        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (shouldTendrilExist)
            {
                if (victimHitbox.Intersects(TendrilTipHitbox))
                {

                    damageMultiplier *= 1.4f;
                    return false;

                }

            }


            return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        }

        // Walk: frames 0–6
        const int walkStart = 0;
        const int walkEnd = 6;

        // Attack
        const int AttackStartFrame = 8;      // windup
        const int AttackLoopStart = 11;      // looping extension
        const int AttackLoopEnd = 14;        // inclusive


        public override void FindFrame(int frameHeight)
        {
            // Advance frame timer
            NPC.frameCounter++;

            // -------------------------------
            // ATTACK ANIMATION (OVERRIDES)
            // -------------------------------
            if (shouldTendrilExist)
            {
                // Windup phase (first ~15 ticks)
                if (Time < 3)
                {
                    NPC.frame.Y = AttackStartFrame;
                }
                // Looping attack phase
                else
                {
                    int attackFrameCount = AttackLoopEnd - AttackLoopStart + 1;

                    if (NPC.frameCounter >= 5) // animation speed
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y++;

                        if (NPC.frame.Y < AttackLoopStart || NPC.frame.Y > AttackLoopEnd)
                            NPC.frame.Y = AttackLoopStart;
                    }
                }

                return;
            }

            // -------------------------------
            // WALK / IDLE
            // -------------------------------
            if (Math.Abs(NPC.velocity.X) > 0.2f)
            {
                if (NPC.frameCounter >= 7)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y++;

                    if (NPC.frame.Y < walkStart || NPC.frame.Y > walkEnd)
                        NPC.frame.Y = walkStart;
                }
            }
            else
            {
                // Idle frame (first walk frame)
                NPC.frame.Y = walkStart;
                NPC.frameCounter = 0;
            }
        }

        private static void DrawRopePrimitive(
    SpriteBatch spriteBatch,
    Vector2[] points,
    Func<float, float> widthFunc,
    Color color)
        {
            if (points.Length < 2)
                return;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[points.Length * 2];
            short[] indices = new short[(points.Length - 1) * 6];

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 dir;
                if (i == points.Length - 1)
                    dir = points[i] - points[i - 1];
                else
                    dir = points[i + 1] - points[i];

                dir = dir.SafeNormalize(Vector2.UnitX);
                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float progress = i / (float)(points.Length - 1);
                float halfWidth = widthFunc(progress) * 0.5f;

                Vector2 offset = normal * halfWidth;

                

                vertices[i * 2 + 0] = new VertexPositionColorTexture(
                    new Vector3(points[i] + offset, 0f),
                   color,
                    new Vector2(progress, 0f));

                vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(points[i] - offset, 0f),
                    color,
                    new Vector2(progress, 1f));
            }

            for (int i = 0; i < points.Length - 1; i++)
            {
                int v = i * 2;
                int i6 = i * 6;

                indices[i6 + 0] = (short)(v);
                indices[i6 + 1] = (short)(v + 1);
                indices[i6 + 2] = (short)(v + 2);

                indices[i6 + 3] = (short)(v + 2);
                indices[i6 + 4] = (short)(v + 1);
                indices[i6 + 5] = (short)(v + 3);
            }

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            spriteBatch.End();

            BasicEffect effect = new(gd);
            effect.World = Matrix.Identity;
            effect.View = Main.GameViewMatrix.TransformationMatrix;
            effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                0, 1);

            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3);
            }

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            Texture2D tex = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;


            Rectangle frame = tex.Frame(1, 31, 0, NPC.frame.Y);
            Main.EntitySpriteDraw(tex, NPC.Center - screenPos,
                frame,
                drawColor,
                NPC.rotation,
                frame.Size()/2 + new Vector2(0,10),
                NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            );
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            if (shouldTendrilDraw)
            {
                Vector2[] points = new Vector2[TendrilRope.segments.Length];
                for (int i = 0; i < points.Length; i++)
                    points[i] = TendrilRope.segments[i].position - screenPos;

                DrawRopePrimitive(
                    spriteBatch,
                    points,
                    progress => MathHelper.Lerp(18f, 6f, progress),
                    Color.DarkRed
                );
            }
        }
    }
}
