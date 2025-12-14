using Luminance.Core.Graphics;

namespace HeavenlyArsenal.Common.Graphics;

[Autoload(Side = ModSide.Client)]
//im so sorry lucille :Pensive:
public class MoreIndepthRadialScreenDistortionSystem : ModSystem
{
    public struct ScreenDistortion
    {
        public Vector2 Position;

        public float LifetimeRatio;

        public float StartRadius;

        public float EndRadius;
    }

    /// <summary>
    ///     The set of all active distortion effects that have been performed.
    /// </summary>
    public static ScreenDistortion[] Distortions { get; } = new ScreenDistortion[15];

    public override void PostUpdatePlayers()
    {
        for (var i = 0; i < Distortions.Length; i++)
        {
            Distortions[i].LifetimeRatio += 0.064f;
        }
    }

    /// <summary>
    ///     Attempts to create a new distortion effect at a given world position.
    /// </summary>
    /// <param name="position">The world position of the distortion effect.</param>
    /// <param name="maxRadius">The maximum radius of the distortion effect.</param>
    public static void CreateDistortion(Vector2 position, float startRadius, float endRadius)
    {
        var freeIndex = -1;

        for (var i = 0; i < Distortions.Length; i++)
        {
            if (Distortions[i].LifetimeRatio >= 1f)
            {
                freeIndex = i;
            }
        }

        if (freeIndex >= 0)
        {
            Distortions[freeIndex] = new ScreenDistortion
            {
                Position = position,
                StartRadius = startRadius,
                EndRadius = endRadius
            };
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        var anyInUse = false;
        var lifetimeRatios = new float[Distortions.Length];
        var maxRadii = new float[Distortions.Length];
        var positions = new Vector2[Distortions.Length];

        for (var i = 0; i < positions.Length; i++)
        {
            lifetimeRatios[i] = Distortions[i].LifetimeRatio;
            //Distortions[i].StartRadius = float.Lerp(Distortions[i].StartRadius, Distortions[i].EndRadius, 0.02f);
            maxRadii[i] = Distortions[i].StartRadius * Main.GameViewMatrix.Zoom.X;
            positions[i] = Vector2.Transform(Distortions[i].Position - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);

            if (lifetimeRatios[i] > 0f && lifetimeRatios[i] < 1f)
            {
                anyInUse = true;
            }
        }

        if (!anyInUse)
        {
            return;
        }

        var distortionShader = ShaderManager.GetFilter("HeavenlyArsenal.LocalScreenDistortionShader");
        distortionShader.TrySetParameter("lifetimeRatios", lifetimeRatios);
        distortionShader.TrySetParameter("maxRadii", maxRadii);
        distortionShader.TrySetParameter("positions", positions);
        distortionShader.SetTexture(AssetDirectory.Textures.VoidLake, 1, SamplerState.LinearWrap);
        distortionShader.Activate();
    }
}