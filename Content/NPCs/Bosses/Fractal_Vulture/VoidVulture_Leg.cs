using System.Runtime.CompilerServices;
using HeavenlyArsenal.Common.IK;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

public partial class voidVulture
{
    public struct voidVultureLeg(IKSkeleton skeleton)
    {
        public IKSkeleton Skeleton = skeleton;

        public Vector2 TargetPosition = Vector2.Zero;

        public Vector2 EndPosition = Vector2.Zero;
    }

    private void DrawLeg(ref voidVultureLeg limb, Color drawColor, SpriteEffects effects)
    {
        if (NPC.IsABestiaryIconDummy || limb.Skeleton.PositionCount < 0)
        {
            return;
        }

        for (var i = 0; i < limb.Skeleton.PositionCount - 1; i++)
        {
            Utils.DrawLine(Main.spriteBatch, limb.Skeleton.Position(i), limb.Skeleton.Position(i + 1), drawColor, drawColor, 10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLegState(ref voidVultureLeg limb, Vector2 basePos, float lerpSpeed, float anchorThreshold)
    {
        limb.EndPosition = Vector2.Lerp(limb.EndPosition, limb.TargetPosition, lerpSpeed);

        limb.Skeleton.Update(basePos, limb.EndPosition);
    }

    private void CreateLegs()
    {
        // Bends one way (works when head is on the right side)
        _rightLeg = new voidVultureLeg
        (
            new IKSkeleton
            (
                (66f, new IKSkeleton.Constraints
                {
                    MinAngle = MathHelper.ToRadians(70),
                    MaxAngle = MathHelper.ToRadians(130)
                }),
                (60, new IKSkeleton.Constraints()),
                (50f, new IKSkeleton.Constraints
                {
                    MinAngle = MathHelper.ToRadians(70),
                    MaxAngle = MathHelper.ToRadians(110)
                })
            )
        );

        _LeftLeg = new voidVultureLeg
        (
            new IKSkeleton
            (
                (66f, new IKSkeleton.Constraints
                {
                    MinAngle = MathHelper.ToRadians(70),
                    MaxAngle = MathHelper.ToRadians(130)
                }),
                (60, new IKSkeleton.Constraints()),
                (50f, new IKSkeleton.Constraints
                {
                    MinAngle = MathHelper.ToRadians(70),
                    MaxAngle = MathHelper.ToRadians(110)
                })
            )
        );

        _rightLeg.EndPosition = NPC.Center;
        _LeftLeg.EndPosition = NPC.Center;
    }
}