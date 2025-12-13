using CalamityMod.InverseKinematics;
using HeavenlyArsenal.Common.IK;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    partial class DebugNPC
    {
        internal record struct DebugNPCLimb(IKSkeleton Skeleton, bool anchored = false, bool hasTarget = false)
        {
            
            public float skeletonMaxLength => Skeleton._maxDistance;

            public IKSkeleton Skeleton = Skeleton;
            public Vector2 TargetPosition = Vector2.Zero;
            public Vector2 EndPosition = Vector2.Zero;
            public bool IsAnchored = anchored;

            public Vector2? GrabPosition;
            public Vector2? PreviousGrabPosition;
            public float StepProgress;
            public float StepCooldown;

            public bool ShouldStep;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateLimbState(ref DebugNPCLimb debugNPCLimb, Vector2 basePos, float lerpSpeed, float anchorThreshold, int i)
        {
            //Main.NewText(debugNPCLimb.GrabPosition.HasValue? debugNPCLimb.GrabPosition: "");

            debugNPCLimb.ShouldStep = ShouldRelease(i, debugNPCLimb, basePos);
            if (debugNPCLimb.GrabPosition.HasValue)
            {
                float bell = Convert01To010(debugNPCLimb.StepProgress);//(float)Math.Sin((1 - debugNPCLimb.StepProgress) * MathHelper.Pi);
                
                debugNPCLimb.EndPosition = Vector2.Lerp(debugNPCLimb.EndPosition, debugNPCLimb.GrabPosition.Value, 0.2f) - new Vector2(0, 10) * bell;
            }

            //Dust.NewDustPerfect(debugNPCLimb.EndPosition, DustID.Cloud, Vector2.Zero);
            debugNPCLimb.Skeleton.Update(basePos, debugNPCLimb.EndPosition);
            debugNPCLimb.IsAnchored = Vector2.Distance(debugNPCLimb.EndPosition, debugNPCLimb.TargetPosition) < anchorThreshold;

            float maxDist = debugNPCLimb.skeletonMaxLength * 0.9f; 
            if(debugNPCLimb.GrabPosition.HasValue)
            if (Vector2.Distance(basePos, debugNPCLimb.GrabPosition.Value) > maxDist)
                debugNPCLimb.ShouldStep = true;

            if (debugNPCLimb.StepCooldown > 0)
            {

                debugNPCLimb.StepCooldown--;
                debugNPCLimb.ShouldStep = false;
            }

            if (debugNPCLimb.ShouldStep && debugNPCLimb.StepCooldown<=0)
            {
                debugNPCLimb.PreviousGrabPosition = debugNPCLimb.GrabPosition;
                if (IsFalling)
                {
                    debugNPCLimb.GrabPosition = FindFallingGrabPoint(basePos);
                }
                else
                {
                    
                    debugNPCLimb.GrabPosition = FindNewGrabPoint(basePos, i);
                }
                debugNPCLimb.StepProgress = 1f; // start stride animation
                debugNPCLimb.StepCooldown = 30;
            }
        }

        void CreateLimbs()
        {
            NPC.rotation = MathHelper.PiOver2;  
            _limbs = new DebugNPCLimb[LimbCount];
            _limbBaseOffsets = new Vector2[LimbCount];
            float width = NPC.width * 5f;
            for (int i = 0; i < LimbCount; i++)
            {
                _limbBaseOffsets[i] = new Vector2(i % 2 == 0 ? -width : width, NPC.height / 2);
            }
            // Equidistant offsets around the bottom of the NPC
            
            _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 - 20);
            _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20);
            _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10);
            _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10);

            for (int i = 0; i < LimbCount; i++)
            {

                _limbs[i] = new DebugNPCLimb(
                    new IKSkeleton(
                        (30, new IKSkeleton.Constraints
                        {
                            MinAngle = i % 2 == 0?  MathHelper.ToRadians(70) : MathHelper.ToRadians(80),
                            MaxAngle = i % 2 == 0? MathHelper.ToRadians(110): MathHelper.ToRadians(120),
                        }),
                        (i < 2 ? 40f : 30f, new IKSkeleton.Constraints
                        {
                            // This bone bends downward from the hip joint
                            MinAngle = i % 2 == 0 ? MathHelper.ToRadians(20)  // inward limit
                            : MathHelper.ToRadians(-160),  // inward limit mirrored

                            MaxAngle = i % 2 == 0
                ? MathHelper.ToRadians(160)    // outward stretch
                : MathHelper.ToRadians(-20),   // outward stretch mirrored

                        }),
                        (i < 2 ? 67 : 55, new IKSkeleton.Constraints
                        {
                            MinAngle = i % 2 == 0 ? -MathHelper.Pi : MathHelper.ToRadians(-30),
                            MaxAngle = i % 2 == 0 ? MathHelper.ToRadians(30) : MathHelper.Pi
                        })
                    )
                );
                FindFallingGrabPoint(NPC.Center + _limbBaseOffsets[i]);
            }
        }
    }
}
