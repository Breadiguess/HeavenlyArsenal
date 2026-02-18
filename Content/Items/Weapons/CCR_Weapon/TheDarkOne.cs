using CalamityMod;
using CalamityMod.Particles;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Configuration;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

public enum DarkOneState
{
    Idle,

    Charge,

    Exhume,

    Nocking
}

public class TheDarkOne : ModProjectile
{
    #region setup

    public PiecewiseCurve ArrowNockCurve;

    public PiecewiseCurve StringCurve;
    public override bool IsLoadingEnabled(Mod mod)
    {
        // Check config setting
        var enabledInConfig = ModContent.GetInstance<ServerSideConfiguration>().EnableSpecialItems;
        var isOtherModLoaded = ModLoader.HasMod("CalRemix");

        return enabledInConfig || isOtherModLoaded;
    }

    public override bool? CanDamage()
    {
        return false;
    }

    private Vector2 BowTop;

    private Vector2 BowMiddle;

    private Vector2 BowBottom;

    public float t;

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public ref float Charge => ref Projectile.ai[1];

    public ref float ChargeInterp => ref Projectile.ai[2];

    public const int ChargeCap = 5;

    private DarkOneState CurrentState = DarkOneState.Idle;

    public ref Player Owner => ref Main.player[Projectile.owner];

    private Vector2 Offset //set to the owner's center
    {
        get => Owner.Center + new Vector2(0, -Owner.gfxOffY);
        set => Owner.Center = value - new Vector2(0, -Owner.gfxOffY);
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.extraUpdates = 2;
        Projectile.width = 60;
        Projectile.height = 60;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source)
    {
        BowTop = Projectile.Center + new Vector2(0 + Projectile.velocity.X, 300).RotatedBy(Projectile.rotation);
        BowBottom = Projectile.Center + new Vector2(0, -30).RotatedBy(Projectile.rotation);
        BowMiddle = (BowTop + BowBottom) / 2 - new Vector2(10, -10 * Projectile.direction).RotatedBy(Projectile.rotation) * 0;
        floatInterp = new float[ChargeCap];
    }

    #endregion

    #region AI

    public override void AI()
    {
        if (Owner.HeldItem.type != ModContent.ItemType<NoxusWeapon>() || Owner.CCed || Owner.dead)
        {
            Projectile.Kill();

            return;
        }

        Projectile.timeLeft++;

        Projectile.Center = Owner.MountedCenter;

        StateMachine();

        if (CurrentState != DarkOneState.Charge)
        {
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        }

        var Difference = Owner.MountedCenter - (Projectile.rotation.ToRotationVector2() * 5 + Projectile.Center);
        Owner.direction = Difference.X != 0 ? -Math.Sign(Difference.X) : 1;
        Owner.heldProj = Projectile.whoAmI;
        BowTop = Projectile.Center + new Vector2(15, -60).RotatedBy(Projectile.rotation);
        BowBottom = Projectile.Center + new Vector2(15, 60).RotatedBy(Projectile.rotation);
        BowMiddle = (BowTop + BowBottom) / 2 - new Vector2(40, 0).RotatedBy(Projectile.rotation) * ChargeInterp;

        Time++;
    }

    private void StateMachine()
    {
        switch (CurrentState)
        {
            case DarkOneState.Idle:
                HandleIdle();

                break;
            case DarkOneState.Charge:
                HandleCharge();

                break;
            case DarkOneState.Exhume:
                HandleExhume();

                break;
            case DarkOneState.Nocking:
                NockArrow();

                break;
        }
    }

    private void HandlePullout()
    {
        CurrentState = DarkOneState.Idle;
    }

    private void HandleIdle()
    {
        if (Owner.controlUseItem && Owner.altFunctionUse != 2 && Owner.HasAmmo(Owner.HeldItem))
        {
            CurrentState = DarkOneState.Charge;
            Time = 0;
        }

        if (Charge > 0)
        {
            Charge--;
        }

        ChargeInterp = float.Lerp(ChargeInterp, 0, 0.6f);
        Projectile.rotation = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
    }

