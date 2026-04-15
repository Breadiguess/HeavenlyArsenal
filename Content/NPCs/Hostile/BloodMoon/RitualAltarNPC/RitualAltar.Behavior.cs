using HeavenlyArsenal.Core.Systems;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

public partial class RitualAltar : BaseBloodMoonNPC
{
    private const int LimbCount = 4;

    private RitualAltarLimb[] _limbs;

    private Vector2[] _limbBaseOffsets;

    private bool IsFalling => NPC.velocity.Y > 1f;

    private Vector2 FindNewGrabPoint(Vector2 basePos, int index)
    {
        if (Main.LocalPlayer.Distance(basePos) > 1700)
        {
            return Vector2.One;
        }

        ref var limb = ref _limbs[index];
        
        var maxReach = limb.skeletonMaxLength;
        var current = limb.PlantLocation;

        if (!limb.IsStepping && limb.Skeleton.CanReachConstrained(basePos, current))
        {
            var stability = Vector2.Distance(basePos, current) / maxReach;

            if (stability < 0.75f && !IsFalling)
            {
                return current;
            }
        }

        var bestScore = float.MinValue;
        var bestPoint = basePos + Vector2.UnitY * 80f;

        var isLeft = basePos.X < NPC.Center.X;
        var side = isLeft ? -1f : 1f;

        var predictedCenter = NPC.Center + NPC.velocity * 12f;
        var moveDir = (predictedCenter - NPC.Center).SafeNormalize(Vector2.UnitX * side);

        const int sampleCount = 18;
        
        var stride = 110f + Math.Abs(NPC.velocity.X) * 25f;

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)(sampleCount - 1);
            var forward = MathHelper.Lerp(-stride, stride, t);

            var climbHeight = 16f * 16f;
            var rayStart = basePos + new Vector2(forward, -climbHeight);
            var rayEnd = basePos + new Vector2(forward, 200f);

            var hit = LineAlgorithm.RaycastTo(rayStart, rayEnd, debug: false);

            if (!hit.HasValue)
            {
                continue;
            }

            var candidate = hit.Value.ToWorldCoordinates();
            
            var tile = hit.Value;
            
            if (WorldGen.SolidTile(tile.X, tile.Y))
            {
                var above = new Point(tile.X, tile.Y - 1);

                if (!WorldGen.SolidTile(above.X, above.Y))
                {
                    candidate = new Vector2(tile.X * 16f + 8f, tile.Y * 16f);
                }
            }
            
            var tileData = Main.tile[tile.X, tile.Y];

            if (tileData != null && tileData.HasTile)
            {
                candidate.Y = tile.Y * 16f;
            }

            var hipIsLeft = basePos.X < NPC.Center.X;
            var candidateIsLeft = candidate.X < NPC.Center.X;

            if (hipIsLeft != candidateIsLeft)
            {
                continue;
            }

            var dist = Vector2.Distance(candidate, basePos);
                
            var minReach = limb.skeletonMaxLength * 0.45f;

            if (dist < minReach || dist > maxReach)
            {
                continue;
            }

            var effectiveReach = maxReach;

            if (candidate.Y < basePos.Y)
            {
                effectiveReach *= 1.25f;
            }

            var reachNorm = dist / effectiveReach;

            if (!limb.Skeleton.CanReachConstrained(basePos, candidate))
            {
                continue;
            }

            var dir = (candidate - basePos).SafeNormalize(Vector2.Zero);
            var directionalScore = Vector2.Dot(dir, moveDir);

            var distancePenalty = reachNorm * reachNorm * 1.2f;

            var bodyRadius = NPC.width * 0.55f;

            var bodyCenter = NPC.Center;
            bodyCenter.Y += NPC.height * 0.1f;

            var distToBody = Vector2.Distance(candidate, bodyCenter);
            var bodyPenalty = 0f;

            if (distToBody < bodyRadius)
            {
                var field = 1f - MathHelper.Clamp(distToBody / bodyRadius, 0f, 1f);
                bodyPenalty = field * field * 8f;
            }

            if (candidate.Y > NPC.Center.Y && Math.Abs(candidate.X - NPC.Center.X) < bodyRadius * 0.8f)
            {
                bodyPenalty += 8f;
            }

            var separationPenalty = 0f;
            const float PreferredSeparation = 32f;

            for (var j = 0; j < LimbCount; j++)
            {
                if (j == index)
                {
                    continue;
                }

                if (_limbs[j].IsStepping)
                {
                    continue;
                }

                var d = Vector2.Distance(_limbs[j].PlantLocation, candidate);

                if (d < PreferredSeparation)
                {
                    var t2 = 1f - d / PreferredSeparation;
                    separationPenalty += t2 * t2;
                }
            }

            separationPenalty *= 120f;

            var heightPenalty = 0f;

            if (candidate.Y < NPC.Center.Y)
            {
                heightPenalty = (NPC.Center.Y - candidate.Y) * 0.02f;
            }

            var climbBonus = 0f;

            if (candidate.Y < basePos.Y - 4f)
            {
                climbBonus = 0.8f;
            }

            var score = directionalScore * 1.6f + climbBonus - distancePenalty - bodyPenalty - separationPenalty - heightPenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = candidate;

                if (score > 1.4f)
                {
                    break;
                }
            }
        }

        return bestPoint;
    }

    public static bool AngleBetween(float angle, float min, float max)
    {
        angle = WrapAngle(angle);

        if (angle < 0)
        {
            angle += TwoPi;
        }

        min = WrapAngle(min);

        if (min < 0)
        {
            min += TwoPi;
        }

        max = WrapAngle(max);

        if (max < 0)
        {
            max += TwoPi;
        }

        if (min <= max)
        {
            return angle >= min && angle <= max;
        }

        return angle >= min || angle <= max;
    }
}