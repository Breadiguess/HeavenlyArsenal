using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace HeavenlyArsenal.Core
{
    /// <summary>  
    /// Represents a Bezier curve with methods to evaluate points, calculate arc lengths,  
    /// and generate evenly spaced points along the curve.  
    /// </summary>  
    public class BezierCurve
    {
        /// <summary>  
        /// The control points that define the shape of the Bezier curve.  
        /// </summary>  
        public Vector2[] ControlPoints;

        /// <summary>  
        /// Precomputed arc lengths for the curve, used for parametrization.  
        /// </summary>  
        public float[] arcLenghts;

        /// <summary>  
        /// Initializes a new instance of the <see cref="BezierCurve"/> class with the specified control points.  
        /// </summary>  
        /// <param name="controls">The control points of the curve.</param>  
        public BezierCurve(params Vector2[] controls)
        {
            ControlPoints = controls;
        }

        /// <summary>  
        /// Evaluates the position on the curve at a given interpolant value.  
        /// </summary>  
        /// <param name="interpolant">The interpolant value (0 to 1) to evaluate the curve.</param>  
        /// <returns>The position on the curve at the specified interpolant.</returns>  
        public Vector2 Evaluate(float interpolant) => PrivateEvaluate(ControlPoints, MathHelper.Clamp(interpolant, 0f, 1f));

        /// <summary>  
        /// Generates a list of points along the curve, evenly spaced by the interpolant.  
        /// </summary>  
        /// <param name="totalPoints">The total number of points to generate.</param>  
        /// <returns>A list of points along the curve.</returns>  
        public List<Vector2> GetPoints(int totalPoints)
        {
            float perStep = 1f / totalPoints;

            List<Vector2> points = new List<Vector2>();

            for (float step = 0f; step <= 1f; step += perStep)
                points.Add(Evaluate(step));

            return points;
        }

        /// <summary>  
        /// Parametrizes the curve based on arc length to find the interpolant for a given step.  
        /// </summary>  
        /// <param name="step">The step value (0 to 1) along the curve.</param>  
        /// <param name="totalCurveLentgh">The total arc length of the curve.</param>  
        /// <returns>The interpolant value corresponding to the given step.</returns>  
        public float ArcLentghParametrize(float step, float totalCurveLentgh)
        {
            float pointAtLentgh = step * totalCurveLentgh;

            float longestLenghtFound = 0;
            float longerLenghtFound = 0;

            int index = 0;

            for (int i = 0; i < arcLenghts.Length; i++)
            {
                if (arcLenghts[i] == pointAtLentgh)
                    return i / (float)(arcLenghts.Length - 1);

                if (arcLenghts[i] > pointAtLentgh)
                {
                    longerLenghtFound = arcLenghts[i];
                    break;
                }

                index = i;
                longestLenghtFound = arcLenghts[i];
            }

            if (longerLenghtFound != 0)
            {
                return (index + (pointAtLentgh - longestLenghtFound) / (longerLenghtFound - longestLenghtFound)) / (float)(arcLenghts.Length - 1);
            }

            return 1;
        }

        /// <summary>  
        /// Generates a list of evenly spaced points along the curve based on arc length.  
        /// </summary>  
        /// <param name="totalPoints">The total number of points to generate.</param>  
        /// <param name="computationPrecision">The precision used for arc length computation.</param>  
        /// <param name="forceRecalculate">Whether to force recalculation of arc lengths.</param>  
        /// <returns>A list of evenly spaced points along the curve.</returns>  
        public List<Vector2> GetEvenlySpacedPoints(int totalPoints, int computationPrecision = 30, bool forceRecalculate = false)
        {
            if (arcLenghts == null || arcLenghts.Length == 0 || forceRecalculate)
            {
                arcLenghts = new float[computationPrecision + 1];
                arcLenghts[0] = 0;

                // Calculate the arc length at a bunch of points  
                Vector2 oldPosition = ControlPoints[0];
                for (int i = 1; i <= computationPrecision; i += 1)
                {
                    Vector2 position = Evaluate(i / (float)computationPrecision);
                    float curveLength = (position - oldPosition).Length();
                    arcLenghts[i] = arcLenghts[i - 1] + curveLength;

                    oldPosition = position;
                }
            }

            float totalCurveLentgh = arcLenghts[arcLenghts.Length - 1];

            List<Vector2> points = new List<Vector2>();

            for (int step = 0; step < totalPoints; step++)
                points.Add(Evaluate(ArcLentghParametrize((step / (float)(totalPoints - 1)), totalCurveLentgh)));

            return points;
        }

        /// <summary>  
        /// Recursively evaluates the position on the curve using De Casteljau's algorithm.  
        /// </summary>  
        /// <param name="points">The control points of the curve.</param>  
        /// <param name="T">The interpolant value (0 to 1).</param>  
        /// <returns>The position on the curve at the specified interpolant.</returns>  
        private Vector2 PrivateEvaluate(Vector2[] points, float T)
        {
            while (points.Length > 2)
            {
                Vector2[] nextPoints = new Vector2[points.Length - 1];
                for (int k = 0; k < points.Length - 1; k++)
                    nextPoints[k] = Vector2.Lerp(points[k], points[k + 1], T);

                points = nextPoints;
            }

            if (points.Length <= 1)
                return Vector2.Zero;

            return Vector2.Lerp(points[0], points[1], T);
        }
    }
}