    private void HandleCharge()
    {
        var stretch = ChargeInterp < 0.2f ? Player.CompositeArmStretchAmount.Full : ChargeInterp < 0.6f ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full;
        var rot = Owner.MountedCenter.AngleTo(BowMiddle);
        Owner.SetCompositeArmFront(true, stretch, rot - MathHelper.PiOver2);

        // Owner.SetDummyItemTime(2);
        var toMouse = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
        Projectile.rotation = Projectile.rotation.AngleLerp(toMouse, 0.2f);

        if (Charge > 0)
        {
            for (var i = 0; i < Charge; i++)
            {
                floatInterp[i] = float.Lerp(floatInterp[i], 1, 0.3f);
                floatInterp[i] = MathF.Round(floatInterp[i], 5);
            }
        }

        StringCurve = new PiecewiseCurve()
            .Add(EasingCurves.Sine, EasingType.In, 0.5f, 0.5f)
            .Add(EasingCurves.Cubic, EasingType.Out, 1f, 1f);

        t = Utils.Clamp(t + 0.01f, 0, 1);
        ChargeInterp = StringCurve.Evaluate(t);

        if (Time % 34 == 0 && Charge < 5)
        {
            if (Owner.controlUseItem)
            {
                Charge++;
            }

            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Common.TwinkleMuffled with
                {
                    Pitch = Charge / 5,
                    MaxInstances = 0
                },
                Owner.Center
            );
        }

        ChargeInterp = float.Lerp(ChargeInterp, 1, 0.02f);

        if (!Owner.controlUseItem && Charge <= 1)
        {
            CurrentState = DarkOneState.Idle;
            t = 0;
            Time = 0;
        }

        if (!Owner.controlUseItem && Charge >= 2)
        {
            CurrentState = DarkOneState.Exhume;
            t = 0;
            Time = 0;
        }

        if (Charge == ChargeCap && Time > 186)
        {
            Projectile.Center += Main.rand.NextVector2Unit();

            if (Time % 40 == 0)
            {
                SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Common.Glitch with
                    {
                        PitchVariance = 3
                    },
                    Owner.Center
                );
            }

            if (Time > 300)
            {
                CurrentState = DarkOneState.Exhume;

                SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Common.ScreenShatter with
                    {
                        PitchVariance = 0.2f
                    },
                    Owner.Center
                );

