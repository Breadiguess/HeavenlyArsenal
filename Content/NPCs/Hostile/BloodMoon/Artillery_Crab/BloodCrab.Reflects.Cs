using Luminance.Core.Graphics;
using System.Linq;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {
        private List<ReflectionEffect> reflects = new();

        private ReflectionEffect[] GetReflectData(SpriteEffects flip)
        {
            int count = Math.Min(reflects.Count, 8);

            var newest = reflects
                .OrderByDescending(r => r.Lifetime)
                .Take(count)
                .ToArray();

            for (int i = 0; i < newest.Length; i++)
            {
                ReflectionEffect r = newest[i];

                r.Lifetime /= 25f; // normalize lifetime to 0f -> 1f
                if ((flip & SpriteEffects.FlipHorizontally) != 0)
                    r.Position.X = 1f - r.Position.X;

                newest[i] = r;
            }

            return newest;
        }

        private void PrepareReflectEffect(Texture2D tex, SpriteEffects flip)
        {
            ReflectionEffect[] refls = GetReflectData(flip);

            Vector4[] shaderReflects = refls
                .Select(r => new Vector4(r.Position.X, r.Position.Y, r.Size, r.Lifetime))
                .ToArray();
            Vector3[] shaderColors = refls
                .Select(r => new Vector3(r.Color.R / 255f, r.Color.G / 255f, r.Color.B / 255f))
                .ToArray();

            Texture2D mask = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Artillery_Crab/BloodCrabShellMask").Value;
            Texture2D wavyNoise = WavyBlotchNoiseDetailed.Value;
            Texture2D harshNoise = HarshCellNoise.Value;

            ManagedShader reflectShader = ShaderManager.GetShader("HeavenlyArsenal.BloodcrabReflect");
            reflectShader.SetTexture(mask, 1, SamplerState.LinearWrap);
            reflectShader.SetTexture(wavyNoise, 2, SamplerState.LinearWrap);
            reflectShader.SetTexture(harshNoise, 3, SamplerState.LinearWrap);
            reflectShader.TrySetParameter("reflects", shaderReflects);
            reflectShader.TrySetParameter("reflectColors", shaderColors);
            reflectShader.TrySetParameter("reflectCount", refls.Length);
            reflectShader.TrySetParameter("textureSize", tex.Size());
            reflectShader.Apply();
        }

        private void AddReflectData(Vector2 position, float radius, Color color, float lifetimeMultiplier = 1f)
        {
            Vector2 normalizedPos = (position + Vector2.One) * 0.5f; // -1 to 1 -> 0 to 1
            reflects.Add(new ReflectionEffect(normalizedPos, 25f * lifetimeMultiplier, radius, color));
        }
    }

    public struct ReflectionEffect
    {
        public Vector2 Position;
        public float Lifetime;
        public float Size;
        public Color Color;

        public ReflectionEffect(Vector2 position, float lifetime, float size, Color color)
        {
            this.Position = position;
            this.Lifetime = lifetime;
            this.Size = size;
            this.Color = color;
        }
    }
}
