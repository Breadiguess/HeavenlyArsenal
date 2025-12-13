 using CalamityMod;
using CalamityMod.Particles;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
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
        public override bool? CanDamage() => false;


        private Vector2 BowTop;
        private Vector2 BowMiddle;
        private Vector2 BowBottom;
        public float t = 0;

        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public ref float Charge => ref Projectile.ai[1];
        public ref float ChargeInterp => ref Projectile.ai[2];

        public const int ChargeCap = 5;
        private DarkOneState CurrentState = DarkOneState.Idle;

        public ref Terraria.Player Owner => ref Main.player[Projectile.owner];

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
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            Vector2 Difference = Owner.MountedCenter - (Projectile.rotation.ToRotationVector2() * 5 + Projectile.Center);
            Owner.direction = Difference.X != 0 ? -Math.Sign(Difference.X) : 1;
            Owner.heldProj = this.Projectile.whoAmI;
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
                Charge--;

            ChargeInterp = float.Lerp(ChargeInterp, 0, 0.6f);
            Projectile.rotation = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
        }
        private void HandleCharge()
        {
            Player.CompositeArmStretchAmount stretch = ChargeInterp < 0.2f ? Player.CompositeArmStretchAmount.Full : ChargeInterp < 0.6f ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full;
            float rot = Owner.MountedCenter.AngleTo(BowMiddle);
            Owner.SetCompositeArmFront(true, stretch, rot - MathHelper.PiOver2);

            // Owner.SetDummyItemTime(2);
            float toMouse = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
            Projectile.rotation = Projectile.rotation.AngleLerp(toMouse, 0.2f);
            if (Charge > 0)
                for (int i = 0; i < Charge; i++)
                {
                    floatInterp[i] = float.Lerp(floatInterp[i], 1, 0.3f);
                    floatInterp[i] = MathF.Round(floatInterp[i], 5);
                }
                   



            StringCurve = new PiecewiseCurve()
                .Add(EasingCurves.Sine, EasingType.In, 0.5f, 0.5f)
                .Add(EasingCurves.Cubic, EasingType.Out, 1f, 1f);


            t = Utils.Clamp(t + 0.01f, 0, 1);
            ChargeInterp = StringCurve.Evaluate(t);




            if (Time % 34 == 0 && Charge < 5)
            {
                if (Owner.controlUseItem)
                Charge++;
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = Charge/5, MaxInstances = 0 }, Owner.Center);
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
                    SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { PitchVariance = 3 }, Owner.Center);

                }
                if (Time > 300)
                {
                    CurrentState = DarkOneState.Exhume;
                    SoundEngine.PlaySound(GennedAssets.Sounds.Common.ScreenShatter with { PitchVariance = 0.2f }, Owner.Center);
                    Time = 0;
                    t = 0;
                }
            }
        }
        private void HandleExhume()
        {

            Owner.SetDummyItemTime(2);
            float toMouse = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
            Projectile.rotation = Projectile.rotation.AngleLerp(toMouse, 0.1f);
            ChargeInterp = 0;

            if (Time == 1)
            {
                int ArrowID = ProjectileID.BoneArrow;
                Owner.PickAmmo(Owner.ActiveItem(), out ArrowID, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out int _);

                SoundEngine.PlaySound(SoundID.Item5, Owner.Center);
                Vector2 Velocity = Projectile.rotation.ToRotationVector2() * 30;
                Vector2 SpawnPos = BowMiddle + new Vector2(120, 0).RotatedBy(Projectile.rotation);
                int adjustedDamage = Owner.HeldItem.damage + 3000 * (int)Charge;
                Projectile a = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), SpawnPos, Velocity, ModContent.ProjectileType<CrystalArrow>(), adjustedDamage, 0);
                a.ai[2] = Charge;

                for (int x = 0; x < 3; x++)
                {

                    SpawnPos = BowMiddle + new Vector2(120 + x * 35, 0).RotatedBy(Projectile.rotation);
                    for (int i = 0; i < 40 - x * 10; i++)
                    {
                        Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                        voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

                        float random = Main.rand.NextFloat(-30, 30);
                        if (x > 1)
                        {
                            random = Main.rand.NextFloat(-10, 10);
                        }
                        Vector2 ParticleVelocity = new Vector2(random, 10).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
                        HeavySmokeParticle darkGas = new(SpawnPos + Main.rand.NextVector2Circular(4f, 4f),
                        ParticleVelocity, voidColor, 11, Projectile.scale * 1.24f * (1 - (1 + x) / 3f), 1, Main.rand.NextFloat(0.02f), true);
                        GeneralParticleHandler.SpawnParticle(darkGas);
                    }
                }

            }
            for (int i = 0; i < ChargeCap; i++)
            {
                floatInterp[i] = float.Lerp(floatInterp[i], 0, 0.3f);
                floatInterp[i] = MathF.Round(floatInterp[i], 5);
                //Main.NewText(floatInterp[i].ToString());
            }
            if (Time >= 30)
            {
                Time = 0;
                if (Owner.HasAmmo(Owner.HeldItem))
                    CurrentState = DarkOneState.Nocking;
                else
                    CurrentState = DarkOneState.Idle;
                Charge = 0;
            }

        }

        private float ArrowReloadInterp = 0;
        private Vector2 ArrowPos;
        public void NockArrow()
        {
            if (Time == 1)
                t = 0;
            if (ArrowPos == default)
                ArrowPos = Owner.Center;
            if (ArrowNockCurve == null)
            {
                ArrowNockCurve = new PiecewiseCurve().Add(EasingCurves.Sine, EasingType.In, 0.3f, 0.5f);
                ArrowNockCurve.Add(EasingCurves.Elastic, EasingType.Out, 1f, 1f);
            }
            t = Math.Clamp(t + 0.01f, 0, 1);
            t = MathF.Round(t, 4);

            float thing = ArrowNockCurve.Evaluate(t);
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

        float[] floatInterp;
        void DrawRiftPortal(int i)
        {
            if (i == 0 || i > ChargeCap - 2)
                return;
            Main.spriteBatch.PrepareForShaders();
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            ManagedShader PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");
            float scalar = (1 - i / (float)ChargeCap) * 0.8f + 0.4f * floatInterp[i];
            PortalShader.TrySetParameter("circleStretchInterpolant", Math.Clamp(scalar, 0, 1));
            //Main.NewText($"{i}: "+scalar);
            PortalShader.TrySetParameter("transformation", (Matrix.CreateScale(10f, 2f, 2f)));
            PortalShader.TrySetParameter("uColor", Color.MediumPurple with { A = 255 });
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
            float maxScale = 5f;
            Vector2 textureArea = Projectile.Size / pixel.Size() * maxScale;
            float scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f) * 0.012f);
            textureArea *= scaleMod;

            Vector2 DrawPos = BowMiddle + new Vector2(120 + i * 30, 0).RotatedBy(Projectile.rotation);
            DrawPos -= Main.screenPosition;
            Main.spriteBatch.Draw(pixel, DrawPos, null, Projectile.GetAlpha(Color.MediumPurple), Projectile.rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
            Main.spriteBatch.ResetToDefault();
        }

        public void DrawArrow(ref Color lightColor, SpriteEffects a)
        {
            Texture2D Arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow").Value;
            Texture2D GlowArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_Glow").Value;

            Texture2D IntenseArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_HeadGlow").Value;
            Vector2 DrawPos = BowMiddle - Main.screenPosition;
            DrawPos += new Vector2(Arrow.Width / 2 - 20, 0).RotatedBy(Projectile.rotation);

            float Rot = Projectile.rotation;

            Color Glow = Color.White * (Charge / ChargeCap);

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
            Vector2 ArrowHead = BowMiddle + new Vector2(150, 0).RotatedBy(Projectile.rotation);
            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            for (int i = 0; i < (Charge > 1 ? Charge : 1); i++)
            {
                DrawRiftPortal(i);
                if (CurrentState != DarkOneState.Exhume && CurrentState != DarkOneState.Nocking)
                {
                    float Wane = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f + 10 - i * 10) * 0.2f) + 0.8f;
                    Vector2 IntenseArrowOrigin = new Vector2(IntenseArrow.Width, IntenseArrow.Height / 2);

                    float thing = (i * 5 - 15) * MathF.Cos(Main.GlobalTimeWrappedHourly * 10.1f) * 0.2f;

                    Vector2 Adjusted = ArrowHead + new Vector2(-10, thing).RotatedBy(Projectile.rotation);
                    float adjustedRot = Adjusted.AngleTo(ArrowHead);

                    //Main.EntitySpriteDraw(debug, ArrowHead - Main.screenPosition, null, Color.Green, 0, debug.Size() / 2, 4, 0);
                    //Main.EntitySpriteDraw(debug, Adjusted - Main.screenPosition, null, Color.Red, 0, debug.Size() / 2, 4, 0);
                    Main.EntitySpriteDraw(IntenseArrow, Adjusted - Main.screenPosition, null, Glow, adjustedRot, IntenseArrowOrigin, Wane, a);
                    Texture2D impact = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/Impact").Value;

                    //Main.EntitySpriteDraw(impact, DrawPos + new Vector2(30, 0).RotatedBy(Projectile.rotation), null, Color.White * 0.4f, Projectile.rotation, impact.Size() / 2, 1, 0);

                }

            }

        }


        private void drawString(ref Color lightColor)
        {
            Color thing = Color.Lerp(Color.Purple, Color.CornflowerBlue, MathF.Sin(Main.GlobalTimeWrappedHourly));
            Color Bowstring = lightColor.MultiplyRGB(Color.Purple);
            Utils.DrawLine(Main.spriteBatch, BowTop, BowMiddle, Bowstring, thing, 2);
            Utils.DrawLine(Main.spriteBatch, BowMiddle, BowBottom, thing, lightColor.MultiplyRGB(Color.CornflowerBlue), 2);


        }
        public void DrawBow(ref Color lightColor)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/TheDarkOne").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;// + new Vector2(Projectile.width / 2, Projectile.height / 2);
            SpriteEffects effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Vector2 origin = new Vector2(texture.Width / 8, texture.Height / 2);
            float chargeOffset = Charge * Projectile.scale * 2f;


            drawString(ref lightColor);
            Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            DrawArrow(ref lightColor, effects);
            string debug = "| State: " + CurrentState.ToString() + " | Charge: " + Charge.ToString() + " | Scale: " + Projectile.scale + ", " + Projectile.rotation;
            debug += $"\n | T: {t} | Time: {Time} | ChargeInterp: {ChargeInterp}";

            //Utils.DrawBorderString(Main.spriteBatch, debug, drawPosition + Vector2.UnitY * -100, Color.AntiqueWhite);
            //Utils.DrawBorderString(Main.spriteBatch, "| Time: " + Time.ToString(), drawPosition + Vector2.UnitY * -80, Color.AntiqueWhite);
            return false;
        }
        #endregion
    }
    class noxusHeraldryController : ModPlayer
    {
        public Vector2 HeadPos;
        public override void PostUpdateMiscEffects()
        {
            Vector2 Desired = Player.MountedCenter + new Vector2(-10 * Player.direction, -30);

            if (HeadPos == default)
                HeadPos = Desired;
            HeadPos.X = float.Lerp(HeadPos.X, Desired.X, 0.55f);
            HeadPos.Y = float.Lerp(HeadPos.Y, Desired.Y, 0.9f);
            //HeadPos = Vector2.Lerp(HeadPos, Desired, 0.55f);
        }
    }
    public class NoxusHeraldry : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            bool thing = drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<NoxusWeapon>();

            return thing;
        }
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;




            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/NoxusEye").Value;
            Vector2 position = player.GetModPlayer<noxusHeraldryController>().HeadPos - Main.screenPosition;
            SpriteEffects effects = drawInfo.drawPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = texture.Size() / 2f;

            Vector2 Scale = new Vector2(0.8f, 1) * 0.11f * (MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f) * 0.1f + 1);

            Main.spriteBatch.UseBlendState(BlendState.Additive);

            float eyePulse = Main.GlobalTimeWrappedHourly * 1.3f % 1f;
            Main.EntitySpriteDraw(texture, position, null, Color.BlueViolet, 0, texture.Size() * 0.5f, Scale, 0, 0);
            Main.EntitySpriteDraw(texture, position, null, Color.MidnightBlue * (1f - eyePulse), 0, texture.Size() * 0.5f, Scale * (eyePulse * 0.39f + 1f), 0, 0);
            Main.spriteBatch.UseBlendState(default);
            Main.spriteBatch.ResetToDefault();
        }
    }
}
