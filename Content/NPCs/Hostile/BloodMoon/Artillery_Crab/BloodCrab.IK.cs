using CalamityEntropy.Content.ArmorPrefixes;
using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Core.Systems;
using Luminance.Core.Graphics;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab : BaseBloodMoonNPC
    {
        Vector2 _lastBodyPos;
        Vector2 MotionIntent = Vector2.UnitX;

        public Vector2[] LimbOffsets = new Vector2[4]
        {
            new Vector2(-60, -14),
            new Vector2(-40, 10),
            new Vector2(40, 10),
            new Vector2(60, -14)
        };
        public Vector2[] ActualLimbOffsets = new Vector2[4];
        public BloodCrabLeg[] _bloodCrabLegs = new BloodCrabLeg[4];

        public const int MAX_LIMBS = 4;

        public void InitializeLegs()
        {
            if (_bloodCrabLegs == null)
                _bloodCrabLegs = new BloodCrabLeg[4];

            for (int i = 0; i < MAX_LIMBS; i++)
            {
                float scale = i == 0 || i == 3 ? 4 : 3.6f;

                bool isLeft = i < MAX_LIMBS / 2;

                float side = isLeft ? 1f : -1f;

                IKSkeleton skeleton = new IKSkeleton
                (
                    ((7 * scale), new IKSkeleton.Constraints
                    {
                        MinAngle = i < 2 ? MathHelper.ToRadians(70) : MathHelper.ToRadians(80),
                        MaxAngle = i < 2 ? MathHelper.ToRadians(110) : MathHelper.ToRadians(120)
                    }),
                    ((17 * scale), new IKSkeleton.Constraints
                    {
                        // This bone bends downward from the hip joint
                        MinAngle = i < 2
                            ? MathHelper.ToRadians(12) // inward limit
                            : MathHelper.ToRadians(-174), // inward limit mirrored

                        MaxAngle = i < 2
                            ? MathHelper.ToRadians(160) // outward stretch
                            : MathHelper.ToRadians(-20) // outward stretch mirrored
                    }),
                    ((32 * scale), new IKSkeleton.Constraints
                    {

                        MinAngle = i < 2 ? -MathHelper.Pi : MathHelper.ToRadians(-40),
                        MaxAngle = i < 2 ? MathHelper.ToRadians(60) : MathHelper.Pi
                    })
                );

                _bloodCrabLegs[i] = new(skeleton, i);
            }

            _bloodCrabLegs[0].Phase = 0.5f;
            _bloodCrabLegs[1].Phase = 0f;
            _bloodCrabLegs[2].Phase = 0.5f;
            _bloodCrabLegs[3].Phase = 0.0f;
        }
        public sealed class BloodCrabLeg
        {
            public readonly IKSkeleton Skeleton;
            public readonly int Index;

            public Vector2 Tip => Skeleton.Position(Skeleton.PositionCount - 1);
            public Vector2 StepDestination;
            public Vector2 DefaultStepPosition;

            /// <summary>
            /// where the raycast says the foot SHOULD go
            /// </summary>
            public Vector2 DesiredLocation;

            /// <summary>
            /// where the foot is CURRENTLY planted
            /// </summary>
            public Vector2 PlantLocation;
            public Vector2 IdealPlantLocation;
            public Vector2 PreviousIdealPlantLocation;

            public Vector2 StepStartLocation;
            public float StepProgress;
            public bool IsStepping;


            public float Phase;
            public BloodCrabLeg(IKSkeleton skeleton, int index)
            {
                Skeleton = skeleton;
                Index = index;
            }
            public void DrawLeg(BloodCrabLeg leg, SpriteBatch spritebatch, Vector2 screenPos)
            {
                for (int i = 0; i < leg.Skeleton.PositionCount - 1; i++)
                {
                    Vector2 Start = leg.Skeleton.Position(i);
                    Vector2 End = leg.Skeleton.Position(i + 1);
                    NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(spritebatch, Start, End, Color.White, 5);
                }

                //NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(spritebatch, leg.Skeleton.Position(0), leg.PreviousIdealPlantLocation, Color.Purple, 5);

                Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;



                Utils.DrawBorderString(spritebatch, leg.Index.ToString(), leg.Skeleton.Position(0) - Main.screenPosition, Color.White);
            }
        }

        public record struct BloodCrabArm(IKSkeleton _skeleton, int _index)
        {
            public IKSkeleton Skeleton = _skeleton;
            public int Index = _index;
            public Vector2 Tip => Skeleton.Position(Skeleton.PositionCount - 1);
            public Vector2 DesiredLocation;
        }
        public void BloodCrabLegUpdate()
        {
            for (int i = 0; i < _bloodCrabLegs.Length; i++)
            {
                ref var limb = ref _bloodCrabLegs[i];
                bool isLeft = i < 2;
                float side = isLeft ? -1f : 1f;

                limb.Phase = i % 2 == 0 ? 0 : 0.5f;
                limb.Phase *= side;
                float lateralSpacing = 36f * (i == 0 || i == 3 ? 2f : 0.7f);
                float stepThreshold = 107f * (i == 0 || i == 3 ? 1.1f : 0.7f);

                float stepSpeed = 0.091f;
                float stepHeight = 36f;

                Vector2 hipOffset = new Vector2(side * lateralSpacing, 0f);
                Vector2 hip = NPC.Center + ActualLimbOffsets[i];
                float strideTime = 16f;

                Vector2 predictedFootfall =
                    hip +
                    NPC.velocity * strideTime;


                Vector2 v = Math.Abs(NPC.velocity.X) > 0.3f ? NPC.velocity : Vector2.Zero;
                if (v.LengthSquared() > 0.01f)
                {
                    MotionIntent = Vector2.Lerp(
                        MotionIntent,
                        v.SafeNormalize(Vector2.Zero),
                        0.1f
                    );
                }




               
                Vector2 desired = hip;

                Vector2 moveDir = MotionIntent.SafeNormalize(Vector2.UnitX * side);
                moveDir = (moveDir + Vector2.UnitY * 0.5f).SafeNormalize(Vector2.UnitY);

                Vector2 bestPoint = limb.DesiredLocation;
                float bestScore = float.MinValue;
                bool grounded = false;

                float maxReach = limb.Skeleton._maxDistance;

                for (int s = -6 + (int)side * 2; s <= 6 + (int)side * 2; s++)
                {
                    float forward = s * 16f;

                    Vector2 start =
                        hip + new Vector2(hipOffset.X + forward, -60f);

                    Vector2 end =
                        start +
                        Vector2.UnitY * 260f;

                    Point? ray = LineAlgorithm.RaycastTo(start, end, debug: false);// i == 1 || i == 3);

                    if (!ray.HasValue)
                        continue;

                    Vector2 candidate = ray.Value.ToWorldCoordinates();

                    float distance = hip.X - candidate.X;
                    Vector2 dir = (hip - candidate).SafeNormalize(Vector2.Zero);
                    float directionalScore = (dir.X * moveDir.X);

                    if (!limb.Skeleton.CanReachConstrained(hip, candidate))
                        continue;
                    
                    float distancePenalty = (distance / maxReach) * 0.7f;
                    float reachBonus = Vector2.Distance(candidate, hip) / 20;

                    float RejectCadidatesBeneathBody = 0f;

                    float bodyRejectThreshold = NPC.width * 0.76f;

                    if (Math.Abs(candidate.X - NPC.Center.X) < bodyRejectThreshold)
                    {
                        // Assign a very large penalty so this candidate is effectively invalidated.
                        RejectCadidatesBeneathBody = 1000f;
                    }

                    float separationPenalty = 0;

                    for (int j = 0; j < _bloodCrabLegs.Length; j++)
                    {
                        if (j == i)
                            continue;

                        if (_bloodCrabLegs[j].IsStepping)
                            continue;

                        Vector2 other = _bloodCrabLegs[j].Tip;
                        float d = Vector2.Distance(other, candidate);

                        const float PreferredSeparation = 32f;

                        if (d < PreferredSeparation)
                        {
                            float t = 1f - (d / PreferredSeparation);
                            separationPenalty += t * t;
                        }
                    }
                    separationPenalty *= 100;

                    float score = -directionalScore + distancePenalty / 2 + reachBonus * Convert01To010(s / 12f - moveDir.X) - RejectCadidatesBeneathBody - separationPenalty;
                    Color scoreColor = Color.Lerp(Color.Red, Color.LimeGreen, MathHelper.Clamp((score + 1f) * 0.5f, 0f, 1f));

                    if (false)//i == 1 || i == 3)
                        RayCastVisualizer.Texts.Add(new($"{limb.Index} \n {score.ToString("0.0")}" +
                            $"\n {directionalScore.ToString("0.0")}" +
                            $"\n {separationPenalty.ToString("0.0")}" +
                            $"\n {distancePenalty.ToString("0.0")}"
                            ,
                            candidate + new Vector2(0, limb.Index * 20),
                            scoreColor
                        ));
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPoint = candidate;
                        grounded = true;
                    }
                }

                if (grounded)
                    desired = bestPoint;
                else
                {
                    //assume that the limb has not found a location, and thus, we're probably falling.
                    return;
                }

                if (limb.PlantLocation == Vector2.Zero)
                {
                    Vector2 startPlant = grounded ? desired : hip + Vector2.UnitY * 40f;

                    limb.PlantLocation = startPlant;
                    limb.DefaultStepPosition = startPlant;
                    limb.StepDestination = startPlant;
                    limb.StepStartLocation = startPlant;
                    limb.StepProgress = 0f;
                    limb.IsStepping = false;
                }

                //Main.NewText($"{i}, Can solve: {limb.Skeleton.CanReachConstrained(limb.Skeleton.Position(0), limb.PlantLocation)}");
                // If we didn't find ground this frame, keep the foot planted and skip stepping logic.
                if (!grounded && limb.Skeleton.CanReachConstrained(limb.Skeleton.Position(0), desired))
                    continue;

                limb.DesiredLocation = desired;


                limb.DefaultStepPosition = Vector2.Lerp(
                    limb.DefaultStepPosition,
                    limb.DesiredLocation,
                    1f
                );

                //decide whether we can step based on the distance from the default
                float distFromPlantToDefault = Vector2.Distance(limb.Tip, limb.DefaultStepPosition);

                // Don't let all legs step simultaneously: require at least 2 feet planted.
                int groundedCount = 0;
                for (int j = 0; j < _bloodCrabLegs.Length; j++)
                    if (!_bloodCrabLegs[j].IsStepping)
                        groundedCount++;

                bool enoughSupport = groundedCount >= 2;

                float phaseTime = (Main.GameUpdateCount * 0.05f + limb.Phase) % 1f;
                bool inPhaseWindow = phaseTime < 0.5f;

                // Main step trigger condition
                bool shouldStep = (
                    !limb.IsStepping &&
                    enoughSupport &&
                    Math.Abs(NPC.velocity.X) > 0.4f &&
                    inPhaseWindow &&
                    distFromPlantToDefault > stepThreshold);

                if (shouldStep)
                {
                    limb.IsStepping = true;
                    limb.StepProgress = 0f;
                    limb.StepStartLocation = limb.PlantLocation;
                    limb.StepDestination = limb.DefaultStepPosition;
                }

                if (limb.IsStepping)
                {
                    limb.StepProgress += stepSpeed;
                    float t = MathHelper.Clamp(limb.StepProgress, 0f, 1f);

                    Vector2 flat = Vector2.Lerp(limb.StepStartLocation, limb.StepDestination, t);
                    float arc = MathF.Sin(t * MathF.PI) * stepHeight;

                    limb.PlantLocation = flat - Vector2.UnitY * arc;

                    if (t >= 1f)
                    {
                        PlayStepEffect(ref limb);
                        limb.PreviousIdealPlantLocation = limb.StepDestination;
                        limb.PlantLocation = limb.StepDestination;
                        limb.IsStepping = false;
                        limb.StepProgress = 0f;
                    }
                }


            }
        }

        void PlayStepEffect(ref BloodCrabLeg limb)
        {
            SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.BloodCrab.TempStep with { Pitch = -1, PitchVariance = 0.4f,Volume = 1, MaxInstances = 0 }, (NPC.Center + limb.PlantLocation) / 2).WithVolumeBoost(3);
            foreach (var player in Main.ActivePlayers)
            {
                if (!player.active || player.dead)
                {
                    continue;
                }


                // Get the player's center
                var playerPos = player.Center;

                var dist = Vector2.Distance(playerPos, limb.Tip);

                var maxRange = 1000f; // no shake beyond this
                var minRange = 100f; // full shake if closer than this

                if (dist < maxRange)
                {
                    var strength = 1f - MathHelper.Clamp((dist - minRange) / (maxRange - minRange), 0f, 1f);
                    strength = MathF.Pow(strength, 2f);
                    var shakeMagnitude = MathHelper.Lerp(0f, 3f, strength);

                    if (player.whoAmI == Main.myPlayer)
                    {
                        ScreenShakeSystem.StartShakeAtPoint
                        (
                            limb.PlantLocation,
                            shakeMagnitude,
                            shakeStrengthDissipationIncrement: 0.7f - strength * 0.01f
                        );
                    }
                }
            }
            var tile = Framing.GetTileSafely(limb.PlantLocation.ToTileCoordinates());
            for (var j = 0; j < 3; j++)
            {
                WorldGen.KillTile_MakeTileDust((int)limb.PlantLocation.ToTileCoordinates().X, (int)limb.PlantLocation.ToTileCoordinates().Y, tile);
            }
            //ModContent.GetInstance<TileDistortionMetaball>().CreateParticle(limb.PlantLocation, Vector2.Zero, 10f);
        }
      
       
        public Vector2 normal;
        public Vector2 tangent;
        public static bool EstimateSurfaceFrame(Vector2 origin, out Vector2 normal, out Vector2 tangent)
        {
            const int samples = 13;       // must be odd
            const float spacing = 32f;
            const float depth = 300f;

            int half = samples / 2;

            float sumX = 0f;
            float sumY = 0f;
            float sumXX = 0f;
            float sumXY = 0f;

            int valid = 0;

            for (int i = 0; i < samples; i++)
            {
                float x = (i - half) * spacing;

                Vector2 start = origin + new Vector2(x, -120);
                Vector2 end = start + Vector2.UnitY * depth;

                Point? hit = LineAlgorithm.RaycastTo(
                    start,
                    end,
                    ShouldCountWater: false,
                    debug: false);

                if (!hit.HasValue)
                    continue;

                Vector2 world = hit.Value.ToWorldCoordinates();

                float y = world.Y;

                sumX += x;
                sumY += y;
                sumXX += x * x;
                sumXY += x * y;

                valid++;
            }

            if (valid < 2)
            {
                tangent = Vector2.UnitX;
                normal = Vector2.UnitY;
                return false;
            }

            float denom = valid * sumXX - sumX * sumX;

            if (Math.Abs(denom) < 0.001f)
            {
                tangent = Vector2.UnitX;
                normal = Vector2.UnitY;
                return false;
            }

            float slope = (valid * sumXY - sumX * sumY) / denom;

            tangent = new Vector2(1f, slope).SafeNormalize(Vector2.UnitX);
            normal = new Vector2(-tangent.Y, tangent.X);

            if (normal.Y < 0)
                normal *= -1f;

            return true;
        }
    }
}
