using Luminance.Common.Utilities;

namespace HeavenlyArsenal.Common.utils;

public class Rope
{
    public class RopeSegment
    {
        public Vector2 position;

        public Vector2 oldPosition;

        public Vector2 velocity;

        public bool pinned;

        public RopeSegment(Vector2 position)
        {
            this.position = position;
            oldPosition = position;
        }
    }

    public RopeSegment[] segments;

    public float segmentLength;

    public Vector2 gravity;

    public bool tileCollide;

    public Vector2 colliderOrigin;

    public int colliderWidth;

    public int colliderHeight;

    public float damping;

    private readonly int accuracy;

    public Rope(Vector2 startPos, Vector2 endPos, int segmentCount, float segmentLength, Vector2 gravity, int accuracy = 10)
    {
        segments = new RopeSegment[segmentCount];

        for (var i = 0; i < segmentCount; i++)
        {
            var segmentPos = Vector2.Lerp(startPos, endPos, i / (segmentCount - 1f));
            segments[i] = new RopeSegment(segmentPos);
        }

        segments[0].pinned = true;
        segments[^1].pinned = true;

        this.segmentLength = segmentLength;
        this.gravity = gravity;
        this.accuracy = accuracy;
    }

    public void Settle()
    {
        var oldDamp = damping;
        damping = 0.67f;

        for (var a = 0; a < segments.Length; a++)
        {
            Update();
        }

        damping = oldDamp;
    }

    /// <summary>
    ///     Calculates the overall segment length of a rope based on the horizontal span between its two
    ///     end points and a desired sag distance.
    /// </summary>
    public static float CalculateSegmentLength(float ropeSpan, float sag)
    {
        // A rope at rest is defined via a catenary curve, which exists in the following mathematical form:
        // VerticalOffset(x) = a * cosh(x / a)

        // Furthermore, the length of a rope, given the horizontal width w for a rope, is defined as follows:
        // L = 2a * sinh(w / 2a)

        // In order to use the above equation, the value of a must be determined for the catenary that this rope will form.
        // To do so, a numerical solution will need to be found based on the known width and sag values.

        // Suppose the two supports are at equal height at distances -w/2 and w/2.
        // From this, sag (which will be denoted with h) can be defined in the following way: h = VerticalOffset(w/2) - VerticalOffset(0)
        // Reducing this results in the following equation:

        // h = a(cosh(w / 2a) - 1)
        // a(cosh(w / 2a) - 1) - h = 0
        // This can be used to numerically find a.
        var initialGuessA = sag;
        var a = (float)IterativelySearchForRoot(x => { return x * (Math.Cosh(ropeSpan / x * 0.5) - 1D) - sag; }, initialGuessA, 9);

        // Now that a is known, it's just a matter of plugging it back into the original equation to find L.
        return MathF.Sinh(ropeSpan / a * 0.5f) * a * 2f;
    }

    /// <summary>
    ///     Searches for an approximate for a root of a given function.
    /// </summary>
    /// <param name="fx">The function to find the root for.</param>
    /// <param name="initialGuess">The initial guess for what the root could be.</param>
    /// <param name="iterations">
    ///     The amount of iterations to perform. The higher this is, the more
    ///     generally accurate the result will be.
    /// </param>
    private static double IterativelySearchForRoot(Func<double, double> fx, double initialGuess, int iterations)
    {
        // This uses the Newton-Raphson method to iteratively get closer and closer to roots of a given function.
        // The exactly formula is as follows:
        // x = x - f(x) / f'(x)
        // In most circumstances repeating the above equation will result in closer and closer approximations to a root.
        // The exact reason as to why this intuitively works can be found at the following video:
        // https://www.youtube.com/watch?v=-RdOwhmqP5s
        var result = initialGuess;

        for (var i = 0; i < iterations; i++)
        {
            var derivative = fx.ApproximateDerivative(result);
            result -= fx(result) / derivative;
        }

        return result;
    }

    public void Update()
    {
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i].velocity = (segments[i].position - segments[i].oldPosition) * (1f - damping);

            if (segments[i].velocity.Length() < 0.015f)
            {
                segments[i].velocity = Vector2.Zero;
            }

            segments[i].oldPosition = segments[i].position;

            if (!segments[i].pinned)
            {
                segments[i].position += TileCollision(segments[i].position, segments[i].velocity + gravity);
            }
        }

        for (var a = 0; a < accuracy; a++)
        {
            Constrain();
        }
    }

    public void Constrain()
    {
        for (var i = 0; i < segments.Length - 1; i++)
        {
            var dist = segments[i].position.Distance(segments[i + 1].position);
            var error = dist - segmentLength;
            var correction = segments[i].position.DirectionFrom(segments[i + 1].position) * error;

            var pinned = segments[i].pinned;
            var nextPinned = segments[i + 1].pinned;
            var multiplier = pinned || nextPinned ? 1f : 0.5f;

            if (!pinned)
            {
                segments[i].position -= TileCollision(segments[i].position, correction * multiplier);
            }

            if (!nextPinned)
            {
                segments[i + 1].position += TileCollision(segments[i + 1].position, correction * multiplier);
            }
        }
    }

    private Vector2 TileCollision(Vector2 position, Vector2 velocity)
    {
        if (!tileCollide)
        {
            return velocity;
        }

        var newVelocity = Collision.noSlopeCollision(position + colliderOrigin, velocity, colliderWidth, colliderHeight + 2, true, true);
        newVelocity = Collision.noSlopeCollision(position + colliderOrigin, newVelocity, colliderWidth, colliderHeight, true, true);
        var result = velocity;

        if (Math.Abs(velocity.X) > Math.Abs(newVelocity.X))
        {
            result.X = 0;
        }

        if (Math.Abs(velocity.Y) > Math.Abs(newVelocity.Y))
        {
            result.Y = 0;
        }

        return result;
    }

    public Vector2[] GetPoints()
    {
        var points = new Vector2[segments.Length];

        for (var i = 0; i < segments.Length; i++)
        {
            points[i] = segments[i].position;
        }

        return points;
    }

    public Rectangle GetCollisionRect(int i)
    {
        return new Rectangle((int)(segments[i].position.Floor().X + colliderOrigin.X), (int)(segments[i].position.Floor().Y + colliderOrigin.Y), colliderWidth, colliderHeight);
    }
}