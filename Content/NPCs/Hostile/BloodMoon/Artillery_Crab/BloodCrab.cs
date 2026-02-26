using HeavenlyArsenal.Core.Systems;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab : BaseBloodMoonNPC
    {

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab";


        public override int MaxBlood => 600;

        public override BloodMoonBalanceStrength Strength => new BloodMoonBalanceStrength(1, 1, 1);

        protected override void SetDefaults2()
        {
            NPC.width = 100;
            NPC.height = 75;
            NPC.damage = 200;
            NPC.defense = 130 / 2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0.3f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            InitializeLegs();
        }
        public override void SetStaticDefaults2()
        {
            
            Main.npcFrameCount[Type] = 13;
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
            StateMachine();

            // Tilt based on horizontal velocity only:
            // - referenceSpeed defines how fast it must move to reach full tilt.
            // - maxTilt limits the tilt angle (in radians).
            var referenceSpeed = 12f;
            var maxTilt = MathHelper.ToRadians(20f);
            var normalized = MathHelper.Clamp(NPC.velocity.X / referenceSpeed, -1f, 1f);
            var targetRotation = normalized * maxTilt;

            // Slightly lerp rotation toward the horizontal-velocity-based target.
            NPC.rotation = NPC.rotation.AngleLerp(targetRotation, 0.2f);

            Time++;
            //CurrentState = Behavior.SquidMissiles;
        }

        public override void PostAI()
        {
            EstimateSurfaceFrame(NPC.Center, out Vector2 normal, out Vector2 tangent);

            this.normal = normal;
            this.tangent = tangent;

            for(int i = 0; i< LimbOffsets.Length; i++)
            {
                ActualLimbOffsets[i] = LimbOffsets[i].RotatedBy(tangent.ToRotation()*0.6f);
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
            for (int i = 0; i < _bloodCrabLegs.Length; i++)
            {
                _bloodCrabLegs[i].Skeleton.Update(NPC.Center + ActualLimbOffsets[i], _bloodCrabLegs[i].PlantLocation);
            }

            float maxCheck = 160f;
            int hitCount = 0;
            float accumulatedHeight = 0f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 start = NPC.Center;
                Vector2 end = start + Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i/3f - MathHelper.PiOver2/3f - NPC.rotation) * maxCheck;

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
            float desiredHeight = 120f;
            float tolerance = 1.5f;

            float error = desiredHeight - actualHeight;

            if (MathF.Abs(error) < tolerance)
            {
                NPC.velocity.Y = 0f;
                NPC.noGravity = true;
                return;
            }

            float correctionStrength = 0.08f;

            float moveAmount = error * correctionStrength;
            moveAmount = MathHelper.Clamp(moveAmount, -2f, 2f);

            NPC.position.Y -= moveAmount;
            NPC.noGravity = true;
            NPC.velocity.Y = 0f;
        }



       
    }
}

