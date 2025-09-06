using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Particles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;

public class SolynSentientStar : ModProjectile, IPixelatedPrimitiveRenderer
{
    /// <summary>
    /// Whether this star should be rendered with afterimages.
    /// </summary>
    public bool UseAfterimages
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this star should be rendered over players.
    /// </summary>
    public bool RenderOverPlayers
    {
        get;
        set;
    }

    /// <summary>
    /// How long this star has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

  

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 18;
    }

    public override void SetDefaults()
    {
        Projectile.width = 38;
        Projectile.height = 38;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 999999;
        Projectile.penetrate = -1;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Projectile.localAI[0]);
        writer.Write(Projectile.localAI[1]);
        writer.Write(Projectile.localAI[2]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.localAI[0] = reader.ReadSingle();
        Projectile.localAI[1] = reader.ReadSingle();
        Projectile.localAI[2] = reader.ReadSingle();
    }

    public override void AI()
    {
      

        Projectile.hide = RenderOverPlayers;

        // Release star particles.
        if (Main.rand.NextBool(3))
        {
            int starPoints = Main.rand.Next(3, 9);
            float starScaleInterpolant = Main.rand.NextFloat();
            int starLifetime = (int)float.Lerp(11f, 30f, starScaleInterpolant);
            float starScale = float.Lerp(0.2f, 0.4f, starScaleInterpolant) * Projectile.scale;
            Color starColor = Color.Lerp(new(1f, 0.41f, 0.51f), new(1f, 0.85f, 0.37f), Main.rand.NextFloat());

            Vector2 starSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(16f, 16f);
            Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity;
            TwinkleParticle star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
            star.Spawn();
        }

        Time++;
    }

    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    public float StarFallTrailWidthFunction(float completionRatio) => Projectile.scale * Utils.Remap(completionRatio, 0f, 0.9f, 18f, 1f);

    public Color StarFallTrailColorFunction(float completionRatio) => new Color(75, 128, 250).HueShift(completionRatio * 0.15f) * (1f - completionRatio) * Projectile.Opacity;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (RenderOverPlayers)
            overPlayers.Add(index);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (!UseAfterimages)
            return;

        PrimitiveSettings settings = new PrimitiveSettings(StarFallTrailWidthFunction, StarFallTrailColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos.Take(8), settings, 60);
    }
}
