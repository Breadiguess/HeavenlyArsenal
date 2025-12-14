using System.Collections.Generic;
using System.Linq;

namespace HeavenlyArsenal.Common.Graphics.Primitives;

public abstract class PrimitiveShape
{
    // Current color for rendering the vertices/wireframe
    public Color Color = Color.White;

    // Number of vertices
    public virtual int VertexCount => vertices.Count;

    // GPU data arrays (filled by GenerateMesh)
    public VertexPositionColor[] VertexBuffer { get; protected set; }

    public short[] IndexBuffer { get; protected set; }

    // List of 2D points (x,VerticalOffset) making up the shape
    protected List<Vector2> vertices = new();

    // Edges as pairs of vertex indices, defining the wireframe lines
    protected List<(int, int)> edges = new();

    // Methods to add/remove/update points
    public virtual void AddVertex(Vector2 v)
    {
        vertices.Add(v);
    }

    public virtual void RemoveVertex(int index)
    {
        vertices.RemoveAt(index);
    }

    public virtual void UpdateVertex(int i, Vector2 v)
    {
        vertices[i] = v;
    }

    // Methods to add/remove edges (connecting vertices)
    public virtual void AddEdge(int indexA, int indexB)
    {
        edges.Add((indexA, indexB));
    }

    public virtual void RemoveEdge(int index)
    {
        edges.RemoveAt(index);
    }

    // Builds the mesh from current vertices and edges.
    public virtual void GenerateMesh()
    {
        // Convert 2D vertices to VertexPositionColor (with z=0 and current color)
        VertexBuffer = vertices
            .Select(p => new VertexPositionColor(new Vector3(p, 0f), Color))
            .ToArray();

        // Build index list: each edge yields two indices for a line
        var indexList = new List<short>();

        foreach (var (a, b) in edges)
        {
            indexList.Add((short)a);
            indexList.Add((short)b);
        }

        IndexBuffer = indexList.ToArray();
    }

    // Draws the wireframe using MonoGame; assumes a BasicEffect is already set up.
    public virtual void Draw(GraphicsDevice device, BasicEffect basicEffect)
    {
        // Apply the effect (e.g. with VertexColorEnabled = true:contentReference[oaicite:1]{index=1})
        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // Draw as a list of lines: each edge is an independent line segment:contentReference[oaicite:2]{index=2}
            device.DrawUserIndexedPrimitives
            (
                PrimitiveType.LineList,
                VertexBuffer, // vertex array
                0,
                VertexCount,
                IndexBuffer, // index array (2 indices per line)
                0,
                edges.Count
            );
        }
    }
}