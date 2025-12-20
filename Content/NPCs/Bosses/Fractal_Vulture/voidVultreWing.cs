using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.voidVulture;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

public class voidVultureWing(float wingRotation, float wingFlapProgress, float wingActivationProgress, int time, float cachedStartRot)
{
    public static float WingCycleTime = 47;

    public float WingRotation = wingRotation;

    public float WingFlapProgress = wingFlapProgress;

    public float WingActivationProgress = wingActivationProgress;

    public int Time = time;

    private PiecewiseCurve _flapCurve;

    private float _cachedStartRot = cachedStartRot;


        public const int WingSubdivisions = 24;

        public DynamicVertexBuffer WingVertexBuffer;
        public IndexBuffer WingIndexBuffer;

        public bool BuffersReady;

        public static void FlapWings(voidVultureWing wing, float flapCompletion, float startingRotation)
        {
            wing._flapCurve = new PiecewiseCurve()
                // Fast, powerful downstroke
                .Add(
                    EasingCurves.Cubic,
                    EasingType.Out,
                    startingRotation + MathHelper.ToRadians(100f),
                    0.22f,
                    startingRotation
                )

                // Brief compression / hold
                .Add(
                    EasingCurves.Linear,
                    EasingType.InOut,
                    startingRotation + MathHelper.ToRadians(55f),
                    0.62f
                )

                // Slow recovery (upstroke)
                .Add(
                    EasingCurves.Cubic,
                    EasingType.Out,
                    startingRotation,
                    1f
                );

            if (wing._flapCurve == null || !startingRotation.Equals(wing._cachedStartRot))
            {
                wing._cachedStartRot = startingRotation;

                
            }
            float previousWingRotation = wing.WingRotation;
            float t = flapCompletion % 1f;
            //Main.NewText(t);
            wing.WingRotation = wing._flapCurve.Evaluate(t);
            float wingSpeed = Math.Abs(previousWingRotation - wing.WingRotation);
           

        }

        public static void UpdateWings(voidVultureWing wing, NPC npc)
        {

            WingCycleTime = 80;
            wing.WingActivationProgress = float.Lerp(wing.WingActivationProgress, 1, 0.5f);
            float baseRotation = MathHelper.ToRadians(-50);//Math.Abs(npc.velocity.Y) * -0.02f;

            float flapCompletion = (float)wing.Time / WingCycleTime;
            FlapWings(wing, flapCompletion, baseRotation);
            wing.WingFlapProgress = (float)Math.Sin(wing.Time / 8f) * 1.15f - 0.75f;
            wing.Time++;
            if (wing.Time > WingCycleTime + 1)
                wing.Time = 0;

            if (flapCompletion %1 == 0)
            {
                //SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
            }
        }

        public void EnsureBuffers()
        {
            if (Main.netMode == NetmodeID.Server || BuffersReady)
                return;

            Main.QueueMainThreadAction(() =>
            {
                short[] indices = new short[(WingSubdivisions - 1) * 6];
                int idx = 0;

                for (short i = 0; i < WingSubdivisions - 1; i++)
                {
                    int v = i * 2;
                    indices[idx++] = (short)(v + 0);
                    indices[idx++] = (short)(v + 1);
                    indices[idx++] = (short)(v + 2);

                    indices[idx++] = (short)(v + 2);
                    indices[idx++] = (short)(v + 1);
                    indices[idx++] = (short)(v + 3);
                }

                WingIndexBuffer = new IndexBuffer(
                    Main.instance.GraphicsDevice,
                    IndexElementSize.SixteenBits,
                    indices.Length,
                    BufferUsage.WriteOnly
                );
                WingIndexBuffer.SetData(indices);

                WingVertexBuffer = new DynamicVertexBuffer(
                    Main.instance.GraphicsDevice,
                    typeof(VertexPositionColorTexture),
                    WingSubdivisions * 2,
                    BufferUsage.WriteOnly
                );

                BuffersReady = true;
            });
        }
        public void RegenerateVertices(Color DrawColor,
        Vector2 worldCenter,
        float wingRotation,
        bool flipped,
        float opacity)
        {
            if (!BuffersReady)
                return;

            VertexPositionColorTexture[] verts =
                new VertexPositionColorTexture[WingSubdivisions * 2];

            Vector2 size = new Vector2(210f, 120f);
            int vi = 0;
            float dir = flipped ? -1f : 1f;

            for (int x = 0; x < WingSubdivisions; x++)
            {
                float t = x / (float)(WingSubdivisions - 1);

                float z = t * -220f;
                float sideCurlFactor = -MathHelper.SmoothStep(0f, 0f, MathF.Pow(t, 1));

                Matrix flap = Matrix.CreateRotationZ(wingRotation * -dir);
                Matrix curl =
                    Matrix.CreateRotationY(
                        -wingRotation * sideCurlFactor * dir
                    ) *
                    Matrix.CreateRotationX(
                        -wingRotation * MathF.Pow(t, 0.9f) * 1.2f
                    );

                Matrix transform = curl * flap;

                //todo: WHEN WING ROTATION IS GREATER than some number, multiply size.x by -1, to make the wing curve a bit 


                float spanX = size.X * t  * dir;

                Vector3 top = new Vector3(
                    spanX,
                    size.Y * 0.6f,
                    z
                );

                Vector3 bottom = new Vector3(
                    spanX,
                    -size.Y * 0.5f,
                    z
                );


                Vector3 origin = new Vector3(worldCenter, 0f);

                top = Vector3.Transform(top,transform) *1.4f+ origin;
                bottom = Vector3.Transform(bottom, transform)*1.4f + origin;


                Vector2 uvTop = new Vector2(t, 1f);
                Vector2 uvBot = new Vector2(t, 0f);

                Color c = DrawColor * opacity;

                verts[vi++] = new VertexPositionColorTexture(top, c, uvTop);
                verts[vi++] = new VertexPositionColorTexture(bottom, c, uvBot);
            }
            //hate hate hate hate
            WingVertexBuffer.SetData(verts, SetDataOptions.Discard);
        }


