using HeavenlyArsenal.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar
    {
        #region Limb System
        private const int LimbCount = 4;

        private RitualAltarLimb[] _limbs;
        private Vector2[] _limbBaseOffsets;
        bool IsFalling => NPC.velocity.Y > 1f;
        void ApplyStanceHeightAdjustment()
        {

            float totalError = 0f;
            int groundedCount = 0;
            for (int i = 0; i < LimbCount; i++)
            {
                var limb = _limbs[i];
                Vector2 basePos = NPC.Center + _limbBaseOffsets[i];

                if (!IsLimbGrounded(limb))
                    continue;

                float dist = Vector2.Distance(basePos, limb.EndPosition);

                float max = limb.skeletonMaxLength;
                float stance = max * 0.75f;

                totalError += (dist - stance);
                groundedCount++;
            }

            if (groundedCount == 0)
            {
                NPC.noGravity = false;
                return;
            }

            NPC.noGravity = true;
            float avgError = totalError / groundedCount;

            float correction = MathHelper.Clamp(avgError, -10f, 10f);

            float strength = 0.15f;

            //Main.NewText(correction * strength);

            NPC.position.Y += correction * strength;
        }
        Vector2 FindNewGrabPoint(Vector2 basePos, int index)
        {
            // Determine a base maximum reach from the skeleton
            float maxDist = _limbs[index].skeletonMaxLength * 0.75f;

            // Determine whether this leg origin is behind (+) or ahead (-) of the NPC center.
            float sideRelative = basePos.X - NPC.Center.X;

            // Scale factor from horizontal speed (0..1)
            float velocityFactor = MathHelper.Clamp(MathF.Abs(NPC.velocity.X) / 10f, 0f, 1f);

            // If the leg is behind the NPC, reduce forward reach when moving fast.
            // If the leg is ahead, allow it to reach further when moving fast.
            if (sideRelative > 0f)
            {
                // behind: more conservative reach, reduce with speed
                maxDist *= MathHelper.Lerp(0.5f, 1f, 1f - velocityFactor);
            }
            else
            {
                // ahead: allow an extended reach proportional to speed
                maxDist *= MathHelper.Lerp(1f, 1.5f, velocityFactor);
            }

            // Compute a horizontal offset for the raycast; bias it depending on whether the leg is behind/ahead.
            float bias = sideRelative > 0f ? 0.6f : 1.2f;
            float sideOffset = MathHelper.Clamp(NPC.velocity.X * 90f * bias, -250f, 250f);

            // Ensure the horizontal offset does not exceed the allowed maximum reach.
            sideOffset = MathHelper.Clamp(sideOffset, -maxDist*2, 2*maxDist);
            
            Point? hit = LineAlgorithm.RaycastTo(
                NPC.Top + new Vector2(0,-100).RotatedBy(NPC.rotation + MathHelper.PiOver2)+ _limbBaseOffsets[index],
                basePos + new Vector2(sideOffset, 300)//.RotatedBy((NPC.rotation + MathHelper.PiOver2)*0.5f)
            );

            if (!hit.HasValue)
                return NPC.Center + _limbBaseOffsets[index] + new Vector2(0, 80);// FindNewGrabPoint(basePos + new Vector2(Main.rand.Next(-1, 1), 0), index); // fallback

            if (!WorldGen.SolidTile(hit.Value))
                return FindFallingGrabPoint(basePos + new Vector2(Main.rand.Next(-1, 1), 0));

            Vector2 world = hit.Value.ToWorldCoordinates();

            for (int i = 0; i < LimbCount; i++)
            {
                if (world.Distance(_limbs[i].EndPosition) < 30)
                {
                    // _limbs[i].StepCooldown++;
                }
            }
            
            //Main.NewText(basePos - world);
            return world;
        }
        bool IsLimbGrounded(RitualAltarLimb limb)
        {
            if (!limb.GrabPosition.HasValue)
                return false;

            if (limb.StepProgress > 0f)
                return false; // stepping legs don't support body weight

            Vector2 foot = limb.GrabPosition.Value;

            // Raycast a short distance downward
            Point? hit = LineAlgorithm.RaycastTo(
                foot,
                foot + new Vector2(0, 24f) // 24px downward tolerance
            );

            if (!hit.HasValue)
                return false;

            Tile t = Framing.GetTileSafely(hit.Value);
            return t.HasTile && Main.tileSolid[t.TileType];
        }
        bool ShouldRelease(int limbIndex, RitualAltarLimb limb, Vector2 basePos)
        {

            // Never release if stepping
            if (limb.StepProgress > 0f)
                return false;
            Vector2 thing = NPC.Center + NPC.rotation.ToRotationVector2();
            //Main.NewText(thing.X - basePos.X);
            if (basePos.X < thing.X && !AngleBetween(NPC.rotation, MathHelper.ToRadians(-115), MathHelper.ToRadians(-60)))
            {
                //don't capsize, idiot
                //Main.NewText(!AngleBetween(NPC.rotation,  MathHelper.ToRadians(-135), MathHelper.ToRadians(-30)));
                //return true;
            }
            //float difference = limb.GrabPosition.Value.X - basePos.X;
            //difference *= NPC.velocity.X != 0 ? Math.Sign(NPC.velocity.X) : NPC.direction;

           
            // Must have a foothold to release FROM
            if (!limb.GrabPosition.HasValue)
                return true; // needs to find one immediately

            int opposite = GetOppositeLeg(limbIndex);
            var other = _limbs[opposite];

            // If the paired leg is NOT grounded, this leg must WAIT.
            if (!IsLimbGrounded(other))
            {

                return false;
            }



            float maxDist = limb.skeletonMaxLength * 0.87f;
            float dist = Vector2.Distance(basePos, limb.GrabPosition.Value);


            float tolerance = 0f;
            if (dist > maxDist + tolerance)
                return true;


            return false;
        }

        public static bool AngleBetween(float angle, float min, float max)
        {
            // Normalize all angles to [0, 2π)
            angle = MathHelper.WrapAngle(angle);
            if (angle < 0) angle += MathHelper.TwoPi;

            min = MathHelper.WrapAngle(min);
            if (min < 0) min += MathHelper.TwoPi;

            max = MathHelper.WrapAngle(max);
            if (max < 0) max += MathHelper.TwoPi;

            // If min <= max, it's a simple range
            if (min <= max)
                return angle >= min && angle <= max;

            // If min > max, the range wraps past 2π → 0
            return angle >= min || angle <= max;
        }

        Vector2 FindFallingGrabPoint(Vector2 basePos)
        {
            // Raycast straight down under the leg's origin
            Point? hit = LineAlgorithm.RaycastTo(
                basePos,
                basePos + new Vector2(0, 300f) // long ray down
            );

            if (hit.HasValue)
                return hit.Value.ToWorldCoordinates() + new Vector2(0, -8f);

            // If no ground found, keep leg fully extended downward
            return basePos + new Vector2(0, 250f);
        }
        int GetOppositeLeg(int i)
        {
            if (i == 0) return 2;
            if (i == 2) return 0;
            if (i == 1) return 3;
            if (i == 3) return 1;
            return i;
        }
        #endregion
    }
}
