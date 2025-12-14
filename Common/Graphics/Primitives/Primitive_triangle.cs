namespace HeavenlyArsenal.Common.Graphics.Primitives;

public class TriangleShape : PrimitiveShape
{
    public TriangleShape(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        AddVertex(p1);
        AddVertex(p2);
        AddVertex(p3);
        AddEdge(0, 1);
        AddEdge(1, 2);
        AddEdge(2, 0);
        GenerateMesh();
    }

    public override void GenerateMesh()
    {
        base.GenerateMesh();
    }
}