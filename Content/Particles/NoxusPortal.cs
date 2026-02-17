using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

public class NoxusPortal : BaseParticle
{
    public static ParticlePool<NoxusPortal> pool = new(200, GetNewParticle<NoxusPortal>);

    public float Rotation;

    public Vector2 Position;

    public Vector2 Velocity;

    public float Scale;

    public int TimeLeft;

    public float PortalInterp;

    public Projectile Bolt;

    public void Prepare(Vector2 position, Vector2 velocity, Color color, float rotation, int TimeLeft, float portalInterp, Projectile bolt)
    {
        Position = position;
        Velocity = velocity;

        Rotation = rotation;
        PortalInterp = portalInterp;
        Bolt = bolt;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;

        Scale = 5f;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        if (Bolt != null && Bolt.active && Bolt.ModProjectile is EntropicBlast entropicBlast)
        {
            TimeLeft = Bolt.timeLeft;
            Rotation = Bolt.rotation;

            var blast = Bolt.ModProjectile as EntropicBlast;
            PortalInterp = blast.portalInterp;

            Position = blast.SpawnPos;
        }
        else
        {
            ShouldBeRemovedFromRenderer = true;

            return;
        }

        if (TimeLeft <= 0 || PortalInterp == 0)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public void DrawPortal(Vector2 DrawPos)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");

        PortalShader.TrySetParameter("circleStretchInterpolant", PortalInterp);
        PortalShader.TrySetParameter("transformation", Matrix.CreateScale(3f, 1f, 1f));
        PortalShader.TrySetParameter("aimDirection", Rotation + MathHelper.PiOver2);

        PortalShader.TrySetParameter
        (
            "uColor",
            Color.MediumPurple with
            {
                A = 0
            }
        );

        PortalShader.TrySetParameter("uSecondaryColor", Color.White);
        PortalShader.TrySetParameter("edgeFadeInSharpness", 0.3f);

        PortalShader.TrySetParameter("uProgress", PortalInterp * Main.GlobalTimeWrappedHourly + TimeLeft);
        PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
        PortalShader.TrySetParameter("uIntensity", Math.Sin(Main.GlobalTimeWrappedHourly));

        PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 1);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.CheckeredNoise, 0);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2);
        PortalShader.SetTexture(GennedAssets.Textures.Extra.Void, 3);
        //PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.Spikes, 4);

        PortalShader.Apply();
        Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        var maxScale = 6.5f;
        var textureArea = Bolt.Size / pixel.Size() * maxScale;
        var scaleMod = 0.5f; // + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f + (Bolt.identity)) * 0.012f);
        textureArea *= scaleMod;

        Main.spriteBatch.Draw(pixel, DrawPos, null, Color.White, Rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        //Utils.DrawBorderString(Main.spriteBatch, "Interp: " + PortalInterp.ToString() + " | Pos: " + Position.ToString(), DrawPos - Vector2.UnitY * 120, Color.AntiqueWhite, 1);
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch)
    {
        DrawPortal(Position - Main.screenPosition);
    }
}