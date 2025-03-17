using Microsoft.Xna.Framework;
using Terraria;

namespace HeavenlyArsenal.Core.Physics.ClothManagement;

/// <summary>
/// Represents a point on a grid that composes a piece of cloth.
/// </summary>
public class ClothPoint
{
    /// <summary>
    /// The previous position of the point in space.
    /// </summary>
    public Vector3 PreviousPosition;

    /// <summary>
    /// The current position of the point in space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The normal direction of this cloth.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// The acceleration of this cloth.
    /// </summary>
    public Vector3 Acceleration;

    /// <summary>
    /// Dictates whether this cloth point should move or not.
    /// </summary>
    public bool IsFixed;

    /// <summary>
    /// The X grid position of this point.
    /// </summary>
    public readonly int X;

    /// <summary>
    /// The Y grid position of this point.
    /// </summary>
    public readonly int Y;

    public ClothPoint(Point gridPosition, Vector3 position, bool isFixed = false)
    {
        X = gridPosition.X;
        Y = gridPosition.Y;
        Position = position;
        PreviousPosition = position;
        IsFixed = isFixed;
        Acceleration = Vector3.Zero;
    }

    public void AddForce(Vector3 force)
    {
        Acceleration += force;
    }

    public void Update(float dt, bool collision, float dampingCoefficient)
    {
        if (IsFixed)
        {
            Acceleration = Vector3.Zero;
            return;
        }

        Vector3 offset = (Position - PreviousPosition) * (1f - dampingCoefficient) + Acceleration * dt * dt;
        if (collision && !IsFixed)
        {
            Vector2 collisionDetectedOffset = Collision.TileCollision(new Vector2(Position.X, Position.Y), new Vector2(offset.X, offset.Y), 2, 2);
            offset.X = collisionDetectedOffset.X;
            offset.Y = collisionDetectedOffset.Y;
        }

        Vector3 newPos = Position + offset;

        PreviousPosition = Position;
        Position = newPos;
        Acceleration = Vector3.Zero;
    }
}
