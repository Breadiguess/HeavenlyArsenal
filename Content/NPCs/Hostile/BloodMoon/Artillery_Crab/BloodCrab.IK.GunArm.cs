using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.IK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab 
    {
        public BloodCrabClawGun GunArm;

        private void InitializeGunClaw()
        {
            float scale = 4f;

            float[] lengths =
            {
        20f * scale,
        16f * scale,
    };

            var jacobian = new IKSkeletonJacobian(Vector2.Zero, lengths);

            // Rest pose (slightly open claw)
            jacobian.RestAngles[0] = 0f;
            jacobian.RestAngles[1] = MathHelper.ToRadians(-25f);

            jacobian.IsUnlimited[0] = true;

            jacobian.MinAngles[1] = MathHelper.ToRadians(-140f);
            jacobian.MaxAngles[1] = MathHelper.ToRadians(20f);


             GunArm = new BloodCrabClawGun(clawHitRect, jacobian);
        }

        public sealed class BloodCrabClawGun(ExtraNPCSegment extraNPCSegment, IKSkeletonJacobian _skeleton)
        {
            public ExtraNPCSegment Collider = extraNPCSegment;
            public IKSkeletonJacobian Skeleton = _skeleton;
            public Vector2 Tip;
            public Vector2 DesiredLocation;


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
                Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Artillery_Crab/TempGunClaw").Value;
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

                    string text = $"{angleDeg:0.0}°";

                    Vector2 textPos = start - Main.screenPosition;

                    //Utils.DrawBorderString(Main.spriteBatch, text, textPos, Color.Red );
                }

                float Rot = Tip.AngleFrom(Skeleton.JointPositions[Skeleton.JointCount - 1]);
                Main.EntitySpriteDraw(tex, Tip - Main.screenPosition, null, Color.White, Rot, tex.Size() / 2, 1f, SpriteEffects.None);


                // Also draw tip label
                Vector2 tipScreen = Skeleton.JointPositions[^1] - Main.screenPosition;
                //Utils.DrawBorderString(Main.spriteBatch, "TIP", tipScreen, Color.Yellow);
            }

        }
    }
}
