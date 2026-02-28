using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.IK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HeavenlyArsenal.Common.IK.IKSkeleton;
using static System.Net.Mime.MediaTypeNames;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {
        public BloodCrabClaw claw;
        public Vector2 ClawDesiredLoc;

        public float ClampedClawOpenAmount => Math.Clamp(ClawOpenAmount, -0.2f,3);
        public float ClawOpenAmount = 0;
        private void InitializeClaw()
        {
            float scale = 4f;

            float[] lengths =
            {
                20f * scale,
                16f * scale,
                15f * scale
            };

            var jacobian = new IKSkeletonJacobian(Vector2.Zero, lengths);

            // Rest pose (slightly open claw)
            jacobian.RestAngles[0] = 0f;
            jacobian.RestAngles[1] = MathHelper.ToRadians(-25f);
            jacobian.RestAngles[2] = MathHelper.ToRadians(-35f);

            // Base joint – wide aim freedom
            jacobian.IsUnlimited[0] = true;

            jacobian.MinAngles[1] = MathHelper.ToRadians(-140f);
            jacobian.MaxAngles[1] = MathHelper.ToRadians(20f);

            jacobian.MinAngles[2] = -MathHelper.PiOver2;
            jacobian.MaxAngles[2] = MathHelper.PiOver2;

            claw = new BloodCrabClaw(NPC,clawHitRect, jacobian);
        }

        public class BloodCrabClaw(NPC npc,ExtraNPCSegment extraNPCSegment, IKSkeletonJacobian _skeleton)
        {
            public ExtraNPCSegment Collider = extraNPCSegment;
            public IKSkeletonJacobian Skeleton = _skeleton;
            public Vector2 Tip;
            public Vector2 DesiredLocation;
            public NPC Owner => npc;
            public BloodCrab bloodCrab => Owner.ModNPC as BloodCrab;

            public void Update(Vector2 root, Vector2 target, float interp = 0.2f)
            {
                Skeleton.Root = root;

                DesiredLocation = Vector2.Lerp(DesiredLocation, target, interp);

                Skeleton.Solve(DesiredLocation, iterations: 12, alpha: 0.006f);

                Tip = Skeleton.JointPositions[^1];

                if (Collider != null)
                {
                    Collider.Hitbox.Location =
                        (Tip - Collider.Hitbox.Size() / 2).ToPoint();
                }
            }

            public void DrawClaw()
            {
                Texture2D ClawBottom = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Artillery_Crab/TempClaw").Value;
                Texture2D ClawTop = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Artillery_Crab/TempClawTop").Value;

                float Rot = Tip.AngleFrom(Skeleton.JointPositions[Skeleton.JointCount - 1]);
                float TopRot = Rot - MathHelper.PiOver2 * bloodCrab.ClampedClawOpenAmount;
                Vector2 ClawtopOrigin = new Vector2(18, 46);
                Main.EntitySpriteDraw(ClawTop, Tip - Main.screenPosition, null, Color.White, TopRot, ClawtopOrigin, 1f, SpriteEffects.None);
                Main.EntitySpriteDraw(ClawBottom, Tip - Main.screenPosition, null, Color.White, Rot, ClawBottom.Size() / 2, 1f, SpriteEffects.None);


                // Also draw tip label
                Vector2 tipScreen = Skeleton.JointPositions[^1] - Main.screenPosition;
                Utils.DrawBorderString(Main.spriteBatch, "TIP", tipScreen, Color.Yellow);

                for (int i = 0; i < Skeleton.JointCount; i++)
                {
                    Vector2 start = Skeleton.JointPositions[i];
                    Vector2 end = Skeleton.JointPositions[i + 1];

                    NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(
                        Main.spriteBatch,
                        start,
                        end,
                        Color.White,
                        3
                    );


                    // --- Angle Debug ---
                    float angleRad = Skeleton.Angles[i];
                    float angleDeg = MathHelper.ToDegrees(angleRad);

                    string text = $"{i}\n{angleDeg:0.0}°";

                    Vector2 textPos = start - Main.screenPosition;

                    //Utils.DrawBorderString(Main.spriteBatch, text, textPos, Color.Red);
                }


            }

        }


    }
}
