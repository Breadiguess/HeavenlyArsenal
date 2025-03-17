using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Projectiles.Holdout.Nadir2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons;

public class avatar_FishingRodProjectile : ModProjectile
{

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
        Projectile.timeLeft = 10;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.hide = true;
    }

    public ref Player Player => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];
    public ref float BellRingCooldown => ref Projectile.ai[1];
    public ref float RiftTime => ref Projectile.ai[2];

    public Vector2 bellPosition;
    private Vector2 oldBellVelocity;

    public bool riftOpen;

    public const int SwingTime = 12;
    public const int RetractTime = 40;

    public override void AI()
    {
        if (Player.channel)
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

            // If you don't want a distance limit, remove these three lines 
            // I think its a little nicer when the thing doesnt fly offscreen
            const float maxDistance = 500;
            if (targetPosition.Distance(Player.MountedCenter) > maxDistance)
                targetPosition = Player.MountedCenter + Player.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero) * maxDistance;

            Player.LimitPointToPlayerReachableArea(ref targetPosition);
            Vector2 spoolOffset = new Vector2(MathF.Sin(Projectile.localAI[0] * 0.05f) * 5f, MathF.Cos(Projectile.localAI[0] * 0.025f) * 5f);
            targetPosition += spoolOffset + Player.velocity;
            Vector2 targetVelocity = (targetPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.Distance(targetPosition) * 0.6f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, Utils.GetLerpValue(SwingTime / 3, SwingTime + 40, Time, true) * 0.1f);
            Projectile.velocity *= 0.8f;
            
            Projectile.netUpdate = true;
        }

        UpdateFishingString();

        if (Time > SwingTime * 4)
            UpdateBellRinging();

        UpdateBellString();

        if (riftOpen)
            UpdateRift();

        Time++;

        // Incrementing a second timer for visuals
        Projectile.localAI[0]++;

        // Spool visual stuff
        Projectile.rotation = Projectile.velocity.X * 0.005f;
    }

    public void SetPlayerItemAnimations()
    {
        Player.heldProj = Projectile.whoAmI;
        Player.SetDummyItemTime(3);
        Projectile.direction = Player.direction;

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
        // Specifically not using AngleLerp because we want to go from negative to positive
        Player.itemRotation = MathHelper.Lerp(-MathHelper.PiOver2, 1f, swingProgress) * Player.direction;
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
        miscRope.segmentLength = segmentLength * Utils.GetLerpValue(0, SwingTime, Time, true) * Utils.GetLerpValue(2, 40, Projectile.timeLeft, true);
        miscRope.damping = 0.3f;
        miscRope.segments[0].position = rodHeadPosition;
        miscRope.segments[^1].position = Projectile.Center + Projectile.velocity;
        miscRope.gravity = Projectile.velocity * 0.2f + Vector2.UnitY * 3f;

        miscRope.Update();
    }

    public void UpdateBellString()
    {
        const int segmentCount = 16;
        float segmentLength = 10f;

        // Initialize the segments
        if (bellRope == null)
        {
            bellRope = new Rope(Projectile.Center, Projectile.Bottom, segmentCount, 1f, Vector2.UnitY * 2f, 50);
            bellRope.segments[^1].pinned = false;
            bellRope.Settle();
        }
        // Extend when thrown, retract when not in use
        bellRope.segmentLength = segmentLength * Utils.GetLerpValue(0, SwingTime, Time, true) * Utils.GetLerpValue(2, 40, Projectile.timeLeft, true);
        bellRope.damping = 0.06f;
        bellRope.segments[0].position = Projectile.Center + Projectile.velocity;

        bellRope.gravity = Vector2.UnitY * (Utils.GetLerpValue(40, 10, BellRingCooldown, true) + 0.05f);

        bellRope.Update();
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
                SoundEngine.PlaySound(NoxusBoss.Assets.GennedAssets.Sounds.NamelessDeity.ClockStrike with { Pitch = 1f, MaxInstances = 0 }, bellPosition);

                if (!riftOpen)
                {
                    riftOpen = true;
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

    private int indexOfLastRiftedSegment;

    public void UpdateRift()
    {
        Vector2 riftPosition = Player.MountedCenter + new Vector2(170 * Player.direction, 120);

        if (bellRope.segments[^1].position.Distance(riftPosition) < 50 && bellRope.segments[^1].position.Y < riftPosition.Y + 10)
        {
            bellRope.segments[^1].velocity *= 0.1f;
            bellRope.segments[^1].position = Vector2.Lerp(bellRope.segments[^1].position, riftPosition, 0.8f);
            BellRingCooldown = 50;
            Projectile.timeLeft = 50;

            RiftTime++;

            if (Main.myPlayer == Projectile.owner)
            {
                if (RiftTime % 5 == 0)
                {
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.active && npc.CanBeChasedBy(Player) && npc.Distance(Player.MountedCenter) < 800)
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<AntishadowAssassinSlash>(), Projectile.damage, 0, Player.whoAmI);
                    }
                }
            }
        }
        else if (RiftTime > 0)
            RiftTime = 241;

        // If the rift has been in use for some time, close it and disable the bell for some time
        if (RiftTime > 240)
        {
            BellRingCooldown = 240;
            RiftTime = 0;
            riftOpen = false;
            SoundEngine.PlaySound(NoxusBoss.Assets.GennedAssets.Sounds.Avatar.RiftEject with { MaxInstances = 0 }, bellRope.segments[^1].position);
        }
    }

    public static Asset<Texture2D> spoolTexture;
    public static Asset<Texture2D> bellTexture;

    public override void Load()
    {
        spoolTexture = ModContent.Request<Texture2D>(Texture + "_Spool");
        bellTexture = ModContent.Request<Texture2D>(Texture + "_Bell");
    }

    public override bool PreDraw(ref Color lightColor)
    {
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
        float fadeScale = Utils.GetLerpValue(SwingTime / 3, SwingTime / 2, Time, true) * Utils.GetLerpValue(2, RetractTime / 2, Projectile.timeLeft, true);
        Vector2 spoolOrigin = new Vector2(spoolTexture.Width() / 2, spoolTexture.Height() - 10);
        Main.EntitySpriteDraw(spoolTexture.Value, Projectile.Center - Main.screenPosition, spoolTexture.Frame(), Color.White, Projectile.rotation + spoolRotationOffset, spoolOrigin, fadeScale, 0, 0);
    }

    private void DrawBell()
    {
        // We need that last segment, and others to draw extra stuff
        if (bellRope == null)
            return;

        Vector2 bellPosition = bellRope.segments[^1].position; // First index from the end
        float bellRotation = bellRope.segments[^2].position.AngleTo(bellPosition) - MathHelper.PiOver2;
        Vector2 origin = new Vector2(bellTexture.Width() / 2, 8);
        float fadeScale = Utils.GetLerpValue(SwingTime / 3, SwingTime / 2, Time, true) * Utils.GetLerpValue(2, RetractTime / 2, Projectile.timeLeft, true);

        Main.EntitySpriteDraw(bellTexture.Value, bellPosition - Main.screenPosition, bellTexture.Frame(), Color.White, bellRotation, origin, Projectile.scale * fadeScale, 0, 0);
    }

    private void DrawStrings()
    {
        if (miscRope == null || bellRope == null)
            return;

        DrawString(miscRope.GetPoints());
        DrawString(bellRope.GetPoints());
    }

    private void DrawString(Vector2[] positions)
    {
        Texture2D stringTexture = TextureAssets.FishingLine.Value;
        Color stringBaseColor = new Color(5, 182, 255, 120);
        Vector2 stringOrigin = new Vector2(stringTexture.Width / 2, 0);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector2 stretch = new Vector2(1f, positions[i].Distance(positions[i + 1]) / stringTexture.Height);
            float stringRotation = positions[i].AngleTo(positions[i + 1]) - MathHelper.PiOver2;
            Main.EntitySpriteDraw(stringTexture, positions[i] - Main.screenPosition, stringTexture.Frame(), stringBaseColor, stringRotation, stringOrigin, stretch, 0, 0);
        }
    }
}