                Time = 0;
                t = 0;
            }
        }
    }

    private void HandleExhume()
    {
        Owner.SetDummyItemTime(2);
        var toMouse = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
        Projectile.rotation = Projectile.rotation.AngleLerp(toMouse, 0.1f);
        ChargeInterp = 0;

        if (Time == 1)
        {
            int ArrowID = ProjectileID.BoneArrow;
            Owner.PickAmmo(Owner.HeldItem, out ArrowID, out var SpeedNoUse, out var bulletDamage, out var kBackNoUse, out var _);

            SoundEngine.PlaySound(SoundID.Item5, Owner.Center);
            var Velocity = Projectile.rotation.ToRotationVector2() * 30;
            var SpawnPos = BowMiddle + new Vector2(120, 0).RotatedBy(Projectile.rotation);
            var adjustedDamage = Owner.HeldItem.damage + 3000 * (int)Charge;
            var a = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), SpawnPos, Velocity, ModContent.ProjectileType<CrystalArrow>(), adjustedDamage, 0);
            a.ai[2] = Charge;

            for (var x = 0; x < 3; x++)
            {
                SpawnPos = BowMiddle + new Vector2(120 + x * 35, 0).RotatedBy(Projectile.rotation);

                for (var i = 0; i < 40 - x * 10; i++)
                {
                    var voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

                    var random = Main.rand.NextFloat(-30, 30);

                    if (x > 1)
                    {
                        random = Main.rand.NextFloat(-10, 10);
                    }

                    var ParticleVelocity = new Vector2(random, 10).RotatedBy(Projectile.rotation - MathHelper.PiOver2);

                    HeavySmokeParticle darkGas = new
                    (
                        SpawnPos + Main.rand.NextVector2Circular(4f, 4f),
                        ParticleVelocity,
                        voidColor,
                        11,
                        Projectile.scale * 1.24f * (1 - (1 + x) / 3f),
                        1,
                        Main.rand.NextFloat(0.02f),
                        true
                    );

                    GeneralParticleHandler.SpawnParticle(darkGas);
                }
            }
        }

        for (var i = 0; i < ChargeCap; i++)
        {
            floatInterp[i] = float.Lerp(floatInterp[i], 0, 0.3f);
            floatInterp[i] = MathF.Round(floatInterp[i], 5);
            //Main.NewText(floatInterp[i].ToString());
        }

        if (Time >= 30)
        {
            Time = 0;

            if (Owner.HasAmmo(Owner.HeldItem))
            {
                CurrentState = DarkOneState.Nocking;
            }
            else
            {
                CurrentState = DarkOneState.Idle;
            }

            Charge = 0;
        }
    }

    private float ArrowReloadInterp;

    private Vector2 ArrowPos;

    public void NockArrow()
    {
        if (Time == 1)
        {
            t = 0;
        }

        if (ArrowPos == default)
        {
            ArrowPos = Owner.Center;
        }

        if (ArrowNockCurve == null)
        {
            ArrowNockCurve = new PiecewiseCurve().Add(EasingCurves.Sine, EasingType.In, 0.3f, 0.5f);
            ArrowNockCurve.Add(EasingCurves.Elastic, EasingType.Out, 1f, 1f);
        }

        t = Math.Clamp(t + 0.01f, 0, 1);
        t = MathF.Round(t, 4);

        var thing = ArrowNockCurve.Evaluate(t);
        ArrowPos = Projectile.Center + new Vector2(thing * 70, 0).RotatedBy(Projectile.rotation);
        Projectile.rotation = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld + new Vector2(0, 60));
        ArrowReloadInterp = float.Lerp(ArrowReloadInterp, 1, 0.02f);

        if (t == 1 || Time >= 100)
        {
            CurrentState = DarkOneState.Idle;
            Time = 0;
            t = 0;
            ArrowReloadInterp = 0;
        }
    }

    #endregion

    #region drawCode

    private float[] floatInterp;

    private void DrawRiftPortal(int i)
    {
        if (i == 0 || i > ChargeCap - 2)
        {
            return;
        }

        Main.spriteBatch.PrepareForShaders();
        //Main.spriteBatch.End();
        //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");
        var scalar = (1 - i / (float)ChargeCap) * 0.8f + 0.4f * floatInterp[i];
        PortalShader.TrySetParameter("circleStretchInterpolant", Math.Clamp(scalar, 0, 1));
        //Main.NewText($"{i}: "+scalar);
        PortalShader.TrySetParameter("transformation", Matrix.CreateScale(10f, 2f, 2f));

        PortalShader.TrySetParameter
        (
            "uColor",
            Color.MediumPurple with
            {
                A = 255
            }
        );

        //PortalShader.TrySetParameter("uSecondaryColor", Color.White);
        PortalShader.TrySetParameter("edgeFadeInSharpness", 20.3f);
        PortalShader.TrySetParameter("aheadCircleMoveBackFactor", 0.67f);
        PortalShader.TrySetParameter("aheadCircleZoomFsctor", 0.09f);
        //PortalShader.TrySetParameter("uProgress", portalInterp * Main.GlobalTimeWrappedHourly);
        PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);

        PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 0);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 1);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2);
        PortalShader.SetTexture(GennedAssets.Textures.Extra.Void, 3);
        //PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.Spikes, 4, SamplerState.PointWrap);

        PortalShader.Apply();
        Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        var maxScale = 5f;
        var textureArea = Projectile.Size / pixel.Size() * maxScale;
        var scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f) * 0.012f);
        textureArea *= scaleMod;

        var DrawPos = BowMiddle + new Vector2(120 + i * 30, 0).RotatedBy(Projectile.rotation);
        DrawPos -= Main.screenPosition;
        Main.spriteBatch.Draw(pixel, DrawPos, null, Projectile.GetAlpha(Color.MediumPurple), Projectile.rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }

    public void DrawArrow(ref Color lightColor, SpriteEffects a)
    {
        var Arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow").Value;
        var GlowArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_Glow").Value;

        var IntenseArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_HeadGlow").Value;
        var DrawPos = BowMiddle - Main.screenPosition;
        DrawPos += new Vector2(Arrow.Width / 2 - 20, 0).RotatedBy(Projectile.rotation);

        var Rot = Projectile.rotation;

        var Glow = Color.White * (Charge / ChargeCap);

        if (CurrentState == DarkOneState.Nocking)
        {
            DrawPos = ArrowPos + new Vector2() - Main.screenPosition;
            Glow = Color.White * ArrowReloadInterp;

            if (Owner.HasAmmo(Owner.HeldItem))
            {
                Main.EntitySpriteDraw(Arrow, DrawPos, null, lightColor * ArrowReloadInterp, Rot, Arrow.Size() * 0.5f, 1, a);
                Main.EntitySpriteDraw(GlowArrow, DrawPos, null, Glow, Rot, Arrow.Size() * 0.5f, 1f, a);
            }
        }

        if (CurrentState != DarkOneState.Exhume && CurrentState != DarkOneState.Nocking)
        {
            if (Owner.HasAmmo(Owner.HeldItem))
            {
                Main.EntitySpriteDraw(Arrow, DrawPos, null, lightColor, Rot, Arrow.Size() * 0.5f, 1, a);
                Main.EntitySpriteDraw(GlowArrow, DrawPos, null, Color.AntiqueWhite, Rot, Arrow.Size() * 0.5f, 1f, a);
            }
        }

        var ArrowHead = BowMiddle + new Vector2(150, 0).RotatedBy(Projectile.rotation);
        Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        for (var i = 0; i < (Charge > 1 ? Charge : 1); i++)
        {
            DrawRiftPortal(i);

            if (CurrentState != DarkOneState.Exhume && CurrentState != DarkOneState.Nocking)
            {
                var Wane = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f + 10 - i * 10) * 0.2f) + 0.8f;
                var IntenseArrowOrigin = new Vector2(IntenseArrow.Width, IntenseArrow.Height / 2);

                var thing = (i * 5 - 15) * MathF.Cos(Main.GlobalTimeWrappedHourly * 10.1f) * 0.2f;

                var Adjusted = ArrowHead + new Vector2(-10, thing).RotatedBy(Projectile.rotation);
                var adjustedRot = Adjusted.AngleTo(ArrowHead);

                //Main.EntitySpriteDraw(debug, ArrowHead - Main.screenPosition, null, Color.Green, 0, debug.Size() / 2, 4, 0);
                //Main.EntitySpriteDraw(debug, Adjusted - Main.screenPosition, null, Color.Red, 0, debug.Size() / 2, 4, 0);
                Main.EntitySpriteDraw(IntenseArrow, Adjusted - Main.screenPosition, null, Glow, adjustedRot, IntenseArrowOrigin, Wane, a);
                var impact = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/Impact").Value;

                //Main.EntitySpriteDraw(impact, DrawPos + new Vector2(30, 0).RotatedBy(Projectile.rotation), null, Color.White * 0.4f, Projectile.rotation, impact.Size() / 2, 1, 0);
            }
        }
    }

    private void drawString(ref Color lightColor)
    {
        var thing = Color.Lerp(Color.Purple, Color.CornflowerBlue, MathF.Sin(Main.GlobalTimeWrappedHourly));
        var Bowstring = lightColor.MultiplyRGB(Color.Purple);
        Utils.DrawLine(Main.spriteBatch, BowTop, BowMiddle, Bowstring, thing, 2);
        Utils.DrawLine(Main.spriteBatch, BowMiddle, BowBottom, thing, lightColor.MultiplyRGB(Color.CornflowerBlue), 2);
    }

    public void DrawBow(ref Color lightColor) { }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/TheDarkOne").Value;
        var drawPosition = Projectile.Center - Main.screenPosition; // + new Vector2(Projectile.width / 2, Projectile.height / 2);
        var effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        var origin = new Vector2(texture.Width / 8, texture.Height / 2);
        var chargeOffset = Charge * Projectile.scale * 2f;

        drawString(ref lightColor);
        Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
        DrawArrow(ref lightColor, effects);
        var debug = "| State: " + CurrentState + " | Charge: " + Charge + " | Scale: " + Projectile.scale + ", " + Projectile.rotation;
        debug += $"\n | T: {t} | Time: {Time} | ChargeInterp: {ChargeInterp}";

        //Utils.DrawBorderString(Main.spriteBatch, debug, drawPosition + Vector2.UnitY * -100, Color.AntiqueWhite);
        //Utils.DrawBorderString(Main.spriteBatch, "| Time: " + Time.ToString(), drawPosition + Vector2.UnitY * -80, Color.AntiqueWhite);
        return false;
    }

    #endregion
}

