using HeavenlyArsenal.Core.Systems;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar : BaseBloodMoonNPC
{
    #region Limb System

    private const int LimbCount = 4;

    private RitualAltarLimb[] _limbs;

    private Vector2[] _limbBaseOffsets;

    private bool IsFalling => NPC.velocity.Y > 1f;


    private Vector2 FindNewGrabPoint(Vector2 basePos, int index)
    {
        ref var limb = ref _limbs[index];

        float maxReach = limb.skeletonMaxLength;
        float bestScore = float.MinValue;
        Vector2 bestPoint = basePos + Vector2.UnitY * 80f;

        bool isLeft = basePos.X < NPC.Center.X;
        float side = isLeft ? -1f : 1f;

        Vector2 moveDir =
            Math.Abs(NPC.velocity.X) > 0.2f
            ? NPC.velocity.SafeNormalize(Vector2.UnitX * side)
            : Vector2.UnitX * side;

        float lateralSpacing = 36f;

        for (int s = -6; s <= 6; s++)
        {
            float forward = s * lateralSpacing * 0.5f;

            Vector2 rayStart =
                basePos + new Vector2(forward, -100f);

            Vector2 rayEnd =
                rayStart + Vector2.UnitY * 300f;

            Point? hit = LineAlgorithm.RaycastTo(rayStart, rayEnd, debug: false);
            if (!hit.HasValue)
                continue;

            Vector2 candidate = hit.Value.ToWorldCoordinates();



            // --------------------------------------------------
            // HARD BODY SIDE BARRIER
            // Limb is not allowed to select points
            // across the body's vertical midline.
            // --------------------------------------------------

            bool hipIsLeft = basePos.X < NPC.Center.X;
            bool candidateIsLeft = candidate.X < NPC.Center.X;

            if (hipIsLeft != candidateIsLeft)
            {
                continue; // immediately discard
            }

            // --- Constrained reach filter ---
            if (!limb.Skeleton.CanReachConstrained(basePos, candidate))
                continue;

            float dist = Vector2.Distance(candidate, basePos);
            float reachNorm = dist / maxReach;

            // --- Directional preference ---
            Vector2 dir = (candidate - basePos).SafeNormalize(Vector2.Zero);
            float directionalScore = Vector2.Dot(dir, moveDir);

            // --- Distance penalty ---
            float distancePenalty = reachNorm * 0.8f;

            float bodyRejectPenalty = 0f;

            // --- Radial exclusion field ---
            float bodyRadius = NPC.width * 0.55f;
            Vector2 bodyCenter = NPC.Center;

            // Offset slightly downward so feet don't clip underside
            bodyCenter.Y += NPC.height * 0.1f;

            float distToBody = Vector2.Distance(candidate, bodyCenter);

            // Hard reject if inside body radius
            if (distToBody < bodyRadius)
            {
                bodyRejectPenalty = 2000f;
            }

            // Extra penalty for directly beneath
            if (candidate.Y > NPC.Center.Y &&
                Math.Abs(candidate.X - NPC.Center.X) < bodyRadius * 0.8f)
            {
                bodyRejectPenalty += 2000f;
            }


            float separationPenalty = 0f;

            const float PreferredSeparation = 32f;

            for (int j = 0; j < LimbCount; j++)
            {
                if (j == index)
                    continue;

                if (_limbs[j].IsStepping)
                    continue;

                float d = Vector2.Distance(_limbs[j].PlantLocation, candidate);

                if (d < PreferredSeparation)
                {
                    float t = 1f - (d / PreferredSeparation);
                    separationPenalty += t * t;
                }
            }
          

            separationPenalty *= 120f;

            float score =
                directionalScore * 1.5f
                - distancePenalty
                - bodyRejectPenalty
                - separationPenalty;
            Color scoreColor = Color.Lerp(Color.Red, Color.LimeGreen, MathHelper.Clamp((score + 1f) * 0.5f, 0f, 1f));

            RayCastVisualizer.Texts.Add(new($"{index} \n {score.ToString("0.0")}"
                          
                          ,
                          candidate + new Vector2(0, index * 20),
                          scoreColor
                      ));
            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = candidate;
            }
        }

        return bestPoint;
    }


    public static bool AngleBetween(float angle, float min, float max)
    {
        // Normalize all angles to [0, 2π)
        angle = MathHelper.WrapAngle(angle);

        if (angle < 0)
        {
            angle += MathHelper.TwoPi;
        }

        min = MathHelper.WrapAngle(min);

        if (min < 0)
        {
            min += MathHelper.TwoPi;
        }

        max = MathHelper.WrapAngle(max);

        if (max < 0)
        {
            max += MathHelper.TwoPi;
        }

        // If min <= max, it's a simple range
        if (min <= max)
        {
            return angle >= min && angle <= max;
        }

        // If min > max, the range wraps past 2π → 0
        return angle >= min || angle <= max;
    }

  

    #endregion
}