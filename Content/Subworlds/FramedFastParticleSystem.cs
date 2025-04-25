using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using System;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;
using MatrixSIMD = System.Numerics.Matrix4x4;
using Vector2SIMD = System.Numerics.Vector2;

namespace HeavenlyArsenal.Content.Subworlds;

// TODO -- Move this file somewhere else, it should not be in the Subworlds directory.
[Autoload(Side = ModSide.Client)]
public class FramedFastParticleSystem(int totalFrames, int maxParticles, Action renderPreparations, FastParticleSystem.ParticleUpdateAction extraUpdates = null) :
    FastParticleSystem(maxParticles, renderPreparations, extraUpdates)
{
    /// <summary>
    /// The amount of frames this particle has.
    /// </summary>
    public readonly int TotalFrames = totalFrames;

    protected override void PopulateVertexBufferIndex(VertexPosition2DColorTexture[] vertices, int particleIndex)
    {
        ref FastParticle particle = ref particles[particleIndex];

        float fadeIn = LumUtils.InverseLerp(0f, 36f, particle.Time);
        Color color = (particle.Active ? particle.Color : Color.Transparent) * fadeIn;
        Vector2SIMD center = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Position);
        Vector2SIMD size = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Size) * fadeIn;

        float rotationX = Math.Clamp(particle.Rotation * 1.1f, -0.3f, 0.3f);
        float rotationY = Math.Clamp(particle.Rotation * 0.5f, -0.3f, 0.3f);
        MatrixSIMD particleRotationMatrix = MatrixSIMD.CreateRotationX(rotationX) *
                                            MatrixSIMD.CreateRotationY(rotationY) *
                                            MatrixSIMD.CreateRotationZ(particle.Rotation);

        Vector2SIMD topLeftPosition = center + Vector2SIMD.Transform(topLeftOffset * size, particleRotationMatrix);
        Vector2SIMD topRightPosition = center + Vector2SIMD.Transform(topRightOffset * size, particleRotationMatrix);
        Vector2SIMD bottomLeftPosition = center + Vector2SIMD.Transform(bottomLeftOffset * size, particleRotationMatrix);
        Vector2SIMD bottomRightPosition = center + Vector2SIMD.Transform(bottomRightOffset * size, particleRotationMatrix);

        int frameY = particleIndex % TotalFrames;
        float topY = frameY / (float)TotalFrames;
        float bottomY = (frameY + 1f) / TotalFrames;

        int vertexIndex = particleIndex * 4;
        vertices[vertexIndex] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topLeftPosition), color, Vector2.UnitY * topY, frameY);
        vertices[vertexIndex + 1] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topRightPosition), color, new Vector2(1f, topY), frameY);
        vertices[vertexIndex + 2] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomRightPosition), color, new Vector2(1f, bottomY), frameY);
        vertices[vertexIndex + 3] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomLeftPosition), color, Vector2.UnitY * bottomY, frameY);
    }
}
