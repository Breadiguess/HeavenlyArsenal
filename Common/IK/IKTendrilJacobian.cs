using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.IK
{
    public sealed class IKTendril
    {
        public IKSkeletonJacobian Skeleton;

        // Attachment
        public bool IsAttached;
        public Vector2 AnchorPoint;

        public float[] BaseLengths;
        public float ContractionRatio = 0.5f;

        // Muscle parameters
        public float RestLength;
        public float Stiffness = 0.15f;
        public float MaxForce = 60f;
        public float Activation; // 0..1

        // Output
        public Vector2 RootForce;

        public float TotalLength { get; }

        public IKTendril(IKSkeletonJacobian skeleton)
        {
            Skeleton = skeleton;

            float total = 0f;
            foreach (float l in skeleton.Lengths)
                total += l;

            TotalLength = total;
            RestLength = total;
            BaseLengths = skeleton.Lengths.ToArray();
        }

        public void Update(Vector2 rootPosition, float dt)
        {
            RootForce = Vector2.Zero;

            Skeleton.Root = rootPosition;


            for (int i = 0; i < Skeleton.JointCount; i++)
            {
                float contracted =
                    BaseLengths[i] * (1f - ContractionRatio * Activation);

                Skeleton.Lengths[i] = contracted;
            }

            if (!IsAttached)
                return;

            Skeleton.Solve(AnchorPoint, alpha: 0.002f);

            Vector2 root = Skeleton.Root;
            Vector2 tip = Skeleton.JointPositions[^1];

            Vector2 dir = (AnchorPoint - root).SafeNormalize(Vector2.Zero);

            float tension =
                Vector2.Distance(root, AnchorPoint);

            RootForce = dir * tension * 0.05f;


        }

        public void Attach(Vector2 desiredAnchor)
        {
            Vector2 root = Skeleton.Root;

            float maxReach = Skeleton.Lengths.Sum();

            Vector2 delta = desiredAnchor - root;
            float dist = delta.Length();

            if (dist > maxReach)
            {
                delta /= dist; // normalize
                AnchorPoint = root + delta * maxReach;
            }
            else
            {
                AnchorPoint = desiredAnchor;
            }

            IsAttached = true;
        }
     
        public void Detach()
        {
            IsAttached = false;
            RootForce = Vector2.Zero;
        }
    }
}
