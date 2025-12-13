using HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.Automators;
using System;
using Terraria;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;

public class SolynButterflyBarrier : ModProjectile, IProjOwnedByBoss<BattleSolynBird>, IDrawsWithShader
{
    /// <summary>
    /// How long this forcefield has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The owner of this forcefield.
    /// </summary>
    public Player Owner => Main.player[(int)Projectile.ai[1]];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 440;
        Projectile.height = 440;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 999999;
        Projectile.Opacity = 0f;
    }

    public override void AI()
    {
        if (!Owner.active || Owner.dead || Owner.GetModPlayer<ButterflyMinionPlayer>().ButterflyBarrierActive == false)
        {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft++;
       
        Time++;
        Projectile.scale = 0.35f;//Utils.Remap(Time, 0f, 25f, 2f, (float)Math.Cos(MathHelper.TwoPi * Time / 7f) * 0.05f + 0.6f) + InverseLerp(20f, 0f, Projectile.timeLeft) * 1.1f;
        Projectile.Opacity = 1;//InverseLerp(0f, 30f, Time) * InverseLerp(0f, 20f, Projectile.timeLeft);
        Projectile.Center = Vector2.Lerp(Projectile.Center,Owner.Center,0.9f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        ManagedShader forcefieldShader = ShaderManager.GetShader("NoxusBoss.SolynForcefieldShader");
        Main.spriteBatch.End();


        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);

        Texture2D WhitePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel.Value;
        Vector4[] palette = HomingStarBolt.StarPalette;
      
        forcefieldShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
        forcefieldShader.TrySetParameter("forcefieldPalette", palette);
        forcefieldShader.TrySetParameter("forcefieldPaletteLength", palette.Length);
        forcefieldShader.TrySetParameter("shapeInstability", 0);//(Projectile.scale - 1f) * 0.07f + 0.012f);
        forcefieldShader.TrySetParameter("flashInterpolant", 0f);
        forcefieldShader.TrySetParameter("bottomFlattenInterpolant", 0f);
        forcefieldShader.Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, WhitePixel.Size() * 0.5f, Projectile.Size / WhitePixel.Size() * Projectile.scale, 0, 0f);


        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

    }
}
