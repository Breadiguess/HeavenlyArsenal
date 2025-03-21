using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Magic;

public class avatar_FishingRodVoid : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 7;
        Projectile.timeLeft = 100;
        Projectile.penetrate = 4;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
        Projectile.extraUpdates = 1;
    }

    public ref float Time => ref Projectile.ai[0];
    public int NPCToAttachTo => (int)Projectile.ai[1];

    public ref float VisualScale => ref Projectile.localAI[0];

    private VoidLakeShadowHandData[] shadowHands;

    public struct VoidLakeShadowHandData
    {
        public VoidLakeShadowHandData(float targetLength)
        {
            Length = targetLength * Main.rand.NextFloat(0.8f, 1.2f);
            ArmStyle = Main.rand.Next(3);
            HandStyle = Main.rand.Next(3);
        }

        private int ArmStyle;
        private int HandStyle;

        public float Length;
        public float Rotation;

        public void Draw(Vector2 center, float extensionAmount, float scale, float rotation, int direction = 1)
        {
            const float armLength = 215;

            Texture2D armTexture = AssetDirectory.Textures.VoidLakeShadowArm.Value;
            Texture2D handTexture = AssetDirectory.Textures.VoidLakeShadowHand.Value;
            Rectangle armFrame = armTexture.Frame(3, 1, ArmStyle);
            Rectangle handFrame = handTexture.Frame(3, 1, ArmStyle);
            SpriteEffects flip = direction > 0 ? 0 : SpriteEffects.FlipHorizontally;

            float scalingUp = 0.5f * scale * MathF.Sqrt(Utils.GetLerpValue(0, 0.5f, extensionAmount, true));
            Vector2 armScale = new Vector2(scalingUp, extensionAmount * (Length / armLength));

            Vector2 handPosition = center + rotation.ToRotationVector2() * extensionAmount * Length;
            Main.EntitySpriteDraw(handTexture, handPosition, handFrame, Color.Red, rotation + MathHelper.PiOver2, new Vector2(handFrame.Width / 2, handFrame.Height - 60), scalingUp * 1.15f, flip, 0);

            Main.EntitySpriteDraw(armTexture, center, armFrame, Color.Red, rotation + MathHelper.PiOver2, new Vector2(armFrame.Width / 2, armFrame.Height - 16), armScale, flip, 0);
        }
    }

    public const float DistanceFromTarget = 120;

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (NPCToAttachTo > -1 && NPCToAttachTo < Main.npc.Length)
        {
            NPC attached = Main.npc[NPCToAttachTo];
            if (!attached.CanBeChasedBy(this))
            {
                Projectile.ai[1] = -1;
                return;
            }
            Projectile.rotation = Projectile.AngleTo(attached.Center);
            Projectile.Center = attached.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, attached.velocity.SafeNormalize(Vector2.Zero) + Vector2.UnitY * 3f, 0.003f);

            VisualScale = Math.Clamp(MathF.Sqrt(MathHelper.Max(attached.width, attached.height) / 50f), 0.25f, 2f) * Projectile.scale;
        }
        else if (Projectile.timeLeft > 40)
            Projectile.timeLeft = 40;

        if (Time % 3 == 0 && Projectile.timeLeft > 40)
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1, 1), 3f, 5f, 7, 1000));

        if (Time == 0)
        {
            Projectile.direction = Main.rand.NextBool().ToDirectionInt();
            shadowHands = new VoidLakeShadowHandData[3];
            shadowHands[0] = new VoidLakeShadowHandData(0.7f * VisualScale * DistanceFromTarget);
            shadowHands[1] = new VoidLakeShadowHandData(1.1f * VisualScale * DistanceFromTarget);
            shadowHands[2] = new VoidLakeShadowHandData(0.7f * VisualScale * DistanceFromTarget);
        }

        Time++;
    }

    public override bool? CanCutTiles() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Time > 20 && Projectile.timeLeft > 40)
            return base.Colliding(projHitbox, targetHitbox);

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

    public override bool PreDraw(ref Color lightColor)
    {        
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

        float vanishTime = Utils.GetLerpValue(0, 20, Time, true) * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true);
        Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * DistanceFromTarget * VisualScale;

        Main.EntitySpriteDraw(glow, Projectile.Center + offset - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 200 } * vanishTime * 0.2f, Projectile.rotation, glow.Size() * 0.5f, new Vector2(0.5f, 0.7f) * VisualScale, 0, 0);
        Main.EntitySpriteDraw(glow, Projectile.Center + offset - Main.screenPosition, glow.Frame(), Color.Red with { A = 200 } * vanishTime, Projectile.rotation, glow.Size() * 0.5f, new Vector2(0.15f, 0.2f) * VisualScale * vanishTime, 0, 0);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        Color edgeColor = new Color(1f, 0.06f, 0.06f);
        float timeOffset = Projectile.identity * 2.5552343f;

        ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.42f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 5f);
        riftShader.TrySetParameter("vanishInterpolant", 1f - vanishTime);
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.15f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, Projectile.Center + offset - Main.screenPosition, null, Color.White, Projectile.rotation + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.25f, 0.6f) * VisualScale, 0, 0);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        float handRot = Projectile.rotation;
        DrawShadowHand(Projectile.Center + offset, -0.2f + Projectile.rotation, 0.7f, 0, -1);
        DrawShadowHand(Projectile.Center + offset, 0.2f + Projectile.rotation, 0.8f, 2, 1);
        DrawShadowHand(Projectile.Center + offset, Projectile.rotation, 1f, 1, Projectile.direction);

        Main.EntitySpriteDraw(glow, Projectile.Center + offset - Main.screenPosition, glow.Frame(), Color.Black, Projectile.rotation, glow.Size() * 0.5f, new Vector2(0.07f, 0.2f) * VisualScale * Projectile.scale * vanishTime, 0, 0);

        return false;
    }

    private void DrawShadowHand(Vector2 center, float rotation, float scale, int index, int direction)
    {
        float extensionTime = MathF.Sqrt(Utils.GetLerpValue(15, 25, Time - Projectile.localNPCHitCooldown * index, true) * Utils.GetLerpValue(35, 60, Projectile.timeLeft + Projectile.localNPCHitCooldown * index, true));
        Vector2 offsetForHands = new Vector2(0, 20 * (index - 1)).RotatedBy(Projectile.rotation);
        float wobble = MathF.Sin(((Main.GlobalTimeWrappedHourly * 1.67f + index * 0.2f) % 1f) * MathHelper.TwoPi) * 0.03f;
        shadowHands[index].Draw(center + offsetForHands - Main.screenPosition, extensionTime, scale, rotation + wobble, direction);
    }
}