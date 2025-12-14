using System.Runtime.CompilerServices;
using Luminance.Core.Graphics;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using MatrixSIMD = System.Numerics.Matrix4x4;
using Vector2SIMD = System.Numerics.Vector2;

namespace HeavenlyArsenal.Common.Graphics;

[Autoload(Side = ModSide.Client)]
public class FramedFastParticleSystem(int totalFrames, int maxParticles, Action renderPreparations, FastParticleSystem.ParticleUpdateAction extraUpdates = null) :
    FastParticleSystem(maxParticles, renderPreparations, extraUpdates)
{
    /// <summary>
    ///     The amount of frames this particle has.
    /// </summary>
    public readonly int TotalFrames = totalFrames;

    protected override void PopulateVertexBufferIndex(VertexPosition2DColorTexture[] vertices, int particleIndex)
    {
        ref var particle = ref particles[particleIndex];

        var fadeIn = LumUtils.InverseLerp(0f, 36f, particle.Time);
        var color = (particle.Active ? particle.Color : Color.Transparent) * fadeIn;
        var center = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Position);
        var size = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Size) * fadeIn;

        var rotationX = Math.Clamp(particle.Rotation * 1.1f, -0.3f, 0.3f);
        var rotationY = Math.Clamp(particle.Rotation * 0.5f, -0.3f, 0.3f);

        var particleRotationMatrix = MatrixSIMD.CreateRotationX(rotationX) *
                                     MatrixSIMD.CreateRotationY(rotationY) *
                                     MatrixSIMD.CreateRotationZ(particle.Rotation);

        var topLeftPosition = center + Vector2SIMD.Transform(topLeftOffset * size, particleRotationMatrix);
        var topRightPosition = center + Vector2SIMD.Transform(topRightOffset * size, particleRotationMatrix);
        var bottomLeftPosition = center + Vector2SIMD.Transform(bottomLeftOffset * size, particleRotationMatrix);
        var bottomRightPosition = center + Vector2SIMD.Transform(bottomRightOffset * size, particleRotationMatrix);

        var frameY = particleIndex % TotalFrames;
        var topY = frameY / (float)TotalFrames;
        var bottomY = (frameY + 1f) / TotalFrames;

        var vertexIndex = particleIndex * 4;
        vertices[vertexIndex] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topLeftPosition), color, Vector2.UnitY * topY, frameY);
        vertices[vertexIndex + 1] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topRightPosition), color, new Vector2(1f, topY), frameY);
        vertices[vertexIndex + 2] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomRightPosition), color, new Vector2(1f, bottomY), frameY);
        vertices[vertexIndex + 3] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomLeftPosition), color, Vector2.UnitY * bottomY, frameY);
    }
}