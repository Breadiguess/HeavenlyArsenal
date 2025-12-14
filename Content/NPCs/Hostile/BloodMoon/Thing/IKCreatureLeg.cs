using System.Runtime.CompilerServices;
using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Core.Systems;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Thing;

internal class IKCreatureLeg
{
    public NPC Creature;

    public float StepProgress;

    public int Index;

    public Vector2 TargetPosition;

    public Vector2 EndPosition;

    public IKCreatureLeg Sister;

    public IKSkeleton Skeleton;

    public Vector2? GrabPosition;

    public Vector2? PreviousGrabPosition;

    public float StepCooldown;

    public bool ShouldStep;

    public IKCreatureLeg(NPC Owner, int i, IKCreatureLeg sister)
    {
        Creature = Owner;
        Index = i;

        Skeleton = new IKSkeleton
        (
            (30, new IKSkeleton.Constraints
            {
                MinAngle = i % 2 == 0 ? MathHelper.ToRadians(70) : MathHelper.ToRadians(80),
                MaxAngle = i % 2 == 0 ? MathHelper.ToRadians(110) : MathHelper.ToRadians(120)
            }),
            (i < 2 ? 40f : 30f, new IKSkeleton.Constraints
            {
                MinAngle = i % 2 == 0
                    ? MathHelper.ToRadians(20)
                    : MathHelper.ToRadians(-160),
                MaxAngle = i % 2 == 0
                    ? MathHelper.ToRadians(160) // outward stretch
                    : MathHelper.ToRadians(-20) // outward stretch mirrored
            }),
            (i < 2 ? 67 : 55, new IKSkeleton.Constraints
            {
                MinAngle = i % 2 == 0 ? -MathHelper.Pi : MathHelper.ToRadians(-30),
                MaxAngle = i % 2 == 0 ? MathHelper.ToRadians(30) : MathHelper.Pi
            })
        );

        Sister = sister;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateLimbState(ref IKCreatureLeg IKCreatureLeg, Vector2 basePos, float lerpSpeed, float anchorThreshold, int i)
    {
        IKCreatureLeg.ShouldStep = false;

        if (IKCreatureLeg.GrabPosition.HasValue)
        {
            var bell = Convert01To010(IKCreatureLeg.StepProgress); //(float)Math.Sin((1 - IKCreatureLeg.StepProgress) * MathHelper.Pi);
            IKCreatureLeg.EndPosition = Vector2.Lerp(IKCreatureLeg.EndPosition, IKCreatureLeg.GrabPosition.Value, 0.2f) - new Vector2(0, 10) * bell;
        }

        Dust.NewDustPerfect(IKCreatureLeg.EndPosition, DustID.Cloud, Vector2.Zero);
        IKCreatureLeg.Skeleton.Update(basePos, IKCreatureLeg.EndPosition);
        //IKCreatureLeg.IsAnchored = Vector2.Distance(IKCreatureLeg.EndPosition, IKCreatureLeg.TargetPosition) < anchorThreshold;
        var maxDist = IKCreatureLeg.Skeleton._maxDistance * 0.9f;

        if (IKCreatureLeg.GrabPosition.HasValue)
        {
            if (Vector2.Distance(basePos, IKCreatureLeg.GrabPosition.Value) > maxDist)
            {
                IKCreatureLeg.ShouldStep = true;
            }
        }

        if (IKCreatureLeg.StepCooldown > 0)
        {
            IKCreatureLeg.StepCooldown--;
            IKCreatureLeg.ShouldStep = false;
        }

        if (IKCreatureLeg.ShouldStep && IKCreatureLeg.StepCooldown <= 0)
        {
            IKCreatureLeg.PreviousGrabPosition = IKCreatureLeg.GrabPosition;

            IKCreatureLeg.GrabPosition = FindNewGrabPoint(basePos, i);

            IKCreatureLeg.StepProgress = 1f; // start stride animation
            IKCreatureLeg.StepCooldown = 30;
        }
    }

    public Vector2 FindNewGrabPoint(Vector2 basePos, int index)
    {
        var maxDist = Creature.As<IkCreature>().Legs[index].Skeleton._maxDistance * 0.65f;

        var sideOffset = Math.Clamp(Creature.velocity.X * 40f, -100, 100);

        //Main.NewText(sideOffset);
        var hit = LineAlgorithm.RaycastTo
        (
            Creature.Top,
            basePos + new Vector2(sideOffset * 1.25f, 115) //.RotatedBy((NPC.rotation + MathHelper.PiOver2)*0.5f)
        );

        if (!hit.HasValue)
        {
            return FindNewGrabPoint(basePos + new Vector2(Main.rand.Next(-1, 1), 0), index); // fallback
        }

        var world = hit.Value.ToWorldCoordinates();

        //Main.NewText(basePos - world);
        return world;
    }

    private bool IsLimbGrounded(IKCreatureLeg limb)
    {
        if (!limb.GrabPosition.HasValue)
        {
            return false;
        }

        if (limb.StepProgress > 0f)
        {
            return false; // stepping legs don't support body weight
        }

        var foot = limb.GrabPosition.Value;

        // Raycast a short distance downward
        var hit = LineAlgorithm.RaycastTo
        (
            foot,
            foot + new Vector2(0, 24f) // 24px downward tolerance
        );

        if (!hit.HasValue)
        {
            return false;
        }

        var t = Framing.GetTileSafely(hit.Value);

        return t.HasTile && Main.tileSolid[t.TileType];
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        //SpriteEffects flip = leftSet ? SpriteEffects.FlipVertically : SpriteEffects.None;
        for (var i = 0; i < Skeleton.PositionCount - 1; i++)
        {
            Utils.DrawLine(spriteBatch, Skeleton.Position(i), Skeleton.Position(i + 1), Color.Red, Color.Red, 4);
            //  Vector2 boneStart = Skeleton.GetBoneStartPosition(i);
            //  Vector2 boneEnd = Skeleton.GetBoneEndPosition(i);
            //  Vector2 boneOrigin = new Vector2(0, Skeleton.GetBoneLength(i) / 2);
            //  float boneRotation = boneStart.AngleTo(boneEnd) - MathHelper.PiOver2;
            //  spriteBatch.Draw(LimbAsset.Value, boneStart - screenPos, null, drawColor, boneRotation, boneOrigin, 1, flip, 0);
        }

        //spriteBatch.Draw(ForelimbAsset.Value, legOriginGraphic - screenPos, null, drawColor, legOriginGraphic.AngleTo(legKnee), forelegSpriteOrigin, 1, flip, 0);

        //spriteBatch.Draw(LimbAsset.Value, legKnee - screenPos, null, drawColor, legKnee.AngleTo(legTipGraphic), legSpriteOrigin, 1, flip, 0);
    }
}