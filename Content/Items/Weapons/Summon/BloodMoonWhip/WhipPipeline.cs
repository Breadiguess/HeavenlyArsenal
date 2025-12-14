using System.Collections.Generic;
using HeavenlyArsenal.Core;
// File: WhipControlPipeline.cs

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

public interface IWhipModifier
{
    /// <summary>
    ///     I HATE WRITING COMMENTS!!!
    /// </summary>
    /// <param name="controlPoints">current list of points (motion already filled it)</param>
    /// <param name="projectile">the whip projectile</param>
    /// <param name="segments">number of logical segments for the whip (useful for index->percent)</param>
    /// <param name="rangeMultiplier"> whip range multiplier (already adjusted by player)</param>
    /// <param name="progress"> normalized 0..1 progress through the swing</param>
    void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress);
}

public interface IWhipMotion
{
    void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress);
}

/// <summary>
///     twists the whip around a certain point.
/// </summary>
public class TwirlModifier : IWhipModifier
{
    private readonly int startIndex;

    private readonly int endIndex;

    private readonly float strength;

    private readonly bool influencedByProgress;

    /// <summary>
    ///     Applies a twirl effect to whip control points.
    /// </summary>
    /// <param name="startIndex">Segment index to start twirling from.</param>
    /// <param name="endIndex">Segment index to stop applying twirl directly (later segments inherit).</param>
    /// <param name="strength">Strength of the twirl effect.</param>
    /// <param name="influencedByProgress">If true, twirl is scaled by swing progress (curved bell shape).</param>
    public TwirlModifier(int startIndex, int endIndex, float strength, bool influencedByProgress = true)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.strength = strength;
        this.influencedByProgress = influencedByProgress;
    }

    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        if (controlPoints == null || controlPoints.Count == 0)
        {
            return;
        }

        var start = Math.Clamp(startIndex, 0, controlPoints.Count - 1);
        var end = Math.Clamp(endIndex, start, controlPoints.Count - 1);

        var curvedProgress = influencedByProgress ? 1 - Math.Abs(2 * progress - 1) : 1f;
        var eff = curvedProgress * strength;

        var pivot = controlPoints[start];
        var lastAngle = 0f;

        // Apply twirl gradually up to the endIndex
        for (var i = start; i <= end; i++)
        {
            var rel = controlPoints[i] - pivot;
            var angle = eff * (i - start);
            rel = rel.RotatedBy(angle);
            controlPoints[i] = pivot + rel;

            lastAngle = angle;
        }

        // Ensure the rest of the whip continues smoothly with the last rotation
        for (var i = end + 1; i < controlPoints.Count; i++)
        {
            var rel = controlPoints[i] - pivot;
            rel = rel.RotatedBy(lastAngle);
            //Main.NewText($"PostSegment: {lastAngle}");
            controlPoints[i] = pivot + rel;
        }
    }
}

public class SmoothSineModifier : IWhipModifier
{
    private readonly int startIndex;

    private readonly int endIndex;

    private readonly float amplitude;

    private readonly float frequency;

    private readonly float period;

    private readonly bool UsesProgress;

    // Default is sin(pi * t) so amplitude ramps up from 0 -> 1 at mid-swing -> 0 at the end.
    public Func<float, float> AmplitudeEnvelope { get; set; } = t => MathF.Sin(MathF.PI * MathHelper.Clamp(t, 0f, 1f));

