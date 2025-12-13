using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    namespace CalamityFables.Content.Boss.MushroomCrabBoss
    {
        public partial class RitualAltar : ModNPC
        {
            public class RitualAltarLimb
            {
                public float maxLenght = 375;
                public bool latchedOn = false;

                public RitualAltarLimb pairedLeg;
                public RitualAltarLimb sisterLeg;

             
                public bool limping;

                public bool frontPair;
                public bool leftSet;

                public Vector2 legOrigin;
                public Vector2 legKnee;
                public Vector2 legTip;

                public Vector2 legTipGraphic;
                public Vector2 legOriginGraphic;

                public float grabDelay = 0;
                public float stepTimer = 0;
                public float strideTimer = 0;
                public float fallTime = 0f;

                /// <summary>
                /// The absolutely ideal grab position of the leg
                /// </summary>
                public Vector2 desiredGrabPosition;
                /// <summary>
                /// The best grab position we found
                /// </summary>
                public Vector2? grabPosition;
                /// <summary>
                /// The best grab position we found
                /// </summary>
                public Vector2? previousGrabPosition;
                /// <summary>
                /// The best grab position we found
                /// </summary>
                public Point? grabTile
                {
                    get
                    {
                        if (grabPosition != null)
                            return grabPosition.Value.ToTileCoordinates();
                        return null;
                    }
                }

                public float ForelegLenght => 87.5f * NPC.scale;
                public float LegLenght => 175f * NPC.scale;

                public RitualAltar crabulon;
                public NPC NPC => crabulon.NPC;
                public float baseRotation;

                public bool playedStepEffects = true; //If it needs to play its stepping sound
                public float stepEffectForce = 1f; //Volume of the stepping sound when played. Amps up the longer the foot is left in the air

                public int Direction => leftSet ? -1 : 1;

                public float SisterInfluence => sisterLeg.latchedOn ? sisterLeg.stepTimer : 1;

                public RitualAltarLimb(RitualAltar crabulon, bool frontPair, bool leftSet, float baseRotation)
                {
                    this.crabulon = crabulon;
                    this.frontPair = frontPair;
                    this.leftSet = leftSet;
                    this.baseRotation = baseRotation;

                    legOrigin = GetLegOrigin();
                    legKnee = legOrigin + Vector2.UnitY * ForelegLenght;
                    legTip = legKnee + Vector2.UnitY * LegLenght;
                    legTipGraphic = legTip;
                    legOriginGraphic = legOrigin + crabulon.visualOffset;

                    //Set skin
                    spriteSizeMultiplier = skin.spriteSizeMultiplier;

                    forelegSpriteOrigin = skin.forelegOrigin;
                    if (leftSet)
                        forelegSpriteOrigin.Y = ForelimbAsset.Value.Height - forelegSpriteOrigin.Y;
                    legSpriteOrigin = skin.legOrigin;
                    if (leftSet)
                        legSpriteOrigin.Y = LimbAsset.Value.Height - legSpriteOrigin.Y;
                }

                public void Update()
                {
                    NPC crabNPC = crabulon.NPC;
                    maxLenght = LegLenght + ForelegLenght;
                    Vector2 legDirection = (baseRotation + crabNPC.rotation).ToRotationVector2();

                    legOrigin = GetLegOrigin();
                    legOriginGraphic = legOrigin + crabulon.visualOffset;

                  

                    //Check if the leg is latched onto something based on if it's close enough to the grab position
                    latchedOn = false;
                    if (grabPosition != null && Vector2.Distance(legTip, grabPosition.Value) < 10f)
                    {
                        legTip = grabPosition.Value;
                        latchedOn = true;
                    }

                    UpdateDesiredGrabPosition(legDirection);
                    bool frontSet = Math.Sign(crabulon.NPC.velocity.X) == Direction;

                    //When grappled
                    if (latchedOn)
                    {
                        //Check if the leg is "uncomfortable" enough and release if it is
                        if (ShouldReleaseLeg(frontSet, out bool noStepDelay))
                        {
                            ReleaseGrip();
                            if (noStepDelay)
                                grabDelay = 0;
                        }

                        //Step effects
                        if (!playedStepEffects)
                        {
                            if (stepEffectForce > 0.5f)
                            {
                                //Step sound volume scales with how long the leg has been out in the air
                                float stepPitch = Utils.GetLerpValue(300f, 1000f, legTip.Distance(Main.LocalPlayer.Center), true) * 1f;
                                float stepVolume = Utils.GetLerpValue(0.5f, 1f, stepEffectForce);
                                SoundEngine.PlaySound(StepSound with { Volume = stepVolume * 0.4f, Pitch = stepPitch }, legTip);
                                Collision.HitTiles(legTip, Vector2.Zero, 9, 9);

                                //Screenshake if big enough
                                if (NPC.scale > 1.4f && CameraManager.Shake < 20f)
                                    CameraManager.Shake += 5f;
                            }
                            playedStepEffects = true;
                        }
                        stepEffectForce = 0f;
                        fallTime = 0f;

                        //Tick down the step timer (Controls the small ground stab motion when it finishes a new step)
                        stepTimer -= 1 / (60f * 0.3f);
                        if (stepTimer < 0)
                            stepTimer = 0;
                    }

                    //When free
                    else
                    {
                        //Check for a new position to latch on if we don't have one
                        if (grabPosition == null)
                            FindGrabPos();

                        //If we still don't have a valid grab position
                        if (grabPosition == null)
                        {
                            //Fall
                            if (crabNPC.velocity.Y > 2)
                            {
                                fallTime++;

                                //When falling, crabulon flails its legs around a point above its legs
                                Vector2 fallingPosition = desiredGrabPosition - Vector2.UnitY * 100f * NPC.scale;
                                Vector2 fallPositionOffset = new Vector2((float)Math.Sin(Main.GlobalTimeWrappedHourly * 30f) * 40f, 21f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 40f) * 70f);
                                fallingPosition += fallPositionOffset * NPC.scale;

                                //Back set of leg is a bit more retracted towards crabulon's center
                                if (!frontPair)
                                    fallingPosition.X -= Direction * 30f * NPC.scale;

                                //Undo the lateral displacement of the desired grab position, the legs should be spread evenly
                                fallingPosition.X -= DesiredGrabPositionVelocityXOffset;

                                //Move towards the falling leg position
                                legTip = Vector2.Lerp(legTip, fallingPosition, MathHelper.Min(1f, fallTime / 8f) * 0.1f);

                                //Entirely lose your previous grab position if falling for too long
                                if (fallTime > 10f)
                                    previousGrabPosition = null;
                            }

                            //Leg limps down
                            else
                            {
                                legTip.Y += 4.2f * NPC.scale;
                                if (FablesUtils.SolidCollisionFix(legTip, 2, 2, true))
                                    legTip.Y -= 4.2f * NPC.scale;
                            }

                        }

                        //Otherwise
                        else
                        {
                            

                            //if we have a previous position to step from, do a nicely eased step
                            if (previousGrabPosition.HasValue)
                            {
                                float lerp = FablesUtils.PolyInOutEasing(1 - strideTimer, 2f); ;
                                legTip = Vector2.Lerp(previousGrabPosition.Value, grabPosition.Value, lerp);

                                //Upwards bump motion
                                legTip.Y -= 11.2f * (float)Math.Sin(strideTimer * MathHelper.Pi) * NPC.scale;
                            }

                            //If this is the first step after spawning, or after falling, do a slightly less clean motion towards the target
                            else
                            {
                                //Move faster towards the tip if the leg has been falling for a while
                                float moveSpeed = (10f + Utils.GetLerpValue(20f, 40f, fallTime, true) * 15f) * Math.Max(NPC.scale, 1f);
                                legTip = legTip.MoveTowards(grabPosition.Value, moveSpeed);
                                legTip.Y -= 4.5f * Utils.GetLerpValue(0f, 50f, Math.Abs(legTip.X - grabPosition.Value.X), true) * NPC.scale;
                            }

                            //Time to move between the last grab position and the new one. Increases with crab's speed
                            float stepTime = 0.3f - 0.12f * Utils.GetLerpValue(4f, 8f, Math.Abs(NPC.velocity.X), true);
                            strideTimer -= 1 / (60f * stepTime);
                            if (strideTimer < 0)
                                strideTimer = 0;

                            //if we somehow moved away from the grab position so far that the leg cant even reach it, stop trying to grip it and find a new one next frame
                            if (legOrigin.Distance(grabPosition.Value) > maxLenght)
                            {
                                ReleaseGrip();
                            }
                        }

                        //Reset visual variables and charge up the force of the step effects
                        stepEffectForce = Math.Min(1f, stepEffectForce + 0.125f);
                        playedStepEffects = false;
                        stepTimer = 1f;
                    }



                    legTipGraphic = legTip;
                    //Leg tip "pierces" the ground a bit when stepping
                    if (!crabulon.TopDownView)
                    {
                        legTipGraphic += Vector2.UnitY * 7f;
                        if (stepTimer < 1)
                            legTipGraphic.Y += 10f * FablesUtils.PolyInEasing(stepTimer, 2f);
                    }

                    CalculateKnee();

                    if (legTipGraphic.Distance(legOriginGraphic) > maxLenght)
                        legTipGraphic = legOriginGraphic + legOriginGraphic.DirectionTo(legTipGraphic) * maxLenght;
                }

                public void CalculateKnee() => legKnee = FablesUtils.InverseKinematic(legOriginGraphic, legTipGraphic, ForelegLenght, LegLenght, !leftSet);

                public float DesiredGrabPositionVelocityXOffset => (Math.Abs(NPC.velocity.X) > 2f && (NPC.velocity.X * Direction < 0)) ? NPC.velocity.X.NonZeroSign() * 140f * Math.Min(NPC.scale, 1f) : 0f;

                public void UpdateDesiredGrabPosition(Vector2 legDirection)
                {
                    if (!crabulon.TopDownView)
                    {
                        desiredGrabPosition = NPC.Center + (legDirection * 1.25f + Vector2.UnitY).SafeNormalize(Vector2.UnitY) * maxLenght * 0.9f;

                        //Offset grab positions sideways
                        desiredGrabPosition += Vector2.UnitX * 170f * Direction * NPC.scale;

                        //Offset the grab positions latterally by the NPC's velocity if on the set of legs trailing behind
                        desiredGrabPosition.X += DesiredGrabPositionVelocityXOffset;

                        //Clamp the distance
                        if (desiredGrabPosition.Distance(legOrigin) >= maxLenght)
                            desiredGrabPosition = legOrigin + legOrigin.DirectionTo(desiredGrabPosition) * maxLenght;
                    }
                    else
                    {
                        Vector2 goingTowards = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
                        float lenghtProportion = frontPair ? 0.85f : 0.7f;
                        float lerper = frontPair ? 0.7f : 0.5f;
                        float rotation = frontPair ? -0.2f : -0.8f;

                        desiredGrabPosition = legOrigin + Vector2.Lerp(legDirection, goingTowards, lerper).SafeNormalize(goingTowards).RotatedBy(rotation * Direction) * maxLenght * lenghtProportion;
                        desiredGrabPosition += goingTowards * maxLenght * 0.2f;

                        float speeding = Utils.GetLerpValue(3f, 7, NPC.velocity.Length(), true);
                        if (speeding > 0f && frontPair)
                        {
                            desiredGrabPosition = legOrigin + legOrigin.DirectionTo(desiredGrabPosition).ToRotation().AngleLerp(NPC.rotation + MathHelper.PiOver2, speeding * 0.9f).ToRotationVector2() * legOrigin.Distance(desiredGrabPosition);
                        }


                        if (desiredGrabPosition.Distance(legOrigin) >= maxLenght * 0.85f + 0.15f * speeding)
                            desiredGrabPosition = legOrigin + legOrigin.DirectionTo(desiredGrabPosition) * maxLenght * (0.85f + 0.15f * speeding);
                    }
                }

                public Vector2 GetLegOrigin()
                {
                    Vector2 offset;
                    if (!crabulon.TopDownView)
                    {
                        offset = new Vector2((frontPair ? 70f : 46f) * (leftSet ? -1 : 1), frontPair ? 7f : 21f);
                        offset *= crabulon.NPC.scale;

                        if (crabulon.lookingSideways)
                            offset.X *= 0.7f;
                    }
                    else
                    {
                        offset = new Vector2((frontPair ? 45f : 40f) * (leftSet ? -1 : 1), frontPair ? 40f : -21f);
                        offset = offset.RotatedBy(crabulon.NPC.rotation) * crabulon.NPC.scale;
                    }

                    return crabulon.NPC.Center + offset;
                }

                #region Check if leg should release
                public bool ShouldReleaseLeg(bool frontSet, out bool noDelay)
                {
                    if (crabulon.TopDownView)
                        return ShouldReleaseLegWallCrawl(out noDelay);

                    noDelay = false;

                    float maxExtensionTreshold = 1f - SisterInfluence * 0.15f;
                    //If the legs are the ones being walked away from, the max lenght treshold is also shortened even more
                    if (!frontSet)
                        maxExtensionTreshold -= (1 - SisterInfluence) * 0.2f;
                    //Keep the treshold full if crabulon walks slowly enough
                    if (Math.Abs(NPC.velocity.X) < 1.4f)
                        maxExtensionTreshold = 1f;

                    float minExtensionTreshold = 0.26f - SisterInfluence * 0.16f;

                    float tooFarUnderTreshold = (0.25f + SisterInfluence * 0.75f) * 40f * NPC.scale;
                    float maxHeightTreshold = 30f * NPC.scale;

                    float extension = legTip.Distance(legOrigin);

                    //Ungrip when extended too far out
                    if (extension > maxLenght * maxExtensionTreshold)
                        return true;

                    //Ungrip when the leg is too compressed
                    else if (extension < maxLenght * minExtensionTreshold)
                    {
                        noDelay = true;
                        return true;
                    }

                    //Ungrip when the leg is too far behind and should take a new step forward
                    //Either immediately if part of the front set of legs, or if the step timer is over (to avoid back legs rapid fire
                    else if ((legOrigin.X - legTip.X) * Direction > tooFarUnderTreshold && (frontSet || stepTimer <= 0))
                    {
                        noDelay = true;
                        return true;
                    }

                    //Ungrip when the leg is too far above crabulon and too close to crabulon
                    else if (legOrigin.Y - legTip.Y > maxHeightTreshold && (legTip.X - legOrigin.X) * Direction < maxLenght * 0.2f)
                        return true;

                    return false;
                }

                public bool ShouldReleaseLegWallCrawl(out bool noDelay)
                {
                    noDelay = false;
                    float maxExtensionTreshold = 1f - SisterInfluence * 0.15f;
                    float minExtensionTreshold = 0.26f - SisterInfluence * 0.16f;
                    float extension = legTip.Distance(legOrigin);

                    Vector2 goingTowards = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
                    float dot = Vector2.Dot(goingTowards, legOrigin.DirectionTo(legTip));

                    if (!frontPair && dot < 0f)
                        maxExtensionTreshold *= 0.7f;

                    //Ungrip when extended too far out
                    if (extension > maxLenght * maxExtensionTreshold && dot < 0.6f)
                        return true;

                    //Ungrip when the leg is too compressed
                    else if (extension < maxLenght * minExtensionTreshold)
                    {
                        noDelay = true;
                        return true;
                    }

                    return false;
                }

                public void ReleaseGrip()
                {
                    if (pairedLeg.grabDelay < 1 && grabDelay < 1)
                        grabDelay = 3;

                    strideTimer = 1f;
                    previousGrabPosition = grabPosition ?? legTip;
                    grabPosition = null;
                    latchedOn = false;
                }
                #endregion

                #region Grab position scanning
                private void FindGrabPos(bool debugView = false)
                {
                    //Don't grab if in delay period
                    if (grabDelay > 0)
                    {
                        grabDelay--;
                        return;
                    }

                    if (crabulon.TopDownView)
                    {
                        //Nothing much to be said. Yuuurp
                        grabPosition = desiredGrabPosition;
                        return;
                    }

                    bool frontSet = Math.Sign(crabulon.NPC.velocity.X) == Direction;

                    //The position tracing from the shoulder to the desired grab position
                    Vector2 shoulder = legOrigin;
                    Vector2 grip = desiredGrabPosition;
                    if (frontSet)
                    {
                        shoulder.X += crabulon.NPC.velocity.X * 40f * NPC.scale;
                        grip.X += crabulon.NPC.velocity.X * 10f * NPC.scale;
                        grip.Y -= 20f * NPC.scale;

                        //Clamp distances
                        if (grip.Distance(legOrigin) > maxLenght)
                            grip = legOrigin + legOrigin.DirectionTo(grip) * maxLenght;
                        if (shoulder.Distance(legOrigin) > maxLenght)
                            shoulder = legOrigin + Vector2.UnitX * Direction * maxLenght;

                        if (debugView)
                        {
                            Dust.QuickDust(shoulder, Color.Red);
                            Dust.QuickDust(grip, Color.Yellow);
                            Dust.QuickDustLine(shoulder, grip, 10, Color.Blue);
                        }
                    }

                    Point? fromShoulderGuess = shoulder.RaytraceToFirstSolid(grip);
                    Point? bestGuess = null;
                    bool tooClose = false;

                    //We don't really want crab to grab a tile thats too close to himself
                    if (fromShoulderGuess != null)
                    {
                        if (TileToGripPoint(fromShoulderGuess.Value).Distance(legOrigin) < maxLenght * 0.45f)
                            tooClose = true;
                        else
                            bestGuess = fromShoulderGuess;
                    }

                    if (bestGuess == null)
                    {
                        //Look down to find a potential grab position
                        if (!tooClose)
                            bestGuess = RadialDownGrabPosScan(4, 1.2f, ref tooClose, debugView);

                        //Look around to find a grab position, without any raycasting
                        if (tooClose)
                        {
                            float radius = maxLenght * (frontPair ? 0.8f : 0.6f);
                            float startAngle = frontSet ? MathHelper.PiOver4 : MathHelper.PiOver2 * 0.8f;
                            bestGuess = RadialGrabPosScan(startAngle, MathHelper.Pi * 0.95f, radius, debugView);
                        }
                    }

                    //if we couldn't find anything better with the radial check, just go with the straight raycast as a fallback
                    if (bestGuess == null && fromShoulderGuess.HasValue)
                        bestGuess = fromShoulderGuess;

                    if (bestGuess != null)
                        TryConfirmWewGrabPos(bestGuess.Value);
                }

                private void GetTopDownGrabPos()
                {

                }

                /// <summary>
                /// Tries to look downwards for solid ground by raycasting from the shoulder to the desired grab position, rotated more and more towards the floor
                /// </summary>
                /// <param name="iterations">How many raycasts should happen</param>
                /// <param name="angle">How far down should the check be</param>
                /// <param name="tooClose"></param>
                /// <returns></returns>
                public Point? RadialDownGrabPosScan(int iterations, float angle, ref bool tooClose, bool debugView = false)
                {
                    int i = 0;
                    Point? bestGuess = null;
                    Vector2 toGrabPosition = legOrigin.DirectionTo(desiredGrabPosition);

                    while (i < iterations && bestGuess == null)
                    {
                        //Try tilting the grab position downwards until we find ground
                        Vector2 tiltedGrabPosition = legOrigin + toGrabPosition.RotatedBy(i * Direction / (float)iterations * angle) * maxLenght * 0.95f;

                        if (debugView)
                            Dust.QuickDust(tiltedGrabPosition, Color.Green);

                        bestGuess = legOrigin.RaytraceToFirstSolid(tiltedGrabPosition);
                        //Can't grab if the resulting grip location would be too close
                        if (bestGuess.HasValue && TileToGripPoint(bestGuess.Value).Distance(legOrigin) < maxLenght * 0.45f)
                        {
                            bestGuess = null;
                            tooClose = true;
                        }
                        else
                        {
                            if (debugView)
                                Dust.QuickDust(tiltedGrabPosition, Color.White);
                            tooClose = false;
                        }
                        i++;
                    }

                    return bestGuess;
                }

                /// <summary>
                /// Tries to look in a radius to the side of the leg for any solid ground tile. Prioritizes gripping on tiles that are exposed to air, but cant grab inside the ground
                /// Prefers having a grab spot that's close to straight to the side
                /// </summary>
                /// <param name="angleStart"></param>
                /// <param name="angleEnd"></param>
                /// <param name="searchRadius"></param>
                /// <returns></returns>
                public Point? RadialGrabPosScan(float angleStart, float angleEnd, float searchRadius, bool debugView = false)
                {
                    Vector2 origin = legOrigin;

                    //if crabulon is moving
                    if (Math.Abs(crabulon.NPC.velocity.X) > 2f)
                    {
                        //Move the check for the pair of legs that is being dragged a bit ahead
                        if (crabulon.NPC.velocity.X * Direction < 0)
                            origin.X += crabulon.NPC.velocity.X.NonZeroSign() * 140f * Math.Min(NPC.scale, 1f);

                        //Make the radius for the pair of legs that is moving forward a bit bigger, but not bigger than the max leg lenght
                        else
                            searchRadius = Math.Min(maxLenght, searchRadius * 1.2f);
                    }

                    float totalAngle = angleEnd - angleStart;
                    bool lastInAir = false;
                    float progress = 0f;
                    float halfTileAngle = FablesUtils.ArcAngle(8f, searchRadius);
                    float step = halfTileAngle / totalAngle;
                    List<Point> potentialGrabPoints = new List<Point>();
                    List<Point> insideTilesPositions = new List<Point>();


                    while (progress <= 1f)
                    {
                        float angle = (angleStart + progress * totalAngle) * Direction;
                        Vector2 tiltedGrabPosition = origin + (-Vector2.UnitY).RotatedBy(angle) * searchRadius;
                        Point candidate = tiltedGrabPosition.ToTileCoordinates();
                        Tile t = Main.tile[candidate];

                        if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] || (Main.tileSolidTop[t.TileType] && t.TileFrameY == 0))
                        {
                            //If we find a solid tile and we were previously in the air, that's a potential new step candidate
                            if (lastInAir)
                            {
                                potentialGrabPoints.Add(candidate);

                                if (debugView)
                                    Dust.QuickDust(candidate, Color.Red);
                            }
                            else
                                insideTilesPositions.Add(candidate);
                            lastInAir = false;
                        }

                        if (debugView)
                            Dust.QuickDust(candidate, Color.Blue);

                        //Purposefully makes it so that platforms immediately set "lastinair" back to false)
                        if (!t.HasUnactuatedTile || (!Main.tileSolid[t.TileType] && !TileID.Sets.Platforms[t.TileType]))
                            lastInAir = true;

                        progress += step;
                    }

                    if (potentialGrabPoints.Count > 0)
                        return potentialGrabPoints.OrderBy(p => RadialPosScanRating(p)).Last();
                    else if (insideTilesPositions.Count > 0)
                        return insideTilesPositions.OrderBy(p => RadialPosScanRating(p)).Last();

                    return null;
                }

                public float RadialPosScanRating(Point p)
                {
                    Vector2 worldPos = TileToGripPoint(p);
                    float lenght = worldPos.Distance(legOrigin);

                    //Check the angle from the left so its easier to get
                    Vector2 angleStart = worldPos;
                    if (angleStart.X < legOrigin.X)
                        angleStart.X += (legOrigin.X - angleStart.X) * 2;
                    float angle = legOrigin.AngleTo(angleStart);

                    Vector2 idealAngleStart = desiredGrabPosition;
                    if (idealAngleStart.X < legOrigin.X)
                        idealAngleStart.X += (legOrigin.X - idealAngleStart.X) * 2;
                    float idealAngle = legOrigin.AngleTo(idealAngleStart);

                    float idealGrabHeightBias = 0.2f + 0.8f * Utils.GetLerpValue(100f, 10f, Math.Abs(crabulon.FloorPosition.Y - worldPos.Y), true);

                    //Platforms with a close enough Y position are penalized, to prevent crabulon from grabbing onto platforms taht are going through itself
                    float closePlatformScoreReduction = 0f;
                    if (Main.tileSolidTop[Main.tile[p].TileType])
                        closePlatformScoreReduction += Utils.GetLerpValue(16f, 80f, legOrigin.Y - worldPos.Y, true);

                    return (1 - Math.Abs(angle - idealAngle) / MathHelper.PiOver2) * Utils.GetLerpValue(0, maxLenght * 0.85f, lenght, true) * idealGrabHeightBias - closePlatformScoreReduction;
                }

                private void TryConfirmWewGrabPos(Point potentialGrabPosition)
                {
                    if (grabPosition == null || RateGripPoint(potentialGrabPosition) > RateGripPoint(grabTile.Value))
                    {
                        Vector2 attachPoint = TileToGripPoint(potentialGrabPosition);

                        //Grab destination is the closest point on the tile
                        if (grabTile != null && grabTile.Value == attachPoint.ToTileCoordinates())
                            return;
                        grabPosition = attachPoint;
                    }
                }

                public Vector2 TileToGripPoint(Point tilePosition)
                {
                    Tile t = Main.tile[tilePosition];
                    Vector2 tileWorldCoordinates = tilePosition.ToWorldCoordinates();
                    Rectangle aroundTile = FablesUtils.RectangleFromVectors(tileWorldCoordinates - Vector2.One * 9f, tileWorldCoordinates + Vector2.One * 9f);
                    if (t.IsHalfBlock || t.Slope != SlopeType.Solid)
                    {
                        aroundTile.Y += 8;
                        aroundTile.Height -= 8;
                    }

                    return legOrigin.ClampInRect(aroundTile);
                }

                public float RateGripPoint(Point gripPoint)
                {
                    return 100f / Vector2.Distance(gripPoint.ToWorldCoordinates(), desiredGrabPosition);
                }

                public float ReleaseScore()
                {
                    float releaseScore;
                    if (grabPosition == null)
                        releaseScore = Vector2.Distance(legTip, desiredGrabPosition);
                    else
                        releaseScore = Vector2.Distance(grabPosition.Value, desiredGrabPosition) * 2f;


                    if (latchedOn)
                        releaseScore *= 2f;
                    //We really don't want to release if the other leg in teh pair isn't latched on
                    if (!pairedLeg.latchedOn)
                    {
                        releaseScore /= 100f;
                    }

                    int direction = leftSet ? -1 : 1;
                    if ((legTip.X - crabulon.NPC.Center.X).NonZeroSign() != direction)
                        releaseScore *= 100f;

                    return releaseScore;
                }
                #endregion

           

                #region Drawing
                internal readonly Asset<Texture2D> ForelimbAsset;
                internal readonly Asset<Texture2D> LimbAsset;

                public readonly Vector2 forelegSpriteOrigin;
                public readonly Vector2 legSpriteOrigin;
                public float spriteSizeMultiplier;

                public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
                {
                    SpriteEffects flip = leftSet ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    spriteBatch.Draw(ForelimbAsset.Value, legOriginGraphic - screenPos, null, drawColor, legOriginGraphic.AngleTo(legKnee), forelegSpriteOrigin, crabulon.NPC.scale * spriteSizeMultiplier, flip, 0);

                    spriteBatch.Draw(LimbAsset.Value, legKnee - screenPos, null, drawColor, legKnee.AngleTo(legTipGraphic), legSpriteOrigin, crabulon.NPC.scale * spriteSizeMultiplier, flip, 0);
                }
                #endregion
            }
        }
    }

}
