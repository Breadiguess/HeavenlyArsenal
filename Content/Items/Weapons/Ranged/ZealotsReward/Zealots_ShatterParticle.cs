using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_ShatterParticle : BaseParticle
    {

        public static ParticlePool<Zealots_ShatterParticle> pool = new(500, GetNewParticle<Zealots_ShatterParticle>);

        Vector2 Pos;
        Vector2 Velocity;
        int TimeLeft;
        int TimeMax;
        public void Prepare(Vector2 Position, Vector2 Velocity, int TimeMax)
        {
            Pos = Position;
            this.Velocity = Velocity;
            this.TimeMax = TimeMax;
            TimeLeft = TimeMax;

        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            Pos += Velocity;
            Velocity *= 0.98f;

            if (TimeLeft-- < 0)
                ShouldBeRemovedFromRenderer = true;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            var tex = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;

            float interp = TimeLeft / (float)TimeMax;
            
            interp = QuadInOut(interp);
            interp = MathF.Pow(interp, 4);
            interp = 1 - interp;
            Vector2 DrawPos = Pos - Main.screenPosition;

            spritebatch.PrepareForShaders(BlendState.Additive);
            var FrostBuildup = ShaderManager.GetShader("HeavenlyArsenal.StasisBurstShader");
            FrostBuildup.SetTexture(tex, 0);
            FrostBuildup.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1);
            FrostBuildup.TrySetParameter("fragmentProgress", interp);
            FrostBuildup.TrySetParameter("fragmentStrength", 0);
            FrostBuildup.TrySetParameter("edgeWidth", 0.1f);
            FrostBuildup.TrySetParameter("noiseScale", 0.9f);
            FrostBuildup.TrySetParameter("edgeColor", Color.Black.ToVector4());
            FrostBuildup.Apply();

            Vector2 scale = new Vector2(1) * interp;
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, 0, tex.Size() / 2, scale, 0);


            spritebatch.ResetToDefault();
        }
        private static float QuadInOut(float x)
        {
            if (x < 0.5f)
                return 2f * x * x;
            else
                return 1f - 2f * (1f - x) * (1f - x);
        }
    }
}
