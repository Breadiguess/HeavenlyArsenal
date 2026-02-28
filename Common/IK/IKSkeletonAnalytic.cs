using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.IK
{
    public sealed class IKSkeletonAnalytic
    {
        public Vector2 Root;
        public Vector2 Joint;
        public Vector2 Tip;

        public float UpperLength;
        public float LowerLength;


        public void Solve(Vector2 target, Vector2 pole)
        {
            Vector2 AC = target - Root;
            float d = AC.Length();

            d = MathHelper.Clamp
            (
                d,
                MathF.Abs(UpperLength - LowerLength) + 0.001f,
                UpperLength + LowerLength - 0.001f
            );

            float cosTheta =
            (
                UpperLength * UpperLength +
                d * d -
                LowerLength * LowerLength
            ) /
            (
                2f * UpperLength * d
            );

            float theta =
                MathF.Acos(MathHelper.Clamp(cosTheta, -1f, 1f));

            Vector2 dir =
                AC.SafeNormalize(Vector2.UnitX);

            Vector2 perp =
                new Vector2(-dir.Y, dir.X);

            float side =
                MathF.Sign(Vector2.Dot(perp, pole));

            Vector2 jointDir =
                dir.RotatedBy(theta * side);

            Joint =
                Root + jointDir * UpperLength;

            Tip = target;
        }
    }
}
