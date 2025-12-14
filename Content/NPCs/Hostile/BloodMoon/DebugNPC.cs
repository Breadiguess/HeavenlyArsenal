using System.Linq;
using CalamityMod;
using HeavenlyArsenal.Core.Systems;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;

internal partial class DebugNPC : BloodMoonBaseNPC
{
    public int LimbCount = 4;

    public DebugNPCLimb[] _limbs;

    public Vector2[] _limbBaseOffsets;

    private float targetHeightOffset;

    private bool IsFalling => NPC.velocity.Y > 1f;

    public override void OnSpawn(IEntitySource source)
    {
        CreateLimbs();
    }

    public override void SetDefaults()
    {
        NPC.lifeMax = 3;
        NPC.defense = 999999;
        NPC.aiStyle = -1;
        NPC.Size = new Vector2(90, 50);
        NPC.noGravity = false;
        NPC.noTileCollide = false;
    }

    public override void AI()
    {
        NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Calamity().mouseWorld).ToRotationVector2().X * 4; //* NPC.Distance(Main.MouseWorld) ;
        //NPC.velocity.Y = NPC.AngleTo(Main.MouseWorld).ToRotationVector2().Y * NPC.Distance(Main.MouseWorld);
    }

    private void balanceHead(float interp = 0.2f)
    {
        var d = Vector2.Zero;

        for (var i = 0; i < _limbs.Count(); i++)
        {
            if (_limbs[i].GrabPosition.HasValue)
            {
                d += _limbs[i].GrabPosition.Value;
            }

            if (_limbs[i] == default)
            {
                CreateLimbs();
            }
        }

        d /= LimbCount;

        var f = -MathHelper.PiOver2; // + MathHelper.ToRadians(30);
        NPC.rotation = f; //  Math.Clamp(NPC.rotation.AngleLerp(d.AngleTo(NPC.Center), interp), -MathHelper.PiOver2 + MathHelper.ToRadians(-30), -MathHelper.PiOver2 + MathHelper.ToRadians(30));

        var width = NPC.width * 1.2f;

        _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 - 20).RotatedBy(NPC.rotation + MathHelper.PiOver2);
        _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20).RotatedBy(NPC.rotation + MathHelper.PiOver2);
        _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10).RotatedBy(NPC.rotation + MathHelper.PiOver2);
        _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10).RotatedBy(NPC.rotation + MathHelper.PiOver2);
    }

    private void ApplyStanceHeightAdjustment()
    {
        var totalError = 0f;
        var groundedCount = 0;

        for (var i = 0; i < LimbCount; i++)
        {
            var limb = _limbs[i];
            var basePos = NPC.Center + _limbBaseOffsets[i];

            if (!IsLimbGrounded(limb))
            {
                continue;
            }

            var dist = Vector2.Distance(basePos, limb.EndPosition);

            var max = limb.skeletonMaxLength; // You store this somewhere
            var stance = max * 0.75f; // Preferred bend ratio

            totalError += dist - stance; // >0 = too high, <0 = too low
            groundedCount++;
        }

        if (groundedCount == 0)
        {
            NPC.noGravity = false;

            return; // falling, do not lift NPC artificially
        }

        NPC.noGravity = true;
        var avgError = totalError / groundedCount;

        // Limit how much correction occurs per frame
        var correction = MathHelper.Clamp(avgError, -10f, 10f);

        // Smoothing factor
        var strength = 0.15f;

        NPC.position.Y += correction * strength;
    }

    private Vector2 GetIdleRestPosition(int i, Vector2 basePos)
    {
        return FindNewGrabPoint(basePos, i);
    }

    private Vector2 FindNewGrabPoint(Vector2 basePos, int index, int retry = 0)
    {
        var maxDist = _limbs[index].skeletonMaxLength * 0.65f;

        var sideOffset = Math.Clamp(NPC.velocity.X * 40f, -100, 100);

        //Main.NewText(sideOffset);
        var hit = LineAlgorithm.RaycastTo
        (
            basePos + new Vector2(0, -100),
            new Vector2(basePos.X, NPC.Center.Y) + new Vector2(sideOffset * 1.25f, 115 + 100),
            debug: index == 2 //.RotatedBy((NPC.rotation + MathHelper.PiOver2)*0.5f)
        );

        if (!hit.HasValue)
        {
            retry++;

            return FindNewGrabPoint(basePos + new Vector2(Main.rand.Next(-1, 1), 0), index, retry); // fallback
        }

        if (Main.tile[hit.Value].IsTileSolid())

        {
            return hit.Value.ToWorldCoordinates();
        }

        return basePos;
    }

    private Vector2 FindFallingGrabPoint(Vector2 basePos)
    {
        // Raycast straight down under the leg's origin
        var hit = LineAlgorithm.RaycastTo
        (
            basePos,
            basePos + new Vector2(0, 300f) // long ray down
        );

        if (hit.HasValue)
        {
            return hit.Value.ToWorldCoordinates() + new Vector2(0, -8f);
        }

        // If no ground found, keep leg fully extended downward
        return basePos + new Vector2(0, 250f);
    }

    private bool ShouldRelease(int limbIndex, DebugNPCLimb limb, Vector2 basePos)
    {
        //if (limbIndex == 2)
        //Main.NewText(1);
        // Never release if stepping

        if (limb.StepProgress > 0f)
        {
            return false;
        }

        // Must have a foothold to release FROM
        if (!limb.GrabPosition.HasValue)
        {
            return true; // needs to find one immediately
        }

        var opposite = GetOppositeLeg(limbIndex);
        var other = _limbs[opposite];

        // If the paired leg is NOT grounded, this leg must WAIT.
        // if (IsLimbGrounded(other))
        // {
        //      limb.StepCooldown = other.StepCooldown;
        //      return false;
        //  }
        var maxDist = limb.skeletonMaxLength * 0.67f;
        var dist = Vector2.Distance(basePos, limb.GrabPosition.Value);

        if (limb.GrabPosition.Value.Y < basePos.Y)
        {
            return true;
        }

        var tolerance = 18f;

        if (dist > maxDist + tolerance)
        {
            return true;
        }

        if (limb.EndPosition.Distance(basePos) < 40)
        {
            return true;
        }

        if (limb.EndPosition.Distance(limb.GrabPosition.HasValue ? limb.GrabPosition.Value : Vector2.Zero) > 50)
        {
            return true;
        }

        return false;
    }

    private int GetOppositeLeg(int i)
    {
        if (i == 0)
        {
            return 2;
        }

        if (i == 2)
        {
            return 0;
        }

        if (i == 1)
        {
            return 3;
        }

        if (i == 3)
        {
            return 1;
        }

        return i;
    }

    private bool IsLimbGrounded(DebugNPCLimb limb)
    {
        if (!limb.GrabPosition.HasValue)
        {
            return false;
        }

        if (limb.StepProgress > 0f)
        {
            return false; // stepping legs don't support body weight
        }

        var foot = limb.GrabPosition.Value;

        // Raycast a short distance downward
        var hit = LineAlgorithm.RaycastTo
        (
            foot,
            foot + new Vector2(0, 24f) // 24px downward tolerance
        );

        if (!hit.HasValue)
        {
            return false;
        }

        var t = Framing.GetTileSafely(hit.Value);

        return t.HasTile && Main.tileSolid[t.TileType];
    }

    public override void PostAI()
    {
        var isIdle = Math.Abs(NPC.velocity.X) < 0.5f;

        for (var i = 0; i < LimbCount; i++)
        {
            var limb = _limbs[i];

            if (limb.GrabPosition.HasValue)
            {
                //don't grab above body
                if (limb.GrabPosition.Value.Distance(NPC.Top) < 4 && MathF.Round(limb.GrabPosition.Value.Y) >= MathF.Round(NPC.Top.Y))
                {
                    limb.StepCooldown = 0;
                    limb.StepProgress = 0;
                    limb.ShouldStep = true;
                }

                if (limb.GrabPosition.Value.Distance(NPC.Center + _limbBaseOffsets[i]) < 30)
                {
                    //limb.ShouldStep = true;
                    //limb.StepCooldown = 0;
                    //limb.StepProgress = 0;
                }
            }

            var basePos = NPC.Center + _limbBaseOffsets[i];

            /* if (isIdle && limb.StepProgress <= 0f)
             {
                 Vector2 idlePos = GetIdleRestPosition(i, basePos);

                 // distance from current foothold to ideal idle spot
                 float idleError = Vector2.Distance(limb.GrabPosition ?? limb.EndPosition, idlePos);

                 // threshold: if leg is too far from where it *should* rest
                 if (idleError > 50f) // tune this
                 {
                     limb.PreviousGrabPosition = limb.GrabPosition ?? limb.EndPosition;
                     limb.GrabPosition = idlePos;
                     limb.StepProgress = 1f;
                     limb.StepCooldown = 10; // small so settling is quick

                     _limbs[i] = limb;
                     UpdateLimbState(ref _limbs[i], basePos, 0.4f, 15, i);
                     continue;
                 }
             }*/

            var distToGround = Vector2.Distance(basePos, limb.GrabPosition.HasValue ? limb.GrabPosition.Value : limb.EndPosition);
            var max = limb.skeletonMaxLength;
            var stanceLength = max * 0.75f;

            if (limb.StepProgress <= 0f && ShouldRelease(i, limb, basePos))
            {
                limb.PreviousGrabPosition = limb.GrabPosition ?? limb.EndPosition;
                limb.GrabPosition = FindNewGrabPoint(basePos, i);
                limb.StepProgress = 1f;
            }

            // 2. Animate step
            if (limb.StepProgress > 0f)
            {
                //float t = 1f - limb.StepProgress;
                //Vector2 start = limb.PreviousGrabPosition ?? limb.EndPosition;
                //Vector2 end = limb.GrabPosition ?? start;

                //float arc = (float)Math.Sin(t * MathHelper.Pi) * 402f;
                //limb.TargetPosition = Vector2.Lerp(start, end, t) + new Vector2(0, -arc);

                limb.StepProgress -= 0.1f;

                if (limb.StepProgress <= 0f)
                {
                    limb.StepProgress = 0f;
                    //limb.EndPosition = end;
                }
            }
            else
            {
                if (limb.GrabPosition.HasValue)
                {
                    limb.TargetPosition = limb.GrabPosition.Value;
                }
            }

            _limbs[i] = limb;
            UpdateLimbState(ref _limbs[i], basePos, 0.4f, 15, i);
        }

        balanceHead(0.025f);
        ApplyStanceHeightAdjustment();
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
    }
}