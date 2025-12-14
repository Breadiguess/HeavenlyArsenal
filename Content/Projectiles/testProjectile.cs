using System.Collections.Generic;
using System.Linq;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Projectiles;

// Minimal PrimitiveShape base
public abstract class PrimitiveShape
{
    public Color Color = Color.OrangeRed;

    // GPU-side arrays filled by GenerateMesh
    public VertexPositionColor[] VertexBuffer { get; protected set; } = Array.Empty<VertexPositionColor>();

    public short[] IndexBuffer { get; protected set; } = Array.Empty<short>();

    public virtual int VertexCount => vertices.Count;

    public virtual int LineCount => edges.Count;

    protected List<Vector2> vertices = new();

    protected List<(int, int)> edges = new();

    public virtual void AddVertex(Vector2 v)
    {
        vertices.Add(v);
    }

    public virtual void UpdateVertex(int idx, Vector2 v)
    {
        vertices[idx] = v;
    }

    public virtual void AddEdge(int a, int b)
    {
        edges.Add((a, b));
    }

    public virtual void Clear()
    {
        vertices.Clear();
        edges.Clear();
    }

    // Convert stored 2D world-space points into screen-space VertexPositionColor + indices for line list
    // NOTE: Pass in Main.screenPosition to convert to screen coordinates here so the shape can be in world space.
    public virtual void GenerateMesh(Vector2 screenOffset)
    {
        VertexBuffer = vertices
            .Select(v => new VertexPositionColor(new Vector3(v - screenOffset, 0f), Color))
            .ToArray();

        var idx = new List<short>(edges.Count * 2);

        foreach (var (a, b) in edges)
        {
            idx.Add((short)a);
            idx.Add((short)b);
        }

        IndexBuffer = idx.ToArray();
    }

    // Draw using a BasicEffect configured externally. primitiveCount must be the number of lines.
    public virtual void Draw(GraphicsDevice device, BasicEffect effect)
    {
        if (VertexBuffer.Length == 0 || IndexBuffer.Length == 0)
        {
            return;
        }

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            device.DrawUserIndexedPrimitives
            (
                PrimitiveType.LineList,
                VertexBuffer,
                0,
                VertexCount,
                IndexBuffer,
                0,
                LineCount
            );
        }
    }
}

public class WeirdShape : PrimitiveShape
{
    public WeirdShape(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Color color)
    {
        Color = color;
        AddVertex(p1);
        AddVertex(p2);
        AddVertex(p3);
        AddVertex(p4);
        AddVertex(p5);
        // Create edges between vertices
        AddEdge(0, 1);
        AddEdge(1, 2);
        AddEdge(2, 3);
        AddEdge(3, 4);
        AddEdge(4, 0);
    }
}

public class TriangleShape : PrimitiveShape
{
    public TriangleShape(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        Color = color;
        AddVertex(p1);
        AddVertex(p2);
        AddVertex(p3);
        AddEdge(0, 1);
        AddEdge(1, 2);
        AddEdge(2, 0);
    }

    // Optionally expose a helper to set vertices easily
    public void SetPoints(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        if (vertices.Count < 3)
        {
            vertices = new List<Vector2>
            {
                p1,
                p2,
                p3
            };
        }
        else
        {
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
        }
    }
}

// Example ModProjectile that draws a dynamic triangle around itself
public class TriangleProjectile : ModProjectile
{
    private TriangleShape tri;

    private BasicEffect basicEffect;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        // display name, etc.
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.aiStyle = 0;
    }

    public override void AI()
    {
        if (tri == null)
        {
            tri = new TriangleShape
            (
                Projectile.Center + new Vector2(0, -16),
                Projectile.Center + new Vector2(-12, -8),
                Projectile.Center + new Vector2(12, 8),
                Color.Orange
            );

            basicEffect = new BasicEffect(Main.graphics.GraphicsDevice)
            {
                TextureEnabled = false,
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        var rot = Main.GameUpdateCount * 0.02f; // rotating value
        var radius = 16f;
        var p1 = Projectile.Center + new Vector2((float)Math.Cos(rot) * radius, (float)Math.Sin(rot) * radius);
        var p2 = Projectile.Center + new Vector2((float)Math.Cos(rot + MathHelper.TwoPi / 3f) * radius, (float)Math.Sin(rot + MathHelper.TwoPi / 3f) * radius);
        var p3 = Projectile.Center + new Vector2((float)Math.Cos(rot + 2f * MathHelper.TwoPi / 3f) * radius, (float)Math.Sin(rot + 2f * MathHelper.TwoPi / 3f) * radius);

        tri.SetPoints(p1, p2, p3);
        // We could call GenerateMesh here, but we do it inside PostDraw where screen position is known.
    }

    // Draw after the projectile has been updated. Choose PostDraw to draw in world coordinates easily.
    public override void PostDraw(Color lightColor)
    {
        if (tri == null || basicEffect == null)
        {
            return;
        }

        var device = Main.graphics.GraphicsDevice;

        // Setup BasicEffect (projection for screen-space drawing)
        basicEffect.World = Matrix.Identity;
        basicEffect.View = Matrix.Identity;

        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1f);
        basicEffect.VertexColorEnabled = true;
        basicEffect.TextureEnabled = false;

        Utils.DrawBorderString(Main.spriteBatch, basicEffect.Projection.ToString(), Projectile.Center - Main.screenPosition, Color.AntiqueWhite);
        tri.GenerateMesh(Main.screenPosition);

        // keep
        var prevRaster = device.RasterizerState;
        var prevBlend = device.BlendState;
        var prevDepth = device.DepthStencilState;

        device.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.None
        };

        device.BlendState = BlendState.AlphaBlend;
        device.DepthStencilState = DepthStencilState.None;

        // Draw the triangle wireframe
        tri.Draw(device, basicEffect);

        // restore render states
        device.RasterizerState = prevRaster;
        device.BlendState = prevBlend;
        device.DepthStencilState = prevDepth;
    }

    public override void OnKill(int timeLeft)
    {
        // clean up effect if necessary
        basicEffect?.Dispose();
        basicEffect = null;
    }
}