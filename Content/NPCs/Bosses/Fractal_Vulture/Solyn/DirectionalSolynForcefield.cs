using CalamityMod.NPCs.TownNPCs;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;

public class DirectionalSolynForcefield3 : ModProjectile, IProjOwnedByBoss<BattleSolynBird>, IDrawsWithShader
{
    private static Projectile? myself;
    public static Projectile? Myself
    {
        get
        {
            if (Main.gameMenu)
                return myself = null;

            if (myself is null)
                return null;

            if (!myself.active)
                return null;

            if (myself.type != ModContent.ProjectileType<DirectionalSolynForcefield3>())
                return null;

            return myself;
        }
        private set => myself = value;
    }
    public NPC Solyn;
    /// <summary>
    /// The owner of this forcefield.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// The disappearance timer for this forcefield.
    /// </summary>
    public int DisappearTime
    {
        get;
        set;
    }

    /// <summary>
    /// The 0-1 interpolant for this forcefield's disappearance.
    /// </summary>
    public float DisappearanceInterpolant => LumUtils.InverseLerp(0f, 24f, DisappearTime);

    /// <summary>
    /// How long this forcefield has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// The moving average of this forcefield's spin angular velocity.
    /// </summary>
    public ref float SpinSpeedMovingAverage => ref Projectile.ai[1];

