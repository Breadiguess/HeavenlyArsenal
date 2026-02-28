using HeavenlyArsenal.Common;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab.Butterflies;
using HeavenlyArsenal.Core.Systems;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab : BaseBloodMoonNPC, IMultiSegmentNPC
    {
        ExtraNPCSegment clawCutoffHitbox;
        ExtraNPCSegment clawHitRect;

        private List<ExtraNPCSegment> _ExtraHitBoxes = new();
        ref List<ExtraNPCSegment> IMultiSegmentNPC.ExtraHitBoxes()
        {
            return ref _ExtraHitBoxes;
        }
        public override int MaxBlood => 600;

        public override BloodMoonBalanceStrength Strength => new BloodMoonBalanceStrength(1, 1, 1);

        protected override void SetDefaults2()
        {
            NPC.width = 200;
            NPC.height = 75;
            NPC.damage = 200;
            NPC.defense = 130;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0.3f;

            clawCutoffHitbox = new ExtraNPCSegment(new Rectangle(), false, true, true, false);
            clawHitRect = new ExtraNPCSegment(new Rectangle(0, 0, 40, 40), true, false, false, false, 3000000);

            _ExtraHitBoxes.Add(clawCutoffHitbox);
            _ExtraHitBoxes.Add(clawHitRect);
        }

        public override void OnSpawn(IEntitySource source)
        {
            InitializeLegs();
            InitializeClaw();
            InitializeGunClaw();
            InitializeAttachPoints();

            DebugFillAttachPoints();
        }


        public override void SetStaticDefaults2()
        {

            Main.npcFrameCount[Type] = 1;
        }

        void DebugFillAttachPoints()
        {
            int Type = ModContent.NPCType<BloodCrab_Butterfly>();
            for(int i = 0; i< ButterflyAttachPoints.Length; i++)
            {
                if (ButterflyAttachPoints[i].Filled)
                    continue;

                NPC butterfly = NPC.NewNPCDirect(NPC.GetSource_FromThis(), ButterflyAttachPoints[i].Position, Type);

                butterfly.As<BloodCrab_Butterfly>().ParentID = NPC.whoAmI;
                butterfly.As<BloodCrab_Butterfly>().SocketIndex = i;
                butterfly.As<BloodCrab_Butterfly>().State = BloodCrab_Butterfly.ButterflyState.Attached;
                ButterflyAttachPoints[i].Filled = true;
                ButterflyAttachPoints[i].AttacheeIndex = butterfly.whoAmI;
            }
        }

        public override void AI()
        {
            float moveSpeed = 3.5f;
            float accel = 0.12f;

            // where we WANT to be going
            float desiredVelX =
                NPC.DirectionTo(Main.LocalPlayer.Center).X * moveSpeed;

            // steering force toward that velocity
            float steering =
                desiredVelX - NPC.velocity.X;

            // apply limited acceleration
            steering = MathHelper.Clamp(steering, -accel, accel);

            // now ADD — do not overwrite
            NPC.velocity.X += steering;
            NPC.spriteDirection = desiredVelX.NonZeroSign();

            ClawDesiredLoc = NPC.Center + new Vector2(-70, 70);
            StateMachine();

            if (claw is not null)
            {
                claw.Update(NPC.Center + new Vector2(-70, 20), ClawDesiredLoc);
            }

            if (GunArm is not null)
            {
                GunArm.Update(NPC.Center + new Vector2(70, 20), Main.MouseWorld);
            }
            // Tilt based on horizontal velocity only:
            // - referenceSpeed defines how fast it must move to reach full tilt.
            // - maxTilt limits the tilt angle (in radians).
            var referenceSpeed = 12f;
            var maxTilt = MathHelper.ToRadians(20f);
            var normalized = MathHelper.Clamp(NPC.velocity.X / referenceSpeed, -1f, 1f);
            var targetRotation = normalized * maxTilt;

            // Slightly lerp rotation toward the horizontal-velocity-based target.
            NPC.rotation = NPC.rotation.AngleLerp(targetRotation, 0.2f);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            Time++;

        }

        public override void PostAI()
        {
            EstimateSurfaceFrame(NPC.Center, out Vector2 normal, out Vector2 tangent);

            this.normal = normal;
            this.tangent = tangent;

            for (int i = 0; i < LimbOffsets.Length; i++)
            {
                ActualLimbOffsets[i] = LimbOffsets[i].RotatedBy(tangent.ToRotation() * 0.6f);
            }


            Vector2 delta = NPC.Center - _lastBodyPos;

            if (delta.LengthSquared() > 0.001f)
            {
                MotionIntent = Vector2.Lerp(
                    MotionIntent,
                    delta.SafeNormalize(Vector2.Zero),
                    0.25f
                );
            }

            _lastBodyPos = NPC.Center;
            BloodCrabLegUpdate();
            UpdateButterflyAttachPoints();

            float maxCheck = 170f;

            int hitCount = 0;
            float accumulatedHeight = 0f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 start = NPC.Center;
                Vector2 end = start + Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i / 3f - MathHelper.PiOver2 / 3f - NPC.rotation) * maxCheck;

                Point? hit = LineAlgorithm.RaycastTo(start, end, debug: false);

                if (!hit.HasValue)
                    continue;

                float height =
                    hit.Value.ToWorldCoordinates().Y - NPC.Center.Y;

                accumulatedHeight += height;
                hitCount++;
            }

            if (hitCount < 2)
            {
                NPC.noGravity = false;
                return;
            }

            float actualHeight = accumulatedHeight / hitCount;
            float desiredHeight = 135f;
            float tolerance = 1.5f;

            float error = desiredHeight - actualHeight;

            if (MathF.Abs(error) < tolerance)
            {
                NPC.velocity.Y = 0f;
                NPC.noGravity = true;
                return;
            }

            float correctionStrength = 0.07f;

            float moveAmount = error * correctionStrength;
            moveAmount = MathHelper.Clamp(moveAmount, -2f, 2f);

            NPC.position.Y -= moveAmount;
            NPC.noGravity = true;
            NPC.velocity.Y = 0f;
        }

    }
}

