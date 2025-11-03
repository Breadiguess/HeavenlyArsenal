using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar
    {
        #region Limb System
        private const float LimbReach = 125f;
        private const float LimbSearchRadius = LimbReach * 0.8f;
        private const int LimbCount = 4;

        private RitualAltarLimb[] _limbs;
        private Vector2[] _limbBaseOffsets;
        private readonly HashSet<Point> _claimedTiles = new();


        private int _limbStepTimer;
        private bool _stepRightSide;
        private int _activeLimbIndex;
        private Vector2 _forward;
        private Vector2 _right;
        bool TouchingGround(Vector2 target)
        {
            Point tile = target.ToTileCoordinates();
            // Check the tile directly below the anchor point
            Point below = new Point(tile.X, tile.Y + 1);
            if (!WorldGen.InWorld(below.X, below.Y, 10))
                return false;

            Tile t = Framing.GetTileSafely(below);
            return t.HasTile && Main.tileSolid[t.TileType];
        }
        void updateGravity()
        {
            float Strength = 0f;
            for (int i = 0; i < _limbs.Length; i++)
            {
                if (_limbs[i].IsTouchingGround)
                {
                    Strength++;
                }
            }
            if (Strength > 0.2f)
                Strength /= LimbCount;
            NPC.velocity.Y = -Strength;// float.Lerp(NPC.velocity.Y, NPC.velocity.Y - Strength, 0.6f);
        }
        void UpdateLimbMotion()
        {
            UpdateLimbRhythm();
            UpdateLimbTargets();
        }
        void UpdateLimbTargets()
        {
            float speed = NPC.velocity.Length();
            const float baseReach = 90f;
            float reachRelax = baseReach * 1.1f;
            float forwardBias = MathHelper.Clamp(speed * 30f, 40f, 100f);
            float maxSearchDown = 120f;
            int holdTime = (int)MathHelper.Clamp(50f - speed * 10f, 20f, 50f);

            bool idle = speed < 0.1f;

            Vector2 moveDir = (speed > 0.1f) ? NPC.velocity.SafeNormalize(Vector2.UnitY) : Vector2.UnitY;

            bool grounded = false;
            int thing = 0;
            for (int i = 0; i < LimbCount; i++)
            {
                if (_limbs[i].IsTouchingGround)
                    thing++;

            }
            if (thing >= 1)
                grounded = true;

            for (int i = 0; i < LimbCount; i++)
            {
                ref var limb = ref _limbs[i];
                Vector2 basePos = NPC.Center + _limbBaseOffsets[i];

                if (limb.Cooldown > 0)
                    limb.Cooldown--;

                float dist = Vector2.Distance(basePos, limb.TargetPosition);
                bool aboutToOverstretch = dist > baseReach * 0.9f;
                bool tooFar = dist > reachRelax;
                if (idle)
                {
                    UpdateLimbState(ref limb, basePos, 0.15f + speed * 0.02f, 6f);
                    continue;
                }


                if ((aboutToOverstretch && limb.Cooldown <= 0) || tooFar)
                {
                    limb.IsAnchored = false;
                    limb.HasTarget = false;
                    limb.IsTouchingGround = false;
                }

                if (!limb.IsAnchored && limb.Cooldown <= 0)
                {
                    Vector2 probe = basePos + moveDir * forwardBias;
                    probe += Main.rand.NextVector2Circular(50f, 10f);

                    Point tile = probe.ToTileCoordinates();
                    bool found = false;

                    for (int y = 0; y < maxSearchDown / 16f; y++)
                    {
                        int ty = tile.Y + y;
                        if (!WorldGen.InWorld(tile.X, ty, 10))
                            break;


                        if (WorldGen.SolidTile(tile.X, ty, true))
                        {

                            Vector2 DesiredPosition = new Vector2(tile.X * 16, ty * 16 + 8);

                            int a = 0;
                            for (int x = 0; x < LimbCount; x++)
                            {
                                if (limb.TargetPosition.Distance(_limbs[x].TargetPosition) > 10 && _limbs[x].IsAnchored)
                                
                                    a++;

                            }
                            if (a >= 3)
                            {
                                limb.TargetPosition = DesiredPosition;
                                limb.HasTarget = true;
                                found = true;
                                limb.IsTouchingGround = true;
                                break;

                            }

                        }
                        else
                        {

                        }
                    }

                    if (!found)
                        SetFlailTarget(ref limb, basePos, baseReach, i);

                    limb.Cooldown = holdTime;
                }

                float followSpeed = MathHelper.Clamp(0.08f + speed * 0.02f, 0.08f, 0.25f);
                limb.EndPosition = Vector2.Lerp(limb.EndPosition, limb.TargetPosition, followSpeed);

               

                UpdateLimbState(ref limb, basePos, 0.15f + speed * 0.02f, 6f);
            }
        }
        void UpdateLimbRhythm()
        {
            //swap so that it looks different

            if (--_limbStepTimer <= 0)
            {
                _limbStepTimer = Main.rand.Next(25, 45);
                _activeLimbIndex = _stepRightSide ? 1 : 0;
                _stepRightSide = !_stepRightSide;
            }

            // Update direction vectors based on current movement or rotation
            _forward = NPC.velocity.SafeNormalize(Vector2.UnitY);
            _right = _forward.RotatedBy(MathHelper.PiOver2);
        }
        void SetFlailTarget(ref RitualAltarLimb limb, Vector2 basePos, float reach, int limbIndex)
        {
            float sway = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + limbIndex * 1.37f) * 12f;
            // Keep it under the base, within reach
            limb.TargetPosition = basePos + new Vector2(sway, Math.Min(reach * 0.9f, 54f + limbIndex * 5f));
            limb.IsAnchored = false;
            limb.HasTarget = false;
            limb.TargetTile = Point.Zero;
        }
        #endregion
    }
}