    /// <summary>
    /// The 0-1 completion value for impacts to this forcefield.
    /// </summary>
    public ref float ImpactAnimationCompletion => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
    }

    public override void SetDefaults()
    {
        int width = 500;// MarsBody.GetAIInt("BrutalBarrage_SolynForcefieldWidth");
        Projectile.width = width;
        Projectile.height = (int)(width * 0.27f);
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 999999;
        Projectile.penetrate = -1;
        Projectile.Opacity = 0f;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(DisappearTime);
        writer.Write(Projectile.rotation);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        DisappearTime = reader.ReadInt32();
        Projectile.rotation = reader.ReadSingle();
    }

    public override void AI()
    {
        myself = this.Projectile ;
        int ownerIndex = -1;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is BattleSolynBird solyn && solyn.MultiplayerIndex == Owner.whoAmI)
            {
                ownerIndex = npc.whoAmI;
                break;
            }
        }

        int width = (int)(300 * 1);// LumUtils.InverseLerpBump(0, voidVulture.VomitCone_ShootStart, voidVulture.VomitCone_ShootStop, voidVulture.VomitCone_ShootEnd, voidVulture.Myself.As<voidVulture>().Time));
        //  Main.NewText(width);
        Projectile.width = width;
        Projectile.height = (int)(width * 0.27f);
        if (ownerIndex == -1)
        {
            Projectile.Kill();
            return;
        }

        MoveTowardsMouse();
        HandleImpactAnimationTimings();

        if(voidVulture.Myself != null)
        {
            if(voidVulture.Myself.As<voidVulture>().currentState != voidVulture.Behavior.VomitCone && DisappearTime == 0)
            {
                BeginDisappearing();
            }
        }
        if (DisappearTime >= 1)
            DisappearTime++;
        Projectile.Opacity = Projectile.Opacity.StepTowards(LumUtils.InverseLerp(0f, 16f, Time) * (1f - DisappearanceInterpolant), 0.05f);



        // Decide the current scale.
        float impactCompletionBump = LumUtils.Convert01To010(ImpactAnimationCompletion);
        float impactAFfectedScale = float.Lerp(1f, 0.6f, impactCompletionBump) + MathF.Pow(impactCompletionBump, 4f) * 0.56f;
        Projectile.scale = impactAFfectedScale + DisappearanceInterpolant * 0.75f;

        Time++;
        // Main.NewText(DisappearanceInterpolant);
        
        if (DisappearanceInterpolant >= 1f)
            Projectile.Kill();
    }

    /// <summary>
    /// Moves the forcefield towards the mouse.
    /// </summary>
    public void MoveTowardsMouse()
    {
        if (Main.myPlayer != Projectile.owner)
            return;

        float oldDirection = Projectile.velocity.ToRotation();
        float idealDirection = Solyn.AngleTo(voidVulture.Myself.Center);

        float reorientInterpolant = (1f - DisappearanceInterpolant) * (1f - GameSceneSlowdownSystem.SlowdownInterpolant);
        float newDirection = oldDirection.AngleLerp(idealDirection, reorientInterpolant * 0.25f).AngleTowards(idealDirection, reorientInterpolant * 0.008f);

        Projectile.velocity = newDirection.ToRotationVector2();
        Projectile.rotation = newDirection + MathHelper.PiOver2;
        if (oldDirection != newDirection)
        {
            Projectile.netUpdate = true;
            Projectile.netSpam = 0;
        }

        float angleDifference = WrapAngle(newDirection - oldDirection);
        SpinSpeedMovingAverage = float.Lerp(SpinSpeedMovingAverage, Math.Abs(angleDifference), 0.03f);

        float offset = float.Lerp(90f, 67f, LumUtils.Convert01To010(ImpactAnimationCompletion));
        Projectile.Center =  Solyn.Center + Projectile.velocity * offset;
    }

    /// <summary>
    /// Handles the incrementing and resetting of the <see cref="ImpactAnimationCompletion"/> value.
    /// </summary>
    public void HandleImpactAnimationTimings()
    {
        if (ImpactAnimationCompletion > 0f)
        {
            ImpactAnimationCompletion += 0.09f;
            if (ImpactAnimationCompletion >= 1f)
            {
                ImpactAnimationCompletion = 0f;
                Projectile.netUpdate = true;
            }
        }
    }

    /// <summary>
    /// Initiates the disappearance of this forcefield.
    /// </summary>
    public void BeginDisappearing()
    {
        DisappearTime = 1;
        Projectile.netUpdate = true;
    }

    public void DrawWithShader(SpriteBatch spriteBatch)
    {
        ManagedShader forcefieldShader = ShaderManager.GetShader("NoxusBoss.DirectionalSolynForcefieldShader");
        forcefieldShader.TrySetParameter("colorA", new Color(255, 113, 194).ToVector4());
        forcefieldShader.TrySetParameter("colorB", Color.Wheat.ToVector4());
        forcefieldShader.TrySetParameter("glowIntensity", float.Lerp(0.75f, 3f, LumUtils.Convert01To010(ImpactAnimationCompletion)));
        forcefieldShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 1, SamplerState.LinearClamp);
        forcefieldShader.Apply();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(Color.White);
        for (int i = 8; i >= 1; i--)
        {
            float afterimageRotation = Projectile.oldRot[i].AngleLerp(Projectile.rotation, 0.5f);
            float angularOffset = WrapAngle(afterimageRotation - Projectile.rotation);
            float afterimageOpacity = MathF.Exp(i * -0.4f);
            Vector2 afterimageDrawPosition = drawPosition.RotatedBy(angularOffset, Owner.Center - Main.screenPosition);
            Main.spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.WhitePixel, afterimageDrawPosition, null, color * afterimageOpacity, afterimageRotation, GennedAssets.Textures.GreyscaleTextures.WhitePixel.Size() * 0.5f, Projectile.scale * Projectile.Size * 5f, 0, 0f);
        }

        Main.spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.WhitePixel, drawPosition, null, color, Projectile.rotation, GennedAssets.Textures.GreyscaleTextures.WhitePixel.Size() * 0.5f, Projectile.scale * Projectile.Size * 5f, 0, 0f);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        Projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size()) && Projectile.Opacity >= 0.56f;

    public override bool ShouldUpdatePosition() => false;
    //sigh
    private static float WrapAngle(float angle)
    {
        while (angle > MathHelper.Pi)
            angle -= MathHelper.TwoPi;
        while (angle < -MathHelper.Pi)
            angle += MathHelper.TwoPi;
        return angle;
    }
}
