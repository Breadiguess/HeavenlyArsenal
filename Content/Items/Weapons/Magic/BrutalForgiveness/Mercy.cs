using Luminance.Assets;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.Graphics;
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
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
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
        Projectile.Bottom = target.Top - Vector2.UnitY * 32f;
        Projectile.Opacity = LumUtils.InverseLerp(0f, 12f, Time);
        Projectile.scale = MathHelper.SmoothStep(2f, pulse + 0.85f, Projectile.Opacity);

        Projectile.Opacity *= target.Opacity;

        Time++;
    }

    /// <summary>
    /// Renders this text.
    /// </summary>
    public void RenderSelf()
    {
        var font = FontRegistry.Instance.NamelessDeityText;
        string text = DisplayName.Value;
        Color textColor = Projectile.GetAlpha(DialogColorRegistry.NamelessDeityTextColor);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, drawPosition, textColor, 0f, font.MeasureString(text) * 0.5f, Vector2.One * Projectile.scale, -1, 1f);
    }
}
