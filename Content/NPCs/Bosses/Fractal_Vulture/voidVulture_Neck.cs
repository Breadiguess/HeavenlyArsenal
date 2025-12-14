using System.Runtime.CompilerServices;
using HeavenlyArsenal.Common.IK;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

partial class voidVulture
{
    public struct voidVultureNeck(IKSkeleton skeleton, bool anchored = false, bool hasTarget = false)
    {
        public IKSkeleton Skeleton = skeleton;

        public Vector2 TargetPosition = Vector2.Zero;

        public Vector2 EndPosition = Vector2.Zero;

        public bool IsAnchored = anchored;
    }

    private void DrawArm(ref voidVultureNeck limb, Color drawColor, SpriteEffects effects)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            return;
        }

        for (var i = 0; i < limb.Skeleton.PositionCount - 1; i++)
        {
            Utils.DrawLine(Main.spriteBatch, limb.Skeleton.Position(i), limb.Skeleton.Position(i + 1), drawColor, drawColor, 5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLimbState(ref voidVultureNeck neck, Vector2 basePos, float lerpSpeed, float anchorThreshold)
    {
        neck.EndPosition = Vector2.Lerp(neck.EndPosition, neck.TargetPosition, lerpSpeed);

        // Decide which bend to use; this keeps the neck consistent whether its facing left or right
        var targetIsRight = neck.EndPosition.X >= basePos.X;
        neck.Skeleton = targetIsRight ? _neckRightSkeleton : _neckLeftSkeleton;
        neck.Skeleton.Update(basePos, neck.EndPosition);

        neck.IsAnchored = Vector2.Distance(neck.EndPosition, neck.TargetPosition) < anchorThreshold;
    }

    private void CreateLimbs()
    {
        // Bends one way (works when head is on the right side)
        _neckRightSkeleton = new IKSkeleton
        (
            (46f, new IKSkeleton.Constraints()),
            (60f, new IKSkeleton.Constraints
            {
                MinAngle = -MathHelper.Pi,
                MaxAngle = 0f
            })
        );

        // Bends the opposite way (for when head is on the left side)
        _neckLeftSkeleton = new IKSkeleton
        (
            (46f, new IKSkeleton.Constraints()),
            (60f, new IKSkeleton.Constraints
            {
                MinAngle = 0f,
                MaxAngle = MathHelper.Pi
            })
        );

        // Start with one of them – doesn’t really matter which
        Neck2 = new voidVultureNeck(_neckRightSkeleton);
    }
}