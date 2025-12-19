using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.Automators;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;

internal class SolynBarrier : ModProjectile, IProjOwnedByBoss<BattleSolyn>, IDrawsWithShader
{
    public NPC Solyn;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        var forcefieldShader = ShaderManager.GetShader("NoxusBoss.SolynForcefieldShader");
        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);

        var WhitePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel.Value;
        var palette = HomingStarBolt.StarPalette;

        forcefieldShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut.Value, 1, SamplerState.LinearWrap);
        forcefieldShader.TrySetParameter("forcefieldPalette", palette);
        forcefieldShader.TrySetParameter("forcefieldPaletteLength", palette.Length);
        forcefieldShader.TrySetParameter("shapeInstability", 0); //(Projectile.scale - 1f) * 0.07f + 0.012f);
        forcefieldShader.TrySetParameter("flashInterpolant", 0f);
        forcefieldShader.TrySetParameter("bottomFlattenInterpolant", 0f);
        forcefieldShader.Apply();

        var drawPosition = Projectile.Center - Main.screenPosition;

        Main.spriteBatch.Draw
            (WhitePixel, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, WhitePixel.Size() * 0.5f, Projectile.Size / 2 / WhitePixel.Size() * Projectile.scale, 0, 0f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    public override void SetDefaults()
    {
        Projectile.damage = 0;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.Size = new Vector2(100, 100);
        Projectile.timeLeft = 4500;
        Projectile.scale = 2;
    }

    public override void AI()
    {
        Projectile.Size = new Vector2(100, 100);

        Projectile.Center = Solyn.Center;

        if (voidVulture.Myself != null)
        {
            if (voidVulture.Myself.As<voidVulture>().currentState != voidVulture.Behavior.placeholder2)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 0, 0.2f);
            }
        }

        if (Projectile.scale < 0.1f)
        {
            Projectile.active = false;
        }

        ProtectFromCommet();
    }

    public void ProtectFromCommet()
    {
        foreach (var proj in Main.ActiveProjectiles)
        {
            if (proj.type != ModContent.ProjectileType<IntersectingComet>())
            {
                continue;
            }

            if (proj.Distance(Projectile.Center) < 120)
            {
                proj.velocity = Vector2.Zero;
            }
        }
    }
}