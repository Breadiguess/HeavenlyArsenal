using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace HeavenlyArsenal.Core.Physics.ClothManagement;

public class ClothSimulation
{
    internal readonly ClothPoint?[,] particleGrid;

    /// <summary>
    /// The dampening coefficient of this simulation.
    /// </summary>
    public float DampeningCoefficient
    {
        get;
        set;
    }

    /// <summary>
    /// The width of the simulation grid.
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// The height of the simulation grid.
    /// </summary>
    public readonly int Height;

    /// <summary>
    /// The particles that compose this simulation.
    /// </summary>
    public List<ClothPoint> Particles = new List<ClothPoint>(1024);

    /// <summary>
    /// The springs that compose this simulation.
    /// </summary>
    public List<ClothSpring> Springs = new List<ClothSpring>(1024);

    // Creates a grid of particles (cloth) with springs connecting neighbors.
    public ClothSimulation(Vector3 center, int width, int height, float spacing, float stiffness, float dampeningCoefficient)
    {
        // Create a grid of particles.
        particleGrid = new ClothPoint[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Particles.Add(new ClothPoint(new Point(x, y), center));
                particleGrid[x, y] = Particles.Last();
            }
        }

        // Create structural springs (horizontal and vertical connections).
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                // Horizontal spring.
                if (x < width - 1)
                    Springs.Add(new ClothSpring(Particles[index], Particles[index + 1], stiffness, spacing));

                // Vertical spring.
                if (y < height - 1)
                    Springs.Add(new ClothSpring(Particles[index], Particles[index + width], stiffness, spacing));
            }
        }
        Width = width;
        Height = height;
        DampeningCoefficient = dampeningCoefficient;
    }

    // Advances the simulation by one time step.
    public void Simulate(float dt, bool collision, Vector3 gravity)
    {
        foreach (ClothPoint p in Particles)
        {
            float xInterpolant = MathF.Sin(MathHelper.Pi * p.X / Width);
            float gravityFactor = MathHelper.Lerp(0.06f, 1f, MathF.Pow(1f - xInterpolant, 2.7f));
            p.AddForce(gravity * gravityFactor);
        }

        // Apply spring forces.
        foreach (ClothSpring s in Springs)
        {
            s.ApplyForce();
        }

        // Update each particle's position.
        foreach (var p in Particles)
        {
            p.Update(dt, collision, DampeningCoefficient);
        }
    }

    public void Render()
    {
        int[] indices = new int[(Width - 1) * (Height - 1) * 6];
        VertexPositionColorNormalTexture[] vertices = new VertexPositionColorNormalTexture[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                ClothPoint? point = particleGrid[x, y];
                if (point is null)
                    continue;

                Vector3 up = y < Height - 1 ? particleGrid[x, y + 1]!.Position : particleGrid[x, y - 1]!.Position;
                Vector3 side = x < Width - 1 ? particleGrid[x + 1, y]!.Position : particleGrid[x - 1, y]!.Position;
                Vector3 a = up - point.Position;
                Vector3 b = side - point.Position;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(b, a));
                normal.Z = MathF.Abs(normal.Z) * MathF.Sign(point.Position.Z);

                point.Normal = normal;

                vertices[y * Width + x] = new VertexPositionColorNormalTexture(point.Position, Color.White, new Vector2(x / (float)Width, y / (float)Height), point.Normal);
            }
        }

        int index = 0;
        for (int x = 0; x < Width - 1; x++)
        {
            for (int y = 0; y < Height - 1; y++)
            {
                int topLeft = y * Width + x;
                int topRight = y * Width + x + 1;
                int bottomLeft = (y + 1) * Width + x;
                int bottomRight = (y + 1) * Width + x + 1;

                // Triangle 1 (Top Left, Top Right, Bottom Left).
                indices[index++] = topLeft;
                indices[index++] = topRight;
                indices[index++] = bottomLeft;

                // Triangle 2 (Bottom Right, Bottom Left, Top Right).
                indices[index++] = bottomLeft;
                indices[index++] = topRight;
                indices[index++] = bottomRight;
            }
        }

        Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (Width - 1) * (Height - 1) * 2);
    }
}
