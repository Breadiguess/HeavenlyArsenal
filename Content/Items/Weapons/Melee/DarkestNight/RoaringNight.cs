using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Particle = CalamityMod.Particles.Particle;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class RoaringNight : ModProjectile
    {
        public Item CreatorItem;
        public PiecewiseCurve SwingCurve;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float Swinginterp => ref Projectile.localAI[0];
        public ref float t => ref Projectile.localAI[2];

        public float GlowSizeInterpolant;

        public float swingDirection = 0f;
        public float _slashScale;

        public float JoustProgress;

        public bool canHit;
        public bool CreatingGlass;
        public bool SwingInProgress;
        public bool Joust;

        public int SlashDistance = 200;
        public int SwingStage
        {
            get => (int)Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(canHit);
            writer.Write(CreatingGlass);
            writer.Write(SwingInProgress);
            writer.Write(t);

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            canHit = reader.ReadBoolean();
            CreatingGlass = reader.ReadBoolean();
            SwingInProgress = reader.ReadBoolean();
            t = reader.ReadSingle();
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (CreatingGlass)
            {
                behindProjectiles.Add(index);

            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.coldDamage = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.Size = new Vector2(50, 50);
            const int slashLength = 24;
            _slashPositions = new Vector2[slashLength];
            _slashRotations = new float[slashLength];

            SwingInProgress = false;
            Projectile.scale = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

        }
        public override void OnSpawn(IEntitySource source)
        {
            ResetTrail();
            ResetSlash();
            UpdateSlash();
            UpdateTrail();
            ResetValues();
        }

        public override void AI()
        {


            CheckDespawnConditions();

            Projectile.timeLeft++;
            
            Projectile.Center = Owner.GetFrontHandPositionImproved(Owner.compositeFrontArm);
            Owner.heldProj = Projectile.whoAmI;
            Projectile.velocity = Vector2.Zero;

            Projectile.extraUpdates = 7;
            SlashDistance = 245;

            Time++;
            if (Joust && !CreatingGlass)
            {

                LancerLot();
                return;
            }

            if (Owner.controlUseItem && Owner.controlUp && Owner.altFunctionUse != 2)
            {
                Time = 0;
                Joust = true;
            }

            CreateBlackGlass();
            if (CreatingGlass && !Joust)
                return;

            if (Owner.altFunctionUse == 2 && !CreatingGlass)
            {
                Time = 0;
                CreatingGlass = true;
            }
            if (Owner.controlUseItem && Owner.altFunctionUse != 2)
            {
                SwingInProgress = true;
            }

            if (SwingInProgress)
            {
                BasicSwing();
            }


        }
        #region Attack stuff
        public void BasicSwing()
        {
            Owner.SetDummyItemTime(2);
            UpdateSlash();
            UpdateTrail();
            canHit = true;
            if (SwingStage == 0)
            {
                
                if (t == 0)
                {
                    swingDirection = Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(-230) * Owner.direction;
                    //Main.NewText($"{Projectile.rotation}");
                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.Swing with { PitchVariance = 0.2f, Pitch = 0.4f, }, Projectile.Center);


                }
                if (t <= 0.2)
                {

                    Projectile.scale = float.Lerp(Projectile.scale, 1, t);
                    Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
                }

                if (t >= 0.4f && t <= 0.6f)
                {

                    ScreenShakeSystem.StartShake(0.1f);
                }
                if (SwingCurve == null)
                    SwingCurve = new PiecewiseCurve()
                        .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);

                t = Math.Clamp(t + 0.0085f, 0, 1);
                Swinginterp = SwingCurve.Evaluate(t);
                Projectile.rotation = swingDirection + MathHelper.TwoPi * Swinginterp * Owner.direction;
                //Projectile.velocity = Projectile.rotation.ToRotationVector2()*2;
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

                if (t >= 0.74)
                {
                    Projectile.scale *= 0.88f;
                }
                if (t == 1)
                {
                    SwingStage++;
                    ResetValues();
                }
            }



            else if (SwingStage == 1)
            {
                if (t == 0)
                {
                    float basedir = Owner.direction == 1 ? MathHelper.ToRadians(-125) : MathHelper.ToRadians(230);
                    swingDirection = basedir * Owner.direction + Owner.Center.AngleTo(Main.MouseWorld);
                    //Main.NewText($"{Projectile.rotation}");
                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.Swing2 with { PitchVariance = 0.2f }, Projectile.Center);
                    Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);

                }
                if (t <= 0.2f)

                    Projectile.scale = float.Lerp(Projectile.scale, 1, t);
                if (SwingCurve == null)
                    SwingCurve = new PiecewiseCurve()
                        .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);
                t = Math.Clamp(t + 0.005f, 0, 1);

                Swinginterp = SwingCurve.Evaluate(t);
                Projectile.rotation = swingDirection + MathHelper.TwoPi * Swinginterp * -Owner.direction;
                //Projectile.velocity = Projectile.rotation.ToRotationVector2() * 2;
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);


                if (t >= 0.74)
                {
                    Projectile.scale *= 0.88f;
                }
                if (t == 1)
                {
                    SwingStage++;
                    ResetValues();
                }
            }
        }
        public void LancerLot()
        {

            float Desired = Projectile.Center.AngleTo(Main.MouseWorld);
            if (Time == 0)
            {
                Desired = swingDirection;
                Projectile.rotation = Desired;
                Projectile.velocity = Projectile.rotation.ToRotationVector2();
            }
            Projectile.scale = 1;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Desired - MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Desired - MathHelper.PiOver2);


            Projectile.rotation = Projectile.rotation.AngleLerp(Desired, 0.05f);

            if (JoustProgress == 0 && Time == 40 * Projectile.MaxUpdates)
            {
                canHit = true;
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
                Joust = true;
            }
            else if (JoustProgress < 1f && Time > 40 * Projectile.MaxUpdates)
            {

                foreach (Projectile proj in Main.projectile)
                {

                    if (proj.owner != Owner.whoAmI)
                        continue;

                    if (proj.type == ModContent.ProjectileType<BlackGlassMass>())
                    {
                        if (Projectile.Colliding(Projectile.getRect(), proj.getRect()))
                        {
                            Owner.GetModPlayer<GlassPlayer>().SinkSwordIntoGlassMass(proj);
                            JoustProgress = 1;
                        }
                    }
                }

                Owner.Calamity().LungingDown = true;
                Owner.mount?.Dismount(Owner);
                Owner.RemoveAllGrapplingHooks();
                Owner.fallStart = (int)(Owner.position.Y / 16f);

                //yes, i stole this from the exoblade.
                //it'll be replaced later, but i don't have a lot of experience with this kinda thing yet.
                Vector2 newVelocity = Projectile.velocity * Exoblade.LungeSpeed * (0.24f + 0.76f * 0.5f);
                Owner.velocity += newVelocity;
                float rotationStrenght = MathHelper.PiOver4 * 0.05f * (float)Math.Pow(JoustProgress, 3);
                float currentRotation = Projectile.rotation;
                float idealRotation = Owner.MountedCenter.DirectionTo(Owner.Calamity().mouseWorld).ToRotation();

                Projectile.velocity = currentRotation.AngleTowards(idealRotation, rotationStrenght).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);
            }
            if (JoustProgress >= 1)
            {

                //todo: if there is a black glass mass near the player in the direction they're aiming, lock onto it. 
                Owner.velocity *= 0.2f;
                ResetValues();
            }
            if (Time > 40 * Projectile.MaxUpdates)
                JoustProgress = float.Lerp(JoustProgress, 1.1f, 0.01f);
            Main.NewText(JoustProgress);
        }
        public void CreateBlackGlass()
        {


            if (!CreatingGlass)
            {
                if (GlowSizeInterpolant > 0)
                    GlowSizeInterpolant = float.Lerp(GlowSizeInterpolant, 0, 0.05f);

                return;
            }

            int placeholder = 45 * Projectile.MaxUpdates;

            if (Time <= placeholder * 2)
                GlowSizeInterpolant = float.Lerp(GlowSizeInterpolant, 1, 0.01f);
            else
             if (GlowSizeInterpolant > 0)
                GlowSizeInterpolant = float.Lerp(GlowSizeInterpolant, 0, 0.01f);


            int GlassCount = 3;


            Owner.SetDummyItemTime(2);
            Vector2 Direction = Owner.Center.AngleTo(Main.MouseWorld).ToRotationVector2();
            float ToMouse = Owner.Center.AngleTo(Main.MouseWorld);


            float desiredHand = swingDirection.AngleLerp(ToMouse - MathHelper.PiOver2, 0.1f);
            swingDirection = desiredHand;
            Owner.direction = Math.Sign(Direction.X);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, swingDirection);

            //Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0);

            ConeVFXInterp = GlowSizeInterpolant;
            Vector2 AdjustedSpawn = Owner.GetBackHandPositionImproved(Owner.compositeBackArm) + new Vector2(0, -2).RotatedBy(swingDirection);


            if (Time < placeholder && Time % 2 == 0)
            {
                //Time = placeholder - 30;
                float thing = Main.rand.NextFloat(100, 230);

                ScreenShakeSystem.StartShake(0.1f);

                /*
               AdjustedSpawn -= Owner.Center;
               AdjustedSpawn *= 2f;
               AdjustedSpawn += new Vector2(thing, 0).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-25, 26)) + swingDirection + MathHelper.PiOver2);
               AdjustedSpawn += Owner.Center;

               float Rot = desiredHand + MathHelper.PiOver2;

               Projectile.velocity = Rot.ToRotationVector2();
               */
                float RotationOffset = swingDirection + MathHelper.ToRadians(Main.rand.NextFloat(-25, 26)) + MathHelper.PiOver2;

            }
            if (Time >= placeholder && Time % 30 == 0)
                for (int i = 0; i < GlassCount; i++)
                {
                    ScreenShakeSystem.StartShake(1.25f);
                    Vector2 Velocity = Direction.RotatedByRandom(MathHelper.ToRadians(20)) * 10;
                    int Damage = Projectile.damage / 7;
                    Projectile s = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), AdjustedSpawn, Velocity, ModContent.ProjectileType<BlackGlass>(), Damage, 0);
                    s.rotation = s.velocity.ToRotation();
                    if (i == 0)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherAppear with { MaxInstances = 2, PitchVariance = 0.2f, Volume = 0.25f });
                }
            if (Time >= placeholder * 2 + 20)
            {
                Owner.altFunctionUse = 0;
                CreatingGlass = false;
                ConeVFXInterp = 0;
            }
        }

        #endregion

        public void CheckDespawnConditions()
        {
            if (Owner.HeldItem.type != ModContent.ItemType<Rapture>() || !Owner.active || Owner.dead || Owner.GetModPlayer<GlassPlayer>().Empowered || Owner.HasBuff(BuffID.Cursed))
            {
                Projectile.Kill();
                return;
            }
        }
        public void ResetValues()
        {
            Projectile.ResetLocalNPCHitImmunity();
            SwingInProgress = false;
            Projectile.scale = 0;
            SwingCurve = null;
            Swinginterp = 0;
            t = 0;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, 0);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, 0);
            Projectile.rotation = Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(-90);
            ResetSlash();
            ResetTrail();
            Joust = false;
            JoustProgress = 0;
            swingDirection = 0;
            canHit = false;
            if (SwingStage > 1)
                SwingStage = 0;
            ConeVFXInterp = 0;
        }
        #region Slash stuff
        public void ResetTrail(bool rotation = false)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Projectile.oldPos[i] = Projectile.Center;
                if (rotation)
                    Projectile.oldRot[i] = Projectile.rotation;
            }
        }
        public void UpdateTrail()
        {
            Vector2 playerPosOffset = Owner.position - Owner.oldPosition;

            if (Projectile.numUpdates == 0)
            {
                playerPosOffset = Vector2.Zero;

                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                    Projectile.oldRot[i] = Projectile.rotation.AngleLerp(Projectile.oldRot[i - 1], 0.1f);


                }

                //if (!holdTrailUpdate)
                {
                    Projectile.oldPos[0] = Projectile.Center + Projectile.velocity;
                    Projectile.oldRot[0] = Projectile.rotation;
                }

            }
        }
        public void ResetSlash()
        {
            _slashScale = 1f;
            for (int i = _slashPositions.Length - 1; i > 0; i--)
            {
                _slashPositions[i] = new Vector2(SlashDistance * Projectile.scale, 0).RotatedBy(Projectile.rotation);
                _slashRotations[i] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
            }
        }
        public void UpdateSlash()
        {
            for (int i = _slashPositions.Length - 1; i > 0; i--)
            {
                _slashRotations[i] = _slashRotations[i - 1];
                _slashPositions[i] = _slashPositions[i - 1];
            }

            _slashRotations[0] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
            _slashPositions[0] = new Vector2(SlashDistance * Projectile.scale * _slashScale, 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(1 * Projectile.direction));
        }
        #endregion
        #region Onhit/ modify damage stuff
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(50, 100);
            hitbox.Location += new Vector2(180 * Projectile.scale, 0).RotatedBy(Projectile.rotation).ToPoint();
        }
        public override bool? CanDamage()
        {
            return canHit;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (canHit)
            {
                float dist = 230f;


                Vector2 offset = new Vector2(dist * Projectile.scale * 1.65f, 0).RotatedBy(Projectile.rotation);
                float _ = 0;
                return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center + offset, 120f, ref _);
            }

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 AdjustedSpawn = target.Center;
            Particle d = new ImpactParticle(AdjustedSpawn, MathHelper.ToRadians(3), 0, 6, Color.White);

            GeneralParticleHandler.SpawnParticle(d);

            Vector2 thing = (Projectile.rotation + MathHelper.ToRadians(90)).ToRotationVector2() * 10;
            Dust a;
            for (int i = 0; i < 30; i++)
            {

                a = Dust.NewDustDirect(target.Center, 2, 2, DustID.AncientLight, thing.X, thing.Y);
                a.noGravity = true;
                float Variation = Main.rand.NextFloat(0.75f, 4f);
                a.velocity *= SwingStage == 1 ? Variation : -Variation;
            }


        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage = StatModifier.Default + 0.2f;

            modifiers.ArmorPenetration = AddableFloat.Zero + 5;
            if (target.SuperArmor)
            {
                modifiers = modifiers with { SuperArmor = false };
            }

        }
        #endregion
        #region Drawcode
        public override bool PreDraw(ref Color lightColor)
        {

            if (t >= 0.1f && t <= 0.720f)
                DrawSlash();
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            DrawShell(DrawPos, ref lightColor);
            DrawCone(DrawPos);


            return false;
        }
        public void DrawShell(Vector2 DrawPos, ref Color lightColor)
        {
            Texture2D thing = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 Origin = new Vector2(thing.Width / 2, thing.Height + 60);
            float Rot = Projectile.rotation + MathHelper.PiOver2;
            float Scale = Projectile.scale * 0.2f;

            SpriteEffects Flip = SwingStage == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;// | SpriteEffects.FlipVertically;



            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 Afterimages = DrawPos;
                Main.EntitySpriteDraw(thing, Afterimages, null, lightColor * 0.2f, Projectile.oldRot[i] + MathHelper.PiOver2, Origin, Scale, Flip);
                //Utils.DrawBorderString(Main.spriteBatch, i.ToString(), Afterimages, Color.AntiqueWhite);
            }

            Vector2 AdjustedDrawPos = DrawPos + new Vector2(0, 10 * (1 - JoustProgress)).RotatedBy(Rot);
            Main.EntitySpriteDraw(thing, AdjustedDrawPos, null, lightColor.MultiplyRGB(Color.White), Rot, Origin, Scale, Flip);

        }

        public float ConeVFXInterp;
        private Vector2[] ConeVFXPos;
        private float[] ConeVFXdistance;
        private float[] ConeVFXRotation;
        private float[] ConeVFXOpacity;
        private Color[] ConeVFXColor;
        public void DrawCone(Vector2 DrawPos)
        {
            if (GlowSizeInterpolant! <= 0)
                return;
            Vector2 AdjustedSpawn = Owner.GetBackHandPositionImproved(Owner.compositeBackArm) + new Vector2(0, -2).RotatedBy(swingDirection);

            Texture2D Cone = AssetDirectory.Textures.GlowCone.Value;
            float offset = 0;
            Vector2 Adjusted = Owner.GetBackHandPositionImproved(Owner.compositeBackArm) - Main.screenPosition + new Vector2(0, -2).RotatedBy(swingDirection);

            Vector2 Origin = new Vector2(0, Cone.Height / 2);

            float Rot = swingDirection + MathHelper.PiOver2;
            //DrawExtraEffects();

            Color ConeColor = RainbowColorGenerator.TrailColorFunction((float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly + offset))) * 0.2f;
            Vector2 Squish = new Vector2(2, 1);
            Squish.Y *= (float)(1 + MathF.Sin(Main.GlobalTimeWrappedHourly * 20) * 0.1f) * GlowSizeInterpolant;
            UpdateConeVFX(AdjustedSpawn);
            DrawConeVFX(Rot);
            for (int i = 0; i < 3; i++)
            {
                offset = (float)(i * 1f) / 3;
                ConeColor = RainbowColorGenerator.TrailColorFunction((float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly + offset))) * 0.2f;

                Main.EntitySpriteDraw(Cone, Adjusted, null, ConeColor, Rot, Origin, new Vector2(Squish.X * 1, Squish.Y) * i / 3, SpriteEffects.None);

            }


        }
      
        public void UpdateConeVFX(Vector2 HandPos)
        {
            Vector2 AdjustedSpawn = HandPos;
            Vector2 EndPos = new Vector2(0, -2);
            float MaxDistance = 240;
            if (ConeVFXPos == null)
            {
                ConeVFXPos = new Vector2[22];
                ConeVFXdistance = new float[22];
                ConeVFXRotation = new float[22];
                ConeVFXOpacity = new float[22];
                ConeVFXColor = new Color[22];
                for (int i = 0; i < ConeVFXPos.Length - 1; i++)
                {
                    ConeVFXdistance[i] = -Main.rand.NextFloat(MaxDistance);
                    ConeVFXRotation[i] = MathHelper.ToRadians(Main.rand.NextFloat(-24, 24));
                    ConeVFXPos[i] = new Vector2(ConeVFXdistance[i], 0).RotatedBy(ConeVFXRotation[i]).RotatedBy(swingDirection - MathHelper.PiOver2);
                    ConeVFXOpacity[i] = 0.0f;
                }
            }

            for (int i = 0; i < ConeVFXPos.Length - 1; i++)
            {

                ConeVFXdistance[i] = float.Lerp(ConeVFXdistance[i], 0, 0.02f);
                ConeVFXPos[i] = new Vector2(ConeVFXdistance[i], 0).RotatedBy(ConeVFXRotation[i]).RotatedBy(swingDirection - MathHelper.PiOver2);
                ConeVFXOpacity[i] = float.Lerp(ConeVFXOpacity[i], 1, 0.2f);
                if (ConeVFXPos[i].Distance(EndPos) < 22f)
                {
                    ConeVFXdistance[i] = -Main.rand.NextFloat(MaxDistance);

                    ConeVFXPos[i] = new Vector2(ConeVFXdistance[i], 0).RotatedBy(ConeVFXRotation[i]).RotatedBy(swingDirection - MathHelper.PiOver2);
                    ConeVFXOpacity[i] = 0;
                    ConeVFXColor[i] = RainbowColorGenerator.GenerateRandomColor();
                }
            }
            //Main.NewText($"Position: {ConeVFXPos[0]}");
        }
        public void DrawConeVFX(float Rotation)
        {
            if (ConeVFXPos != null && ConeVFXInterp >= 0.5f)
            {
                Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.Star;


                Vector2 HandPos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, swingDirection);
                Vector2 Origin = new Vector2(Debug.Width / 2, 0);
                Vector2 Scale = new Vector2(0.03f, 0.05f);


                for (int i = 0; i < ConeVFXPos.Length - 1; i++)
                {
                    Vector2 Pos = ConeVFXPos[i] + HandPos - Main.screenPosition;
                    float value = swingDirection.ToRotationVector2().AngleFrom(ConeVFXPos[i]) + MathHelper.PiOver2;
                    Color AdjustedColor = ConeVFXColor[i] * ConeVFXOpacity[i];
                    Main.EntitySpriteDraw(Debug, Pos, null, AdjustedColor, value, Debug.Size() * 0.5f, Scale, SpriteEffects.None);
                }
                /*
                for (int i = 0; i < ConeVFXPos.Length - 1; i++)
                {

                    Pos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, swingDirection) - Main.screenPosition;
                    Pos = ConeVFXPos[i];
                    Vector2 EndPos = new Vector2(16, -2 * Owner.direction);
                    float Rot = ConeVFXPos[i].AngleTo(EndPos) + MathHelper.PiOver2;
                    Main.EntitySpriteDraw(Debug, Pos, null, Color.AntiqueWhite with { A = 0 }, Rot, Debug.Size() * 0.5f, new Vector2(0.01f), SpriteEffects.None);


                }
                Pos = Vector2.Zero;

                for (int i = 0; i < ConeVFXPos.Length - 1; i++)
                {
                    Pos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, swingDirection) - Main.screenPosition;
                    Pos += ConeVFXPos[i].RotatedBy(Rotation);


                    Vector2 EndPos = new Vector2(16, -2 * Owner.direction);
                    Main.EntitySpriteDraw(Debug2, Pos, null, Color.AntiqueWhite with { A = 0 }, ConeVFXPos[i].AngleTo(EndPos), Debug2.Size() * 0.5f, 1, SpriteEffects.None);

                    Utils.DrawLine(Main.spriteBatch, Owner.Center + ConeVFXPos[i].RotatedBy(Owner.Center.AngleTo(Main.MouseWorld)),
                        Owner.Center + EndPos.RotatedBy(Owner.Center.AngleTo(Main.MouseWorld)), Color.AntiqueWhite * 0, Color.Blue, 1f);
                }
                */
            }
        }

        private Vector2[] _slashPositions;
        private float[] _slashRotations;

        private static VertexStrip _slashStrip;
        private void DrawSlash()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            // Have a shader prepared, only special thing is that it uses a normalized matrix
            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.RaptureSlash");

            trailShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 0, SamplerState.PointClamp);
            trailShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
            trailShader.TrySetParameter("uWorldViewProjection", Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailShader.TrySetParameter("uColor", Color.AliceBlue.ToVector4() * 0.66f);
            trailShader.TrySetParameter("BetterPi", float.Pi);
            trailShader.Apply();

            // Rendering primitives involves setting vertices of each triangle to form quads
            // This does it for us
            // Have a list of positions and rotations to create vertices, width function to determine how far vertices are from the center
            // Color function determines each vertex's color, which can be used in the shader
            _slashStrip ??= new VertexStrip();
            _slashStrip.PrepareStrip(_slashPositions, _slashRotations, RainbowColorGenerator.TrailColorFunction, TrailWidthFunction, Owner.Center - Main.screenPosition, _slashPositions.Length, true);
            _slashStrip.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
        private float TrailWidthFunction(float p) => 70 * Projectile.scale * _slashScale * Projectile.direction;

        #endregion
    }
}
