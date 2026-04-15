using System.Runtime.CompilerServices;
using HeavenlyArsenal.Common.IK;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

//IM SORRY WAAAAAAH
//credit: https://github.com/mayli4/AllBeginningsMod/blob/main/src/AllBeginningsMod/Content/Bosses/_Nightgaunt/NightgauntNPC.Limbs.cs
// thanks bozo :3
public partial class RitualAltar
{
    public sealed class RitualAltarLimb
    {
        public bool Initialized;
        
        public RitualAltarLimb Sister;
        public RitualAltarLimb Paired;

        public IKSkeleton Skeleton;
        public Vector2 DesiredLocation;     // foothold planner output
        public Vector2 PlantLocation;       // locked stance foot
        public Vector2 StepStartLocation;
        public Vector2 StepDestination;

        public float StepProgress;
        public bool IsStepping;

        public float Phase;

        public float skeletonMaxLength => Skeleton._maxDistance;

        public RitualAltarLimb(IKSkeleton skeleton, RitualAltarLimb sister, RitualAltarLimb paired, bool anchored = false, bool hasTarget = false)
        {
            Skeleton = skeleton;
            Sister = sister;
            Paired = paired;
        }
    }

    private void UpdateLimbState(ref RitualAltarLimb limb, Vector2 basePos, int i)
    {
        var stepThreshold = limb.skeletonMaxLength * 0.65f;
        var desired = FindNewGrabPoint(basePos, i);

        if (!limb.Skeleton.CanReachConstrained(basePos, desired))
        {
            return;
        }

        limb.DesiredLocation = desired;

        if (!limb.Initialized)
        {
            limb.Initialized = true;
            limb.PlantLocation = desired;
            limb.StepDestination = desired;
            limb.StepStartLocation = desired;
            limb.IsStepping = false;
            limb.StepProgress = 0f;
        }

        var dist = Vector2.Distance(limb.PlantLocation, limb.DesiredLocation);

        if (dist < 6f)
        {
            limb.Skeleton.Update(basePos, limb.PlantLocation);
            return;
        }

        var groundedCount = 0;

        for (var j = 0; j < LimbCount; j++)
        {
            if (!_limbs[j].IsStepping)
            {
                groundedCount++;
            }
        }

        var enoughSupport = groundedCount >= 2;

        var phaseTime = (Main.GameUpdateCount * 0.05f + limb.Phase) % 1f;
        var inPhaseWindow = phaseTime < 0.5f;

        var blocked = Math.Abs(NPC.velocity.X) < 0.1f && dist > stepThreshold * 0.6f;

        if (!limb.IsStepping && enoughSupport && (inPhaseWindow || blocked) && dist > stepThreshold)
        {
            limb.IsStepping = true;
            limb.StepProgress = 0f;
            limb.StepStartLocation = limb.PlantLocation;
            limb.StepDestination = limb.DesiredLocation;
        }

        if (limb.IsStepping)
        {
            var stepSpeed = MathHelper.Lerp(0.08f, 0.16f, dist / limb.skeletonMaxLength);
            var stepHeight = MathHelper.Lerp(18f, 34f, dist / limb.skeletonMaxLength);

            limb.StepProgress += stepSpeed;

            var t = MathHelper.Clamp(limb.StepProgress, 0f, 1f);

            var flat = Vector2.Lerp(limb.StepStartLocation, limb.StepDestination, t);
            var arc = MathF.Sin(t * MathF.PI) * stepHeight;

            limb.PlantLocation = flat - Vector2.UnitY * arc;

            if (t >= 1f)
            {
                limb.PlantLocation = limb.StepDestination;
                limb.IsStepping = false;
                limb.StepProgress = 0f;
            }
        }

        limb.Skeleton.Update(basePos, limb.PlantLocation);
    }
    
    private void CreateLimbs()
    {
        _limbs = new RitualAltarLimb[LimbCount];
        _limbBaseOffsets = new Vector2[LimbCount];

        // Equidistant offsets around the bottom of the NPC
        var width = NPC.width * 0.3f;
        _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 - 20);
        _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20);
        _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10);
        _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10);

        for (var i = 0; i < LimbCount; i++)
        {
            var set = i < 2 ? 0 : 2;
            var otherSisterOffset = i % 2 == 0 ? 1 : 0;
            var pairedleg = i == 3 ? 0 : i == 0 ? 3 : i == 1 ? 2 : 1;

            _limbs[i] = new RitualAltarLimb
            (
                new IKSkeleton
                (
                    (46f, new IKSkeleton.Constraints()),
                    (70f, new IKSkeleton.Constraints
                    {
                        MinAngle = i % 2 == 0 ? -MathHelper.Pi : 0,
                        MaxAngle = i % 2 == 0 ? 0 : MathHelper.Pi
                    })
                ),
                default,
                default
            );
            _limbs[i].Phase = i / (float)LimbCount;

        }
    }
}