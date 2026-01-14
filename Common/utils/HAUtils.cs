using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Common.utils
{
    internal class HAUtils
    {
        public static class BasicEffectRenderer
        {
            public static void DrawTexturedMesh(GraphicsDevice gd, ref BasicEffect effect, Texture2D texture, VertexPositionColorTexture[] vertices,
                short[] indices, Matrix world, Matrix? view = null, Matrix? projection = null, BlendState blendState = null, RasterizerState rasterizerState = null,
                DepthStencilState depthState = null,SamplerState samplerState = null)
            {
                if (vertices == null || vertices.Length == 0 || indices == null || indices.Length == 0)
                    return;

                // Lazy-create effect if needed
                effect ??= new BasicEffect(gd)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    LightingEnabled = false
                };

                // Assign effect state
                effect.Texture = texture;
                effect.World = world;
                effect.View = view ?? Matrix.Identity;
                effect.Projection = projection ??
                    Matrix.CreateOrthographicOffCenter(
                        0, Main.screenWidth,
                        Main.screenHeight, 0,
                        -1000f, 1000f
                    );

                // GraphicsDevice state
                gd.BlendState = blendState ?? BlendState.AlphaBlend;
                gd.RasterizerState = rasterizerState ?? RasterizerState.CullClockwise;
                gd.DepthStencilState = depthState ?? DepthStencilState.None;
                gd.SamplerStates[0] = samplerState ?? SamplerState.PointClamp;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        vertices, 0, vertices.Length,
                        indices, 0, indices.Length / 3
                    );
                }
            }
        }

    }
}
