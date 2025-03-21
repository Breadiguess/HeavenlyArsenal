using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.RenderTargets;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class Mercy : ModProjectile
{
    /// <summary>
    /// The cloth sim responsible for the rendering of the ofuda paper that encondes this text.
    /// </summary>
    public ClothSimulation Ofuda
    {
        get;
        set;
    }

    /// <summary>
    /// The render target that contains text that should be rendered on the ofuda.
    /// </summary>
    public static InstancedRequestableTarget TextTarget
    {
        get;
        set;
    }

    /// <summary>
    /// The font used by this text.
    /// </summary>
    public static Asset<DynamicSpriteFont> MercyFont
    {
        get;
        private set;
    }

    /// <summary>
    /// The owner of this text.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    /// The NPC that this text should hover over.
    /// </summary>
    public ref float TargetIndex => ref Projectile.ai[0];

    /// <summary>
    /// How long this text has existed for.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;
        MercyFont = Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/MercyText");

        Main.ContentThatNeedsRenderTargets.Add(TextTarget = new InstancedRequestableTarget());
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 10000000;
        Projectile.penetrate = -1;
        Projectile.Opacity = 0f;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (Owner.ownedProjectileCounts[ModContent.ProjectileType<BrutalForgivenessProjectile>()] <= 0 &&
            Owner.ownedProjectileCounts[ModContent.ProjectileType<BrutalVine>()] <= 0)
        {
            Projectile.Kill();
            return;
        }

        if (TargetIndex <= -1 || TargetIndex >= Main.maxNPCs)
        {
            Projectile.Kill();
            return;
        }

        NPC target = Main.npc[(int)TargetIndex];
        if (!target.active)
        {
            Projectile.Kill();
            return;
        }

        float pulse = MathF.Sin(MathHelper.TwoPi * Time / 150f) * 0.04f;
        Projectile.Top = target.Center;
        Projectile.Opacity = LumUtils.InverseLerp(0f, 12f, Time);
        Projectile.scale = Math.Clamp(target.width / 80f, 1f, 2.3f);

        Projectile.Opacity *= target.Opacity;

        UpdateOfuda();

        Time++;
    }

    /// <summary>
    /// Updates the cloth simulation that represents the ofuda that has this projectile's text.
    /// </summary>
    private void UpdateOfuda()
    {
        Ofuda ??= new ClothSimulation(new Vector3(Projectile.Center, 0f), 7, 17, Projectile.scale * 4f, 40f, 0.02f);

        int steps = 32;
        float windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 0f);
        Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(Time * 0.029f) * 0.67f + windSpeed) * 0.2f;
        for (int i = 0; i < steps; i++)
        {
            for (int x = 0; x < Ofuda.Width; x++)
            {
                for (int y = 0; y < 2; y++)
                    ConstrainParticle(Projectile.Top, Ofuda.particleGrid[x, y], 0f);
            }

            Ofuda.Simulate(0.04f, false, Vector3.UnitY * 10f + wind);
        }
    }

    private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
    {
        if (point is null)
            return;

        float xInterpolant = point.X / (float)Ofuda.Width;
        Vector3 ring = new Vector3((xInterpolant - 0.5f) * Projectile.scale * 25f, 0f, 0f);
        ring.Y += point.Y * 6f;

        point.Position = new Vector3(anchor, 0f) + ring;
        point.IsFixed = true;
    }

    /// <summary>
    /// Renders this text.
    /// </summary>
    public void RenderSelf()
    {
        var font = FontRegistry.Instance.NamelessDeityText;
        string text = DisplayName.Value;
        Color textColor = Projectile.GetAlpha(Color.Red);
        Vector2 textRenderTargetArea = font.MeasureString(text) + Vector2.One * 40f;
        TextTarget.Request((int)textRenderTargetArea.X, (int)textRenderTargetArea.Y, Projectile.whoAmI, () =>
        {
            Main.spriteBatch.Begin();

            Vector2 drawPosition = WotGUtils.ViewportSize * 0.5f;
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, drawPosition, textColor, 0f, font.MeasureString(text) * 0.5f, Vector2.One, -1, 1f);

            Main.spriteBatch.End();
        });

        if (TextTarget.TryGetTarget(Projectile.whoAmI, out RenderTarget2D target) && target is not null)
        {
            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) * Main.GameViewMatrix.TransformationMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);
            Matrix matrix = world * projection;

            ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.MercyOfudaShader");
            clothShader.TrySetParameter("opacity", 1f);
            clothShader.TrySetParameter("transform", matrix);
            clothShader.TrySetParameter("gameZoom", Main.GameViewMatrix.Zoom);
            clothShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
            clothShader.SetTexture(GennedAssets.Textures.Extra.Paper, 1, SamplerState.LinearWrap);
            clothShader.SetTexture(target, 2, SamplerState.LinearWrap);
            clothShader.SetTexture(LightingMaskTargetManager.LightTarget, 3);
            clothShader.Apply();

            Ofuda?.Render();
        }
    }
}