    public SmoothSineModifier(int startIndex, int endIndex, float amplitude, float frequency, float period, bool UsesProgress = true)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.amplitude = amplitude;
        this.frequency = frequency;
        this.period = period;
        this.UsesProgress = UsesProgress;
    }

    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        if (controlPoints == null || controlPoints.Count == 0)
        {
            return;
        }

        var s = Math.Clamp(startIndex, 0, controlPoints.Count - 1);
        var e = Math.Clamp(endIndex, s, controlPoints.Count - 1);
        var denom = Math.Max(1, e - s);

        // compute envelope once (progress assumed normalized 0..1)

        var bell = UsesProgress ? 1 - Math.Abs(progress * 2 - 1) : 0.5f;
        var env = AmplitudeEnvelope != null ? AmplitudeEnvelope(bell) : 1f;
        env = MathHelper.Clamp(env, 0f, 1f);
        var effectiveAmplitude = amplitude * env;

        for (var i = s; i <= e; i++)
        {
            var t = (i - s) / (float)denom; // local 0..1 along modifier
            // phase uses frequency across the mod range, and time progress to animate
            var sine = MathF.Sin((t * frequency + progress) * MathHelper.TwoPi / period);
            var perp = PerpAt(i, controlPoints);

            if (perp == Vector2.Zero)
            {
                continue;
            }

            var v = controlPoints[i];
            v += perp * (sine * effectiveAmplitude);
            controlPoints[i] = v;
        }
    }

    // helper to compute a local perpendicular vector at index i (safe)
    private static Vector2 PerpAt(int i, List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2)
        {
            return Vector2.Zero;
        }

        Vector2 dir;

        if (i <= 0)
        {
            dir = pts[1] - pts[0];
        }
        else if (i >= pts.Count - 1)
        {
            dir = pts[^1] - pts[pts.Count - 2];
        }
        else
        {
            dir = pts[i + 1] - pts[i - 1];
        }

        if (dir.LengthSquared() < 0.0001f)
        {
            return Vector2.Zero;
        }

        dir.Normalize();

        return new Vector2(-dir.Y, dir.X);
    }
}

/// <summary>
///     pretty much vanilla.
///     mostly exists so that you can tweak the vanilla whip without having to re-create the entire
///     vanilla swing.
/// </summary>
public class VanillaWhipMotion : IWhipMotion
{
    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        if (controlPoints == null)
        {
            return;
        }

        // Let the engine compute its canonical whip control points.
        // Projectile.FillWhipControlPoints expects Projectile.ai[0] to already track the
        // swing progress (CleanBaseWhip does that), so calling into it yields the vanilla curve.
        controlPoints.Clear();
        Projectile.FillWhipControlPoints(projectile, controlPoints);

        // If the pipeline passed a different segments / rangeMultiplier and you want to
        // respect them rather than the engine defaults, you'd need a custom implementation.
        // In most cases using the engine's points is the correct vanilla behavior.
    }
}

/// <summary>
///     Stolen from entropy, who stole it from fancy whips.
///     i feel no shame.
/// </summary>
public class BraidedMotion : IWhipMotion
{
    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        controlPoints.Clear();

        // 1) Base arm anchor
        var arm = Main.GetPlayerArmPosition(projectile);
        controlPoints.Add(arm);

        // 2) Parameters
        var hDistancePercent = progress * 1.5f;
        var retractionPercent = 0f;

        if (hDistancePercent > 1f)
        {
            retractionPercent = (hDistancePercent - 1f) / 0.5f;
            hDistancePercent = MathHelper.Lerp(1f, 0f, retractionPercent);
        }

        var distFactor = Main.player[projectile.owner].HeldItem.useAnimation * 2 * progress * Main.player[projectile.owner].whipRangeMultiplier;
        var pxPerSegment = projectile.velocity.Length() * distFactor * hDistancePercent * rangeMultiplier / segments;

        var invH = 1f - hDistancePercent;
        var smoothH = 1f - invH * invH;

        var rot = -MathHelper.PiOver2;
        var rot2 = MathHelper.PiOver2 + MathHelper.PiOver2 * projectile.spriteDirection;
        var rot3 = MathHelper.PiOver2;

        var prev_p = arm;
        var prev_p2 = arm;
        var prev_p3 = arm;

