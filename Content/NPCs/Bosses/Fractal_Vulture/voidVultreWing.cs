using CalamityMod.InverseKinematics;
using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using HeavenlyArsenal.Content.Particles.Metaballs;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.voidVulture;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{

    public class voidVultureWing(float wingRotation, float wingFlapProgress, float wingActivationProgress, int time, float cachedStartRot)
    {
        public float WingRotation = wingRotation;
        public float WingFlapProgress = wingFlapProgress;
        public float WingActivationProgress = wingActivationProgress;
        public static float WingCycleTime = 47;
        
        public int Time = time;

        private PiecewiseCurve _flapCurve;
        private float _cachedStartRot = cachedStartRot;

        public static void FlapWings(voidVultureWing wing, float flapCompletion, float startingRotation)
        {
            if (wing._flapCurve == null || !startingRotation.Equals(wing._cachedStartRot))
            {
                wing._cachedStartRot = startingRotation;

                wing._flapCurve = new PiecewiseCurve()
                .Add(EasingCurves.Exp, EasingType.In, startingRotation + 2.3f, 0.5f, startingRotation)
                .Add(EasingCurves.Quadratic, EasingType.InOut, startingRotation + 1.86f, 0.6f)
                .Add(EasingCurves.Circ, EasingType.Out, startingRotation, 1f);
            }
            float previousWingRotation = wing.WingRotation;
            float t = flapCompletion % 1f;
            //Main.NewText(t);
            wing.WingRotation = wing._flapCurve.Evaluate(t);
            float wingSpeed = Math.Abs(previousWingRotation - wing.WingRotation);
           

        }

        public static void UpdateWings(voidVultureWing wing, NPC npc)
        {

            WingCycleTime = 100;
            wing.WingActivationProgress = float.Lerp(wing.WingActivationProgress, 1, 0.5f);
            float baseRotation = Math.Abs(npc.velocity.Y) * -0.02f;

            float flapCompletion = (float)wing.Time / WingCycleTime;
            FlapWings(wing, flapCompletion, baseRotation);
            wing.WingFlapProgress = (float)Math.Sin(wing.Time / 8f) * 1.15f - 0.75f;
            wing.Time++;
            if (wing.Time > WingCycleTime + 1)
                wing.Time = 0;


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
}