        /*
        public NPC Owner;
        public List<Vector2[]> WingStrings;
        public IKSkeleton Skeleton;
        public float WingRotation;
        public int StrandAmount;
        public Vector2 TargetPosition;
        public Vector2 EndPos;
        public int Direction;

    public int Time;
    public voidVultureWing(NPC owner, int WingStringCount, int StrandAmount,  IKSkeleton skeleton, voidVultureWing pairedWing) : this(owner)
    {
        Skeleton = skeleton;
        WingStrings = new List<Vector2[]>(WingStringCount);
        this.StrandAmount = StrandAmount;
        for (int i = 0; i < WingStrings.Count; i++)
        {
            WingStrings.Add(new Vector2[StrandAmount]);
        }

    }

    public static void UpdateWing(voidVultureWing wing, Vector2 Position, Vector2 ParentVelocity)
    {
        wing.TargetPosition = Position + new Vector2(120 * wing.Direction + 50 * wing.Direction * MathF.Cos(wing.Time/10.1f), MathF.Sin(wing.Time / 10.1f)*20);
        wing.EndPos = Vector2.Lerp(wing.EndPos, wing.TargetPosition, 0.2f);
        Dust.NewDustPerfect(wing.TargetPosition, DustID.Cloud, Vector2.Zero);
        wing.Skeleton.Update(Position, wing.EndPos);

        wing.Time++;
    }


    private void UpdateWingStrings()
    {
        for(int x = 0; x < WingStrings.Count; x++)
        {
            for(int i = 0; i< WingStrings[x].Length-1; i++)
            {
                var strand = WingStrings[x][i];

                strand = Vector2.Lerp(strand, WingStrings[x][i + 1], 0.2f);
            }
        }
    }
    private void RenderWingStrings()
    {

    }
    public static void DrawWing(voidVultureWing wing, NPC npc, int offsetID, Color drawColor)
    {

        Texture2D wingtex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/WingTexture").Value;

        SpriteEffects flip = offsetID % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        float offset = offsetID % 2 == 0 ? 0 : wingtex.Width;
        Vector2 Origin = new Vector2(offset, wingtex.Height);

        float rot = wing.WingRotation * (offsetID % 2 == 0 ? 1 : -1);
        Vector2 Scale = new Vector2(1.75f, 1) * 1f;
        Vector2 DrawPos = voidVulture.wingPos[offsetID] + npc.Center - Main.screenPosition;
        //Main.EntitySpriteDraw(wingtex, DrawPos, null, Color.White * npc.Opacity, rot, Origin, Scale, flip);
        //Utils.DrawBorderString(spriteBatch, wing.Time.ToString() + $"\n" + Math.Round(wing.WingFlapProgress%1, 3).ToString(), DrawPos, Color.AliceBlue);


        drawColor = Color.White;
        for (int i = 0; i < wing.Skeleton.PositionCount - 1; i++)
            Utils.DrawLine(Main.spriteBatch, wing.Skeleton.Position(i), wing.Skeleton.Position(i + 1), drawColor, drawColor, 1);
        for (int i = 0; i < wing.Skeleton.JointCount; i++)
            Utils.DrawBorderString(Main.spriteBatch, i.ToString(), wing.Skeleton.Position(i) - Main.screenPosition, Color.Red);




    }*/
}