using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.RenderTargets;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Magic;

public class avatar_FishingRodProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 40;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Magic;
    }

    public ref Player Player => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];
    public ref float BellRingCooldown => ref Projectile.ai[1];
    public ref float CurrentRiftState => ref Projectile.ai[2];

    public enum RiftState
    {
        Closed,
        OpenButAboveBellAndNeedsToWait,
        Open,
        Dunking,
        Disabled
    }

    public Vector2 riftPosition;
    public bool riftOpen;

    public Vector2 bellPosition;
    private Vector2 oldBellVelocity;

    public const int SwingTime = 12;
    public const int RetractTime = 40;

    public override void AI()
    {
        bool retracting = Projectile.timeLeft < RetractTime;

        if (Player.channel && CurrentRiftState != (int)RiftState.Disabled)
            Projectile.timeLeft = RetractTime + 15;
        else
            Player.itemTime = 0;

        SetPlayerItemAnimations();

        // This extra offset aligns the spool with the end of the rod
        Vector2 rodHeadPosition = Player.itemLocation + new Vector2(0, -62).RotatedBy(Player.itemRotation);
        if (Time < SwingTime * 1.1f)
            Projectile.Center = rodHeadPosition;

        if (Main.myPlayer == Projectile.owner)
        {
            // Push the spool position toward the cursor with some fancy springy movement
            Vector2 targetPosition = Main.MouseWorld;

            // Setting a distance limit
            float maxDistance = 500 * Utils.GetLerpValue(2, RetractTime, Projectile.timeLeft, true);
            if (targetPosition.Distance(Player.MountedCenter) > maxDistance)
                targetPosition = Player.MountedCenter + Player.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero) * maxDistance;

            if (CurrentRiftState == (int)RiftState.Dunking && targetPosition.Y > Player.MountedCenter.Y + RiftHeight)
                targetPosition.Y = Player.MountedCenter.Y + RiftHeight;

            Player.LimitPointToPlayerReachableArea(ref targetPosition);
            Vector2 spoolOffset = new Vector2(MathF.Sin(Projectile.localAI[0] * 0.05f) * 5f, MathF.Cos(Projectile.localAI[0] * 0.025f) * 5f);
            targetPosition += spoolOffset + Player.velocity;
            Vector2 targetVelocity = (targetPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.Distance(targetPosition) * 0.8f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, Utils.GetLerpValue(SwingTime / 3, SwingTime + 40, Time, true) * 0.3f);
            Projectile.velocity *= 0.8f;
            Projectile.netUpdate = true;

            if (Time > SwingTime)
                Projectile.direction = Player.MountedCenter.X > Projectile.Center.X ? -1 : 1;
        }

        UpdateFishingString();
        UpdateBellString();

        if (Time > SwingTime * 4 && CurrentRiftState != (int)RiftState.Disabled && !retracting)
            UpdateBellRinging();
        else
            Time++;

        if (CurrentRiftState != (int)RiftState.Closed && !retracting)
        {
            UpdateRift();
            riftApparitionInterpolant = Math.Min(riftApparitionInterpolant + 0.1f, 1f);
        }
        else
        {
            riftApparitionInterpolant = Math.Max(riftApparitionInterpolant - 0.05f, 0);

            if (CurrentRiftState == (int)RiftState.Closed)
                riftPosition = Player.MountedCenter + new Vector2(0, RiftHeight);
        }

        // Incrementing a second timer for visuals
        Projectile.localAI[0]++;

        // Spool visual stuff
        Projectile.rotation = Projectile.velocity.X * 0.005f;
    }

    public void SetPlayerItemAnimations()
    {
        Player.heldProj = Projectile.whoAmI;
        Player.SetDummyItemTime(3);
        Player.ChangeDir(Projectile.direction);

        // Creating a curve for the initial swing
        float swingTime = MathF.Pow(Time / SwingTime, 3f);
        float overShoot = 1f + MathF.Pow(Utils.GetLerpValue(SwingTime * 3, SwingTime * 1.2f, Time, true), 3) * 0.1f;
        float swingProgress = MathHelper.Lerp(swingTime, overShoot, Utils.GetLerpValue(SwingTime * 0.9f, SwingTime * 1.2f, Time, true));

        float handRotation = ((swingProgress - 1) * 3 - MathHelper.PiOver2) * Player.direction;
        float backHandRotation = handRotation + MathHelper.PiOver4 * Player.direction;
        Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, handRotation);
        Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backHandRotation);

        // RotatedRelativePoint automatically adds gfxOffY for us
        Player.itemLocation = Player.RotatedRelativePoint(Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, handRotation));

        float velocityRotation = Player.velocity.Y * 0.015f + Player.velocity.X * 0.015f * Player.direction;
        // Specifically not using AngleLerp because we want to go from negative to positive
        float targetRotation = MathHelper.Lerp(-MathHelper.PiOver2, 0.9f - velocityRotation, swingProgress) * Player.direction;
        // Using it here for some smoothing on the velocity rotation
        Player.itemRotation = Utils.AngleLerp(Player.itemRotation, targetRotation, 0.5f);
    }

    public Rope miscRope;
    public Rope bellRope;

    // Most of the string stuff is magic numbers to make it look good
    public void UpdateFishingString()
    {
        const int segmentCount = 24;
        float segmentLength = 500f / 60f;
        Vector2 rodHeadPosition = Player.itemLocation + new Vector2(8 * Projectile.direction, -60).RotatedBy(Player.itemRotation);

        // Initialize the segments
        if (miscRope == null)
        {
            miscRope = new Rope(rodHeadPosition, Projectile.Bottom, segmentCount, 1f, Vector2.UnitY);
            miscRope.Settle();
        }
        // Extend when thrown, retract when not in use
        miscRope.segmentLength = segmentLength * Utils.GetLerpValue(0, SwingTime, Time, true) * Utils.GetLerpValue(5, 40, Projectile.timeLeft, true);
        miscRope.damping = 0.3f;
        miscRope.segments[0].position = rodHeadPosition;
        miscRope.segments[^1].position = Projectile.Center + Projectile.velocity;
        miscRope.gravity = Projectile.velocity * 0.2f + Vector2.UnitY * 3f;

        miscRope.Update();
    }

    public void UpdateBellString()
    {
        const int segmentCount = 16;
        float segmentLength = 8f;

        // Initialize the segments
        if (bellRope == null)
        {
            bellRope = new Rope(Projectile.Center, Projectile.Bottom, segmentCount, 1f, Vector2.UnitY * 2f, 50);
            bellRope.segments[^1].pinned = false;
            bellRope.Settle();
        }
        // Extend when thrown, retract when not in use
        bellRope.segmentLength = segmentLength * Utils.GetLerpValue(0, SwingTime, Time, true) * Utils.GetLerpValue(5, 40, Projectile.timeLeft, true);
        bellRope.damping = 0.06f;
        bellRope.segments[0].position = Projectile.Center + Projectile.velocity;
        bellRope.Update();

        // Setting gravity after so changes that happen after can take effect on the next update
        bellRope.gravity = Vector2.UnitY * (Utils.GetLerpValue(RetractTime, RetractTime / 4, BellRingCooldown, true) + 0.05f);
    }

    public void UpdateBellRinging()
    {
        Vector2 bellPosition = bellRope.segments[^1].position;
        Vector2 currentBellVelocity = bellRope.segments[^1].velocity;

        if (oldBellVelocity.Distance(currentBellVelocity) > 16 && currentBellVelocity.Length() > 10) // Finding large accelerations, indicating sudden movements that would ring the bell
        {
            if (BellRingCooldown <= 0)
            {
                BellRingCooldown = 20;
                bellRope.segments[^1].velocity *= 0.1f;
                SoundEngine.PlaySound(SoundID.AbigailAttack with { Pitch = 1f, MaxInstances = 0 }, bellPosition);

                if (CurrentRiftState == (int)RiftState.Closed)
                {
                    CurrentRiftState = (int)RiftState.Open;
                    SoundEngine.PlaySound(NoxusBoss.Assets.GennedAssets.Sounds.Avatar.RiftOpen with { MaxInstances = 0 }, Player.MountedCenter);
                }
            }
        }

        if (BellRingCooldown > 0)
        {
            BellRingCooldown--;

            // Some visual here?
        }

        oldBellVelocity = currentBellVelocity;
    }

    public const int RiftHeight = 150;

    public void UpdateRift()
    {
        if (CurrentRiftState == (int)RiftState.Disabled)
            return;

        riftPosition = Vector2.Lerp(riftPosition, Player.MountedCenter + new Vector2(0, RiftHeight), 0.2f);
        float riftPositionY = riftPosition.Y - 10;

        Rope.RopeSegment bell = bellRope.segments[^1];

        // Essentially creating an infinitely deep box that begins below the player
        bool inBox = bell.position.Y > riftPositionY && Math.Abs(riftPosition.X - Projectile.Center.X) < 300;

        bool canUse = true;
        bool canDunk = (inBox && bell.velocity.Y > 0 && CurrentRiftState == (int)RiftState.Open) || CurrentRiftState == (int)RiftState.Dunking;

        // Preventing the bell from being dunked from below somehow
        if (bell.position.Y > riftPositionY && CurrentRiftState != (int)RiftState.Dunking && !canDunk)
            CurrentRiftState = (int)RiftState.OpenButAboveBellAndNeedsToWait;
        else if (CurrentRiftState == (int)RiftState.OpenButAboveBellAndNeedsToWait)
            CurrentRiftState = (int)RiftState.Open;

        if (canDunk)
        {
            bell.velocity *= 0.1f;
            bellRope.gravity = Vector2.UnitY * 4f;

            if (bell.position.Y < riftPositionY + 40)
                bell.position.Y = MathHelper.Lerp(bell.position.Y, riftPositionY + 40, 0.3f);

            BellRingCooldown = 50;
            Projectile.timeLeft = 50;

            if (CurrentRiftState != (int)RiftState.Dunking)
            {
                SoundEngine.PlaySound(SoundID.Shimmer2, bell.position);
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCry, bell.position);

                Vector2 bellSplashVelocity = new Vector2(bell.velocity.X, Math.Abs(bell.velocity.Y));
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDustPerfect(bell.position + Main.rand.NextVector2Circular(5, 2), DustID.RedTorch, bellSplashVelocity + Main.rand.NextVector2Circular(3, 1) - Vector2.UnitY * 3, 0, Color.Red, 0.5f + Main.rand.NextFloat());
                    if (!Main.rand.NextBool(5))
                        Dust.NewDustPerfect(bell.position + Main.rand.NextVector2Circular(5, 2), DustID.Smoke, bellSplashVelocity + Main.rand.NextVector2Circular(3, 1) - Vector2.UnitY, 0, Color.Black, 1f + Main.rand.NextFloat());
                }
            }

            Time++;
            CurrentRiftState = (int)RiftState.Dunking;

            if (Main.myPlayer == Projectile.owner)
            {
                if (true)
                    Fish();
                else
                {
                    const int waitTimeBetweenGrabs = 17;
                    const float maxDistance = 1000;

                    if (Time % waitTimeBetweenGrabs == 0)
                    {
                        canUse = Player.CheckMana(Player.HeldItem.mana, true);

                        NPC[] target = Main.npc.Where(t => t.active && t.CanBeChasedBy(Player) && t.Distance(Player.MountedCenter) < maxDistance).ToArray();

                        for (int i = 0; i < target.Length; i += Main.rand.Next(1, 4))
                        {
                            Vector2 randomDirection = Main.rand.NextVector2CircularEdge(1, 1);
                            Projectile shadowHand = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target[i].Center, randomDirection, ModContent.ProjectileType<avatar_FishingRodVoid>(), Projectile.damage, 0, Player.whoAmI, ai1: target[i].whoAmI);
                            shadowHand.scale *= Main.rand.NextFloat(0.5f, 1.5f);
                        }
                    }
                }
            }

            if (!Player.channel || !inBox)
                canUse = false;
        }

        if (!canUse)
        {
            CurrentRiftState = (int)RiftState.Disabled;
            bell.position.Y = riftPositionY;
            bellRope.gravity = -Vector2.UnitY * 20;
            BellRingCooldown = 240;
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftEject with { MaxInstances = 0 }, bell.position);
        }
    }

    public delegate void RollDropLevelsDelegate(Projectile proj, int fishingLevel, out bool common, out bool uncommon, out bool rare, out bool veryrare, out bool legendary, out bool crate);
	public static RollDropLevelsDelegate FishingCheck_RollDropLevels;

    public delegate void ProbeForQuestFishDelegate(Projectile proj, ref FishingAttempt fisher);
    public static ProbeForQuestFishDelegate FishingCheck_ProbeForQuestFish;

    public delegate void RollEnemySpawnsDelegate(Projectile proj, ref FishingAttempt fisher);
    public static RollEnemySpawnsDelegate FishingCheck_RollEnemySpawns;

    public delegate void RollItemDropDelegate(Projectile proj, ref FishingAttempt fisher);
    public static RollItemDropDelegate FishingCheck_RollItemDrop;

	private void Fish()
	{
		Rope.RopeSegment bell = bellRope.segments[^1];
        FishingAttempt fisher = new FishingAttempt();
		fisher.X = (int)(Projectile.Center.X / 16f);
		fisher.Y = (int)(Projectile.Center.Y / 16f);
        fisher.CanFishInLava = true;
        fisher.fishingLevel = 65535;

        FishingCheck_RollDropLevels?.Invoke(Projectile, fisher.fishingLevel, out fisher.common, out fisher.uncommon, out fisher.rare, out fisher.veryrare, out fisher.legendary, out fisher.crate);
        FishingCheck_ProbeForQuestFish?.Invoke(Projectile, ref fisher);
        FishingCheck_RollEnemySpawns?.Invoke(Projectile, ref fisher);
        FishingCheck_RollItemDrop?.Invoke(Projectile, ref fisher);

        AdvancedPopupRequest sonar = new AdvancedPopupRequest();
		Vector2 sonarPosition = bell.position; // Bobber position as default
		PlayerLoader.CatchFish(Player, fisher, ref fisher.rolledItemDrop, ref fisher.rolledEnemySpawn, ref sonar, ref sonarPosition);

        if (fisher.rolledItemDrop > 0)
			Player.QuickSpawnItem(Projectile.GetSource_FromThis(), fisher.rolledItemDrop);

        if (fisher.rolledEnemySpawn > 0 && fisher.rolledEnemySpawn != NPCID.TownSlimeRed)
		{
            NPC.NewNPCDirect(Projectile.GetSource_FromThis(), bell.position, fisher.rolledEnemySpawn);

        }
	}

	public override bool? CanCutTiles() => false;

    // Don't do damage directly
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

    public static Asset<Texture2D> spoolTexture;
    public static Asset<Texture2D> bellTexture;

    public override void Load()
    {
        spoolTexture = ModContent.Request<Texture2D>(Texture + "_Spool");
        bellTexture = ModContent.Request<Texture2D>(Texture + "_Bell");

        Main.ContentThatNeedsRenderTargets.Add(riftLakeTargets = new InstancedRequestableTarget());

		FishingCheck_RollDropLevels = typeof(Projectile).GetMethod("FishingCheck_RollDropLevels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .CreateDelegate<RollDropLevelsDelegate>(null);
		FishingCheck_ProbeForQuestFish = typeof(Projectile).GetMethod("FishingCheck_ProbeForQuestFish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .CreateDelegate<ProbeForQuestFishDelegate>(null);
		FishingCheck_RollEnemySpawns = typeof(Projectile).GetMethod("FishingCheck_RollEnemySpawns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .CreateDelegate<RollEnemySpawnsDelegate>(null);
		FishingCheck_RollItemDrop = typeof(Projectile).GetMethod("FishingCheck_RollItemDrop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .CreateDelegate<RollItemDropDelegate>(null);
	}

    public static InstancedRequestableTarget riftLakeTargets;

    private float riftApparitionInterpolant;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

        riftLakeTargets.Request(900, 1000, Projectile.whoAmI, () =>
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);

            ManagedShader dripShader = ShaderManager.GetShader("HeavenlyArsenal.AvatarRodVoidEffect");
            dripShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f);
            dripShader.TrySetParameter("noiseScale", new Vector2(1.25f, 0.5f));
            dripShader.TrySetParameter("noiseStrength", 1.5f * riftApparitionInterpolant);
            dripShader.TrySetParameter("outlineThickness", 0.04f);
            dripShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            dripShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 0, SamplerState.AnisotropicWrap);
            dripShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            dripShader.Apply();

            Main.spriteBatch.Draw(GennedAssets.Textures.Noise.WavyBlotchNoise, new Vector2(450, RiftHeight - 10), null, Color.Black, MathHelper.Pi, new Vector2(innerRiftTexture.Width / 2, innerRiftTexture.Height), new Vector2(1.33f * riftApparitionInterpolant, 0.75f * MathF.Pow(riftApparitionInterpolant, 2)), 0, 0);

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 5f);
            riftShader.TrySetParameter("vanishInterpolant", 1f - riftApparitionInterpolant);
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.15f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            // Rotating it seems to align the stretchy bit of the rift real nice
            Main.spriteBatch.Draw(innerRiftTexture, new Vector2(450, RiftHeight), null, Color.White, -MathHelper.PiOver2, innerRiftTexture.Size() * 0.5f, new Vector2(0.4f, 2.66f), 0, 0);
           
            Main.spriteBatch.End();
        });

        if (riftLakeTargets.TryGetTarget(Projectile.whoAmI, out RenderTarget2D riftTarget) && riftApparitionInterpolant > 0)
        {
            SpriteEffects flip = Main.LocalPlayer.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
            Main.EntitySpriteDraw(glow, riftPosition + new Vector2(0,  -30) - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 200 } * riftApparitionInterpolant, 0, glow.Size() * 0.5f, new Vector2(1.2f, 0.3f), 0, 0);
            Main.EntitySpriteDraw(riftTarget, riftPosition - Main.screenPosition, riftTarget.Frame(), Color.White, 0, new Vector2(riftTarget.Width / 2, RiftHeight), 1f, flip, 0);
            Main.EntitySpriteDraw(glow, riftPosition + new Vector2(0, 20) - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 150 } * riftApparitionInterpolant * 0.4f, 0, glow.Size() * 0.5f, new Vector2(1.5f, 0.7f) * riftApparitionInterpolant, 0, 0);
            Main.EntitySpriteDraw(glow, riftPosition + new Vector2(0, 14) - Main.screenPosition, glow.Frame(), Color.Black * riftApparitionInterpolant, 0, glow.Size() * 0.5f, new Vector2(0.6f, 0.1f) * riftApparitionInterpolant, 0, 0);
        }

        DrawStrings();
        DrawRod();
        DrawSpool();
        DrawBell();

        return false;
    }

    private void DrawRod()
    {
        Texture2D itemTexture = TextureAssets.Projectile[Type].Value;
        int dir = (int)(Player.direction * Player.gravDir);
        SpriteEffects itemEffect = dir > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        const int upRod = 5;
        Vector2 origin = new Vector2(itemTexture.Width * (0.5f - dir * 0.5f) + upRod * dir, itemTexture.Height - upRod);

        Main.EntitySpriteDraw(itemTexture, Player.itemLocation - Main.screenPosition, itemTexture.Frame(), Color.White, Player.itemRotation - MathHelper.PiOver4 * dir, origin, 1f, itemEffect, 0);
    }

    private void DrawSpool()
    {
        float spoolRotationOffset = MathF.Sin(Projectile.localAI[0] * 0.15f) * 0.05f;
        Vector2 spoolOrigin = new Vector2(spoolTexture.Width() / 2, spoolTexture.Height() - 10);
        Main.EntitySpriteDraw(spoolTexture.Value, Projectile.Center - Main.screenPosition, spoolTexture.Frame(), Color.White, Projectile.rotation + spoolRotationOffset, spoolOrigin, 1f, 0, 0);
    }

    private void DrawBell()
    {
        // We need that last segment, and others to draw extra stuff
        if (bellRope == null || CurrentRiftState == (int)RiftState.Dunking || CurrentRiftState == (int)RiftState.Disabled)
            return;

        Vector2 bellPosition = bellRope.segments[^1].position; // First index from the end
        float bellRotation = bellRope.segments[^2].position.AngleTo(bellPosition) - MathHelper.PiOver2;
        Vector2 origin = new Vector2(bellTexture.Width() / 2, 8);
        float fadeScale = Utils.GetLerpValue(SwingTime / 3, SwingTime / 2, Time, true);

        Main.EntitySpriteDraw(bellTexture.Value, bellPosition - Main.screenPosition, bellTexture.Frame(), Color.White, bellRotation, origin, Projectile.scale * fadeScale, 0, 0);
    }

    private void DrawStrings()
    {
        if (miscRope == null || bellRope == null)
            return;

        DrawString();
        DrawBellString();
    }

    private void DrawString()
    {
        Texture2D stringTexture = TextureAssets.FishingLine.Value;

        Vector2[] positions = miscRope.GetPoints();
        Vector2 stringOrigin = new Vector2(stringTexture.Width / 2, 0);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector2 stretch = new Vector2(1f, positions[i].Distance(positions[i + 1]) / stringTexture.Height);
            float stringRotation = positions[i].AngleTo(positions[i + 1]) - MathHelper.PiOver2;
            Color stringBaseColor = Color.Lerp(new Color(255, 0, 35, 120), Color.Black, 1f - (float)i / (positions.Length - 1));
            Main.EntitySpriteDraw(stringTexture, positions[i] - Main.screenPosition, stringTexture.Frame(), stringBaseColor, stringRotation, stringOrigin, stretch, 0, 0);
        }
    }

    private void DrawBellString()
    {
        Texture2D stringTexture = TextureAssets.FishingLine.Value;

        Vector2[] positions = bellRope.GetPoints();
        Vector2 stringOrigin = new Vector2(stringTexture.Width / 2, 0);
        Color stringBaseColor = new Color(255, 0, 35, 120);
        float riftPosY = riftPosition.Y - 10;

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector2 stretch = new Vector2(1f, positions[i].Distance(positions[i + 1]) / stringTexture.Height);
            float stringRotation = positions[i].AngleTo(positions[i + 1]) - MathHelper.PiOver2;

            Color stringColor = stringBaseColor;
            if (CurrentRiftState == (int)RiftState.Dunking)
                stringColor = Color.Lerp(stringBaseColor, Color.Transparent, Utils.GetLerpValue(0, RiftHeight / 3, positions[i].Y - riftPosY + RiftHeight / 3, true));

            Main.EntitySpriteDraw(stringTexture, positions[i] - Main.screenPosition, stringTexture.Frame(), stringColor, stringRotation, stringOrigin, stretch, 0, 0);
        }
    }
}
