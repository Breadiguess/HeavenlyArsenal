using HeavenlyArsenal.Core;
using HeavenlyArsenal.Content.Particles;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using Terraria.Graphics.Renderers;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.DataStructures;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

namespace HeavenlyArsenal.Content.Particles;

public class NoxusPortal : BaseParticle
{
    public static ParticlePool<NoxusPortal> pool = new ParticlePool<NoxusPortal>(200, GetNewParticle<NoxusPortal>);

    
    public float Rotation;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
   
    public int TimeLeft;
    public float PortalInterp;
    public Projectile? Bolt;

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
            
            var portalInterpolant = typeof(EntropicBlast).GetField("portalInterp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (portalInterpolant != null)
            {
                PortalInterp = (float)portalInterpolant.GetValue(entropicBlast);

            }
            var PortalPos = typeof(EntropicBlast).GetField("SpawnPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (PortalPos != null)
            {
                Position = (Vector2)PortalPos.GetValue(entropicBlast);

            }
        }
        else
        {
            ShouldBeRemovedFromRenderer = true;
            return;
        }




        if (TimeLeft <= 0 || PortalInterp == 0)
        {
            ShouldBeRemovedFromRenderer = true;
            return;
        }
    }

    public void DrawPortal(Vector2 DrawPos)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        ManagedShader PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");

        PortalShader.TrySetParameter("circleStretchInterpolant", PortalInterp);
        PortalShader.TrySetParameter("transformation", (Matrix.CreateScale(3f, 1f, 1f)));
        //PortalShader.TrySetParameter("aimDirection", Projectile.rotation + MathHelper.PiOver2);
        PortalShader.TrySetParameter("uColor", Color.MediumPurple with { A = 0 });
        //PortalShader.TrySetParameter("uSecondaryColor", Color.White);
        PortalShader.TrySetParameter("edgeFadeInSharpness", 2.3f);

        //PortalShader.TrySetParameter("uProgress", portalInterp * Main.GlobalTimeWrappedHourly);
        PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
        PortalShader.TrySetParameter("uIntensity", Main.GlobalTimeWrappedHourly);

        PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 1);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 0);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2);
        PortalShader.SetTexture(GennedAssets.Textures.Extra.Void, 3);
        //PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.Spikes, 4);


        PortalShader.Apply();
        Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        float maxScale = 6.5f;
        Vector2 textureArea = (Bolt.Size) / pixel.Size() * maxScale;
        float scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f + (Bolt.identity)) * 0.012f);
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
