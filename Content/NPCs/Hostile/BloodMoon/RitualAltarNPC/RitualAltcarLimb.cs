using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Thing;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using static HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.DebugNPC;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    //IM SORRY WAAAAAAH
    //credit: https://github.com/mayli4/AllBeginningsMod/blob/main/src/AllBeginningsMod/Content/Bosses/_Nightgaunt/NightgauntNPC.Limbs.cs
    // thanks bozo :3
    internal partial class RitualAltar
    {
        public class RitualAltarLimb
        {
            public RitualAltarLimb Sister;
            public RitualAltarLimb Paired;
            public RitualAltarLimb(IKSkeleton skeleton,RitualAltarLimb sister, RitualAltarLimb paired, bool anchored = false, bool hasTarget = false)
            {
                Skeleton = skeleton;
                IsAnchored = anchored;
                Sister = sister;
                Paired = paired;
            }
            public float skeletonMaxLength => Skeleton._maxDistance;

            public IKSkeleton Skeleton;
            public Vector2 TargetPosition = Vector2.Zero;
            public Vector2 EndPosition = Vector2.Zero;
            public bool IsAnchored;

            public Vector2 DesiredGrabPosition;
            public Vector2? GrabPosition;
            public Vector2? PreviousGrabPosition;
            public float StepProgress;
            public float StepCooldown;

            public bool ShouldStep;
        }
       


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateLimbState(ref RitualAltarLimb limb, Vector2 basePos, float lerpSpeed, float anchorThreshold, int i)
        {

            limb.ShouldStep = false;

            if (limb.GrabPosition.HasValue)
            {
                float bell = Convert01To010(limb.StepProgress);//(float)Math.Sin((1 - limb.StepProgress) * MathHelper.Pi);
                if (limb.GrabPosition.Value.Distance(limb.PreviousGrabPosition.Value) < 3)
                    bell = 0;
                limb.EndPosition = Vector2.Lerp(limb.EndPosition, limb.GrabPosition.Value, 0.2f) - new Vector2(0, 10) * bell;
            }

            //Dust.NewDustPerfect(limb.EndPosition, DustID.Cloud, Vector2.Zero);
            limb.Skeleton.Update(basePos, limb.EndPosition);
            limb.IsAnchored = Vector2.Distance(limb.EndPosition, limb.TargetPosition) < anchorThreshold;

            float maxDist = limb.skeletonMaxLength * 0.9f;
            if (limb.GrabPosition.HasValue)
                if (Vector2.Distance(basePos, limb.GrabPosition.Value) > maxDist)
                    limb.ShouldStep = true;

            if (limb.StepCooldown > 0)
            {

                limb.StepCooldown--;
                limb.ShouldStep = false;
            }

            if (limb.ShouldStep && limb.StepCooldown <= 0)
            {
                limb.PreviousGrabPosition = limb.GrabPosition;
                if (IsFalling)
                {
                    limb.GrabPosition = FindFallingGrabPoint(basePos);
                }
                else
                {

                    limb.GrabPosition = FindNewGrabPoint(basePos, i);
                }
                limb.StepProgress = 1f; // start stride animation
                limb.StepCooldown = 30;
            }
        }


        void CreateLimbs()
        {
            _limbs = new RitualAltarLimb[LimbCount];
            _limbBaseOffsets = new Vector2[LimbCount];

            // Equidistant offsets around the bottom of the NPC
            float width = NPC.width * 0.3f;
            _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 -  20);
            _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20);
            _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10);
            _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10);

          
            for (int i = 0; i < LimbCount; i++)
            {
                int set = i < 2 ? 0 : 2;
                int otherSisterOffset = i % 2 == 0 ? 1 : 0;
                int pairedleg = i == 3 ? 0 : (i == 0 ? 3 : (i == 1 ? 2 : 1));

                _limbs[i] = new RitualAltarLimb(
                    new IKSkeleton(
                        (36f, new IKSkeleton.Constraints()),
                        (60f, new IKSkeleton.Constraints
                        {
                            MinAngle = i % 2 == 0 ? -MathHelper.Pi : 0,
                            MaxAngle = i % 2 == 0 ? 0 : MathHelper.Pi
                        })
                    ), default, default
                );
                _limbs[i].TargetPosition = NPC.Center + _limbBaseOffsets[i] + new Vector2(0, 40);
                _limbs[i].EndPosition = _limbs[i].TargetPosition;
            }
            for (int i = 0; i < 4; i++)
            {
                int set = i < 2 ? 0 : 2;
                int otherSisterOffset = i % 2 == 0 ? 1 : 0;
                int pairedleg = i == 3 ? 0 : (i == 0 ? 3 : (i == 1 ? 2 : 1));

                _limbs[i].Paired = _limbs[pairedleg];
                _limbs[i].Sister = _limbs[set + otherSisterOffset];
            }

        }
    }
}
    