        // 3) Build control points
        for (var i = 0; i < segments; i++)
        {
            var segPercent = i / (float)segments;

            // Sine-based oscillation for this segment’s angle
            var thisRot = 3.707f *
                          MathF.Sin(2f * segPercent - 3.42f * progress + 0.75f * hDistancePercent) *
                          -(float)projectile.spriteDirection +
                          MathHelper.PiOver2;

            // Candidate forward steps
            var p = prev_p + rot.ToRotationVector2() * pxPerSegment * 1.2f;
            var p2 = prev_p3 + rot3.ToRotationVector2() * (pxPerSegment * 2f);
            var p3 = prev_p2 + rot2.ToRotationVector2() * (pxPerSegment * 2f);

            // Blend them
            var value = Vector2.Lerp(p2, p, smoothH * 0.9f + 0.1f);
            var vector7 = Vector2.Lerp(p3, value, smoothH * 0.7f + 0.3f);
            var vector9 = arm + (vector7 - arm) * new Vector2(1.7f, 1.65f);
            var offset = vector9 - arm;
            offset = offset.RotatedBy(projectile.rotation);
            vector9 = arm + offset;
            // Add to whip
            controlPoints.Add(vector9);

            // Update state
            rot = thisRot;
            rot2 = thisRot;
            rot3 = thisRot;
            prev_p = p;
            prev_p2 = p3;
            prev_p3 = p2;
        }
    }
}

public class StraightLineMotion : IWhipMotion
{
    /// <summary>
    ///     a new straightline motion instance.
    /// </summary>
    /// <param name="precision">
    ///     this is actually a degree, turned into a radian and used to compute the
    ///     angular offset of the
    /// </param>
    public StraightLineMotion(float precision = 0)
    {
        if (firstTime)
        {
            Precision = Main.rand.NextFloat(-precision, precision);
            firstTime = false;
        }
    }

    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        controlPoints.Clear();
        var player = Main.player[projectile.owner];
        var heldItem = player.HeldItem;
        //if(progress <= 0.25f)
        //Main.NewText($"Progress: {progress}");

        var BellProgress = 1 - Math.Abs(2 * progress - 1);
        var distFactor = ContentSamples.ItemsByType[heldItem.type].useAnimation * 2 * BellProgress * player.whipRangeMultiplier;
        var baseSpeed = projectile.velocity.Length();

        if (baseSpeed < 0.0001f)
        {
            baseSpeed = 12f;
        }

        var pxPerSegment = baseSpeed * distFactor * rangeMultiplier / Math.Max(1, segments);

        var origin = Main.GetPlayerArmPosition(projectile);
        controlPoints.Add(origin);

        var forward = projectile.velocity.LengthSquared() > 0.0001f ? Vector2.Normalize(projectile.velocity).RotatedBy(MathHelper.ToRadians(Precision * progress)) : new Vector2(player.direction, 0f);

        for (var i = 1; i <= segments; i++)
        {
            controlPoints.Add(origin + forward * (pxPerSegment * i));
        }
    }

    internal bool firstTime = true;

    internal float Precision;
}

/// <summary>
///     Vanilla swing baseline, blended toward a designer Bezier shape in mid-swing.
///     The Bezier's ControlPoints are interpreted as LOCAL OFFSETS (p0 must be (0,0), oriented +X).
/// </summary>
public class CustomMotion : IWhipMotion
{
    private readonly BezierCurve template; // local-space curve (do NOT mutate this instance)

    private readonly int precision; // arc-length precision when sampling the world curve

    /// <param name="template">
    ///     Local-space Bezier: p0=(0,0), points laid out facing +X (right).
    ///     e.g., new BezierCurve(new Vector2(0,0), new Vector2(60,-80), new Vector2(160,90), new
    ///     Vector2(220,0));
    /// </param>
    /// <param name="precision">Arc-length sampling precision for Bezier (default 30).</param>
    public CustomMotion(BezierCurve template, int precision = 30)
    {
        this.template = template;
        this.precision = Math.Max(8, precision);
    }

    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        if (controlPoints == null)
        {
            return;
        }