internal class noxusHeraldryController : ModPlayer
{

    public override bool IsLoadingEnabled(Mod mod)
    {
        // Check config setting
        var enabledInConfig = ModContent.GetInstance<ServerSideConfiguration>().EnableSpecialItems;
        var isOtherModLoaded = ModLoader.HasMod("CalRemix");

        return enabledInConfig || isOtherModLoaded;
    }

    public Vector2 HeadPos;

    public override void PostUpdateMiscEffects()
    {
        var Desired = Player.MountedCenter + new Vector2(-10 * Player.direction, -30);

        if (HeadPos == default)
        {
            HeadPos = Desired;
        }

        HeadPos.X = float.Lerp(HeadPos.X, Desired.X, 0.55f);
        HeadPos.Y = float.Lerp(HeadPos.Y, Desired.Y, 0.9f);
        //HeadPos = Vector2.Lerp(HeadPos, Desired, 0.55f);
    }
}

public class NoxusHeraldry : PlayerDrawLayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        // Check config setting
        var enabledInConfig = ModContent.GetInstance<ServerSideConfiguration>().EnableSpecialItems;
        var isOtherModLoaded = ModLoader.HasMod("CalRemix");

        return enabledInConfig || isOtherModLoaded;
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        var thing = drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<NoxusWeapon>();

        return thing;
    }

    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var player = drawInfo.drawPlayer;

        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/NoxusEye").Value;
        var position = player.GetModPlayer<noxusHeraldryController>().HeadPos - Main.screenPosition;
        var effects = drawInfo.drawPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var origin = texture.Size() / 2f;

        var Scale = new Vector2(0.8f, 1) * 0.11f * (MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f) * 0.1f + 1);

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        var eyePulse = Main.GlobalTimeWrappedHourly * 1.3f % 1f;
        Main.EntitySpriteDraw(texture, position, null, Color.BlueViolet, 0, texture.Size() * 0.5f, Scale, 0);
        Main.EntitySpriteDraw(texture, position, null, Color.MidnightBlue * (1f - eyePulse), 0, texture.Size() * 0.5f, Scale * (eyePulse * 0.39f + 1f), 0);
        Main.spriteBatch.UseBlendState(default);
        Main.spriteBatch.ResetToDefault();
    }
}