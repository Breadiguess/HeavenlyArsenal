using HeavenlyArsenal.Common.IK;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    internal class ScavSona_IKArm
    {

       
        public static RenderTarget2D ScavSona_IKArm_Target;
        public Vector2[] StringPos;
        public Vector2[] StringVels;
        public IKSkeleton ArmSkeleton;
        public Vector2 EndPos;

        public static void UpdateArmString(ScavSona_IKArm arm)
        {
            if (arm.StringPos == null || arm.StringVels == null)
                return;

            int count = arm.StringPos.Length;
            if (count < 2)
                return;

            const float gravity = 0.7f;
            const float damping = 0.98f;
            const float segmentLength = 8f;

            arm.StringPos[0] = arm.ArmSkeleton.Position(arm.ArmSkeleton.PositionCount);
            arm.StringVels[0] = Vector2.Zero;

            for (int i = 1; i < count; i++)
            {
                arm.StringVels[i].Y += gravity;
                arm.StringVels[i] *= damping;
                arm.StringPos[i] += arm.StringVels[i];
            }

            for (int pass = 0; pass < 3; pass++)
            {
                arm.StringPos[0] = arm.EndPos;

                for (int i = 1; i < count; i++)
                {
                    Vector2 delta = arm.StringPos[i] - arm.StringPos[i - 1];
                    float dist = delta.Length();

                    if (dist <= 0.0001f)
                        continue;

                    float error = (dist - segmentLength) / dist;
                    Vector2 correction = delta * error;

                    arm.StringPos[i] -= correction;

                    if (i > 1)
                        arm.StringPos[i - 1] += correction * 0.5f;
                }
            }
        }
        public static void UpdateArmIK(ScavSona_IKArm arm, Vector2 StartPos, Vector2 DesiredPos, float lerpStrength = 0.2f)
        {
            if (arm == null)
                return;
            arm.EndPos = Vector2.Lerp(arm.EndPos, DesiredPos, lerpStrength);
            arm.ArmSkeleton.Update(StartPos, arm.EndPos);
        }
        public ScavSona_IKArm(Player Owner, IKSkeleton skeleton, bool ShouldHaveString = false, int string_length = 8)
        {
            ArmSkeleton = skeleton;
            EndPos = Owner.Center;
            if (ShouldHaveString)
            {
                StringPos = new Vector2[string_length];
                StringVels = new Vector2[string_length];
            }
        }
    }
}
