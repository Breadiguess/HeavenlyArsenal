using Microsoft.Xna.Framework;

namespace HeavenlyArsenal.Core.Physics.ClothManagement;

/// <summary>
/// Represents a spring on a grid that keeps two cloth points together.
/// </summary>
public class ClothSpring
{
    /// <summary>
    /// The first point.
    /// </summary>
    public ClothPoint P1;

    /// <summary>
    /// The second point.
    /// </summary>
    public ClothPoint P2;

    /// <summary>
    /// The desired spacing for this spring.
    /// </summary>
    public float RestLength;

    /// <summary>
    /// The stiffness of this spring.
    /// </summary>
    public float Stiffness;

    public ClothSpring(ClothPoint p1, ClothPoint p2, float stiffness, float restLength)
    {
        P1 = p1;
        P2 = p2;
        RestLength = restLength;
        Stiffness = stiffness;
    }

    public void ApplyForce()
    {
        Vector3 delta = P2.Position - P1.Position;
        float currentLength = delta.Length();
        float extension = currentLength - RestLength;

        if (currentLength == 0f)
            return;

        // Force magnitude from Hooke's law: F = -k * extension.
        Vector3 force = delta / currentLength * (extension * Stiffness);

        if (!P1.IsFixed)
            P1.AddForce(force);
        if (!P2.IsFixed)
            P2.AddForce(-force);
    }
}
