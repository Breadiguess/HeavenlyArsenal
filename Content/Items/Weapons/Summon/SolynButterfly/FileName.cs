using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Utilities;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;

public class SolynButterflyChargeUp : ModProjectile, IProjOwnedByBoss<BattleSolyn>
{
    private SlotId soundSlot;

    /// <summary>
    /// Solyn's index in the NPC array.
    /// </summary>
    public int ButterflyIndex => (int)Projectile.ai[0];

    /// <summary>
    /// The owner of this laserbeam.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// How long this effect has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// How long this effect should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(0.75f);

    /// <summary>
    /// The maximum gleam scale factor of this charge up visual.
    /// </summary>
    public static float MaxGleamScaleFactor => 0.45f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 150;
        Projectile.height = 150;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SolynButterflyBeam>()] > 0)
        {
            Projectile.Kill();

            return;
        }
       

        if (Time == 2f && Main.LocalPlayer.WithinRange(Projectile.Center, 3000f))
            soundSlot = SoundEngine.PlaySound(GennedAssets.Sounds.Mars.SolynStarBeamChargeUp).WithVolumeBoost(3f);
        Projectile solyn = Main.projectile[Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly.whoAmI];

        Projectile.Center = solyn.Center + Projectile.velocity;

        float lifetimeRatio = Time / Lifetime;
        Projectile.scale = InverseLerp(0f, 0.65f, lifetimeRatio).Squared() * InverseLerp(1f, 0.9f, lifetimeRatio);
        Projectile.rotation =(float) Math.Pow(Utils.SmoothStep(0f, 1f, lifetimeRatio), 0.3f) * MathHelper.TwoPi * 2f;

        AimTowardsMouse();

        Dust energy = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(200f, 200f), 264);
        energy.velocity = energy.position.SafeDirectionTo(Projectile.Center) * Main.rand.NextFloat(10f);
        energy.noGravity = true;
        energy.noLight = true;
        energy.scale *= Main.rand.NextFloat(1f, 1.6f);
        energy.color = Main.rand.NextBool() ? Color.Wheat : Color.HotPink;

        Time++;
    }

    /// <summary>
    /// Makes this beam slowly aim towards the user's mouse.
    /// </summary>
    public void AimTowardsMouse()
    {
        if (Main.myPlayer != Projectile.owner)
            return;

        Vector2 oldVelocity = Projectile.velocity;
        Projectile solyn = Main.projectile[Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly.whoAmI];
       
        ButterflyMinion butterfly = solyn.ModProjectile as ButterflyMinion;
        if (butterfly.targetNPC == null)
            return;
        float idealRotation = Projectile.AngleTo(butterfly.targetNPC.Center);
        Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(idealRotation,0.5f).ToRotationVector2();

        if (Projectile.velocity != oldVelocity)
        {
            Projectile.netUpdate = true;
            Projectile.netSpam = 0;
        }

        
    }

    public override void OnKill(int timeLeft)
    {
        if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound? sound))
            sound.Stop();
        Projectile solyn = Main.projectile[Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly.whoAmI];
        
        ButterflyMinion butterfly = solyn.ModProjectile as ButterflyMinion;
        if(butterfly.targetNPC != null)
        {
            Vector2 ButterflytoTarget = Projectile.Center.AngleTo(butterfly.targetNPC.Center).ToRotationVector2() * 100;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 
                ButterflytoTarget, ModContent.ProjectileType<SolynButterflyBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, 0f);

        }

    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Time < -1)
            return false;
        // Draw a superheated lens flare and bloom instance at the center of the beam.
        float shineIntensity = Projectile.scale * MaxGleamScaleFactor;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity * 100;
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
        Texture2D flare = MiscTexturesRegistry.ShineFlareTexture.Value;

        for (int i = 0; i < 3; i++)
            Main.spriteBatch.Draw(flare, drawPosition, null, SolynButterflyBeam.LensFlareColor with { A = 0 }, Projectile.rotation, flare.Size() * 0.5f, shineIntensity * 2f, 0, 0f);
        for (int i = 0; i < 2; i++)
            Main.spriteBatch.Draw(glow, drawPosition, null, SolynButterflyBeam.LensFlareColor with { A = 0 }, 0f, glow.Size() * 0.5f, shineIntensity * 2f, 0, 0f);

        return false;
    }
}