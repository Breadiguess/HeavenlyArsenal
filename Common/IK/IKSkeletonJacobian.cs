using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.IK
{
    public sealed class IKSkeletonJacobian
    {
        public Vector2 Root;

        public float[] Lengths;
        public float[] Angles;

        public Vector2[] JointPositions;
        public float[] MinAngles;
        public float[] MaxAngles;
        public float[] RestAngles;
        public bool[] IsUnlimited;

        public int JointCount => Lengths.Length;
        public IKSkeletonJacobian(Vector2 root, float[] lengths)
        {
            Root = root;
            Lengths = lengths;
            Angles = new float[lengths.Length];
            JointPositions = new Vector2[lengths.Length + 1];


            MinAngles = new float[lengths.Length];
            MaxAngles = new float[lengths.Length];
            RestAngles = new float[lengths.Length];
            IsUnlimited = new bool[lengths.Length];
        }

        private void ForwardKinematics()
        {
            JointPositions[0] = Root;

            float totalAngle = 0f;

            for (int i = 0; i < JointCount; i++)
            {
                totalAngle += Angles[i];

                Vector2 dir = new Vector2(
                    MathF.Cos(totalAngle),
                    MathF.Sin(totalAngle)
                );

                JointPositions[i + 1] =
                    JointPositions[i] + dir * Lengths[i];
            }
        }
        public void Solve(Vector2 target, int iterations = 15, float alpha = 0.5f)
        {
            alpha *= 0.001f;
            for (int iter = 0; iter < iterations; iter++)
            {
                ForwardKinematics();

                Vector2 end = JointPositions[^1];
                Vector2 error = target - end;

                if (error.LengthSquared() < 0.001f)
                    return;

                for (int i = JointCount - 1; i >= 0; i--)
                {
                    Vector2 joint = JointPositions[i];
                    Vector2 toEnd = end - joint;

                    // 2D Jacobian column for revolute joint
                    Vector2 jacobianCol = new Vector2(-toEnd.Y, toEnd.X);

                    float gradient = Vector2.Dot(jacobianCol, error);

                    Angles[i] += alpha * gradient;

                    // Apply angle constraints

                    Angles[i] = MathHelper.WrapAngle(Angles[i]);
                    if (!IsUnlimited[i])
                    {
                        Angles[i] = MathHelper.Clamp(
                            Angles[i],
                            MinAngles[i],
                            MaxAngles[i]
                        );
                    }

                    ForwardKinematics();
                    end = JointPositions[^1];
                    error = target - end;
                }
            }
        }
    }
}