        if (template == null || template.ControlPoints == null || template.ControlPoints.Length < 2)
        {
            // Fallback: vanilla only if the curve is invalid
            controlPoints.Clear();
            Projectile.FillWhipControlPoints(projectile, controlPoints);

            return;
        }

        // 1) Get arm position and vanilla swing
        var arm = Main.GetPlayerArmPosition(projectile);
        List<Vector2> vanilla = new();
        Projectile.FillWhipControlPoints(projectile, vanilla);
        var totalPoints = vanilla.Count;

        // 2) Calculate swing direction (first segment of vanilla)
        var baseDir = vanilla[1] - vanilla[0];
        var baseAngle = baseDir.ToRotation();

        // Current vanilla length (sum of segment lengths)
        var vanillaLen = 0f;

        for (var i = 1; i < totalPoints; i++)
        {
            vanillaLen += Vector2.Distance(vanilla[i - 1], vanilla[i]);
        }

        if (vanillaLen < 1e-3f)
        {
            vanillaLen = 1f;
        }

        // Template length in LOCAL space (sample once per frame)
        var tempLocal = template.GetEvenlySpacedPoints(totalPoints, precision);
        var templateLen = 0f;

        for (var i = 1; i < tempLocal.Count; i++)
        {
            templateLen += Vector2.Distance(tempLocal[i - 1], tempLocal[i]);
        }

        if (templateLen < 1e-3f)
        {
            templateLen = 1f;
        }

        var lengthScale = vanillaLen / templateLen;

        // 3) Build world-space Bezier from local offsets
        var worldCP = new Vector2[template.ControlPoints.Length];

        for (var i = 0; i < template.ControlPoints.Length; i++)
        {
            var rel = template.ControlPoints[i];

            // Scale to current whip length
            rel *= lengthScale;

            // Flip horizontally when facing left
            if (projectile.spriteDirection == -1)
            {
                rel.X = -rel.X;
            }

            // Rotate into the same angle as the vanilla swing
            rel = rel.RotatedBy(baseAngle);

            // Anchor at arm
            worldCP[i] = arm + rel;
        }

        // Create a temporary world curve and sample same count as vanilla
        var worldCurve = new BezierCurve(worldCP);
        var bezier = worldCurve.GetEvenlySpacedPoints(totalPoints, precision, true);

        if (bezier.Count != totalPoints)
        {
            // Guard: counts must match to avoid despawn
            controlPoints.Clear();
            controlPoints.AddRange(vanilla);

            return;
        }

        // 3) Bell-curve influence: 0 at ends, 1 at mid-swing
        var bell = MathF.Sin(MathHelper.Clamp(progress, 0f, 1f) * MathHelper.Pi);

        // 4) Blend vanilla <-> Bezier
        controlPoints.Clear();

        for (var i = 0; i < totalPoints; i++)
        {
            controlPoints.Add(Vector2.Lerp(vanilla[i], bezier[i], bell * 0.2f));
        }
    }
}

/// <summary>
/// </summary>
public class ModularWhipController
{
    private readonly IWhipMotion motion;

    private readonly List<IWhipModifier> modifiers = new();

    public ModularWhipController(IWhipMotion motion)
    {
        this.motion = motion;
    }

    public void Clear()
    {
        modifiers.Clear();
    }

    public void AddModifier(IWhipModifier modifier)
    {
        modifiers.Add(modifier);
    }

    // segments & rangeMultiplier must come from Projectile.GetWhipSettings(...) (or similar) so the motion can size itself.
    public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
    {
        if (controlPoints == null)
        {
            return;
        }

        // Build base motion (this clears & fills controlPoints)
        motion.Apply(controlPoints, projectile, segments, rangeMultiplier, progress);

        // Apply all modifiers (they mutate in-place)
        foreach (var mod in modifiers)
        {
            mod.Apply(controlPoints, projectile, segments, rangeMultiplier, progress);
        }
    }
}