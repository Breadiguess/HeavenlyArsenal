
using CalamityMod;
using CalamityMod.DataStructures;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Core;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using BezierCurve = HeavenlyArsenal.Core.BezierCurve;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    partial class Aoe_Rifle_HeldProj : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];

        public Aoe_Rifle_Player RiflePlayer => Owner.GetModPlayer<Aoe_Rifle_Player>();
        public Rope rope;
        public enum RifleState
        {
            pullOut,
            Idle,
            Fire,
            Recoil,
            Cycle,
            Reload
        }
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public RifleState CurrentState
        {
            get => (RifleState)Projectile.ai[1];
            set => Projectile.ai[1] = (float)value;
        }

        private int AttackStage = 0;
        /// <summary>
        /// so the concept behind this is: by setting it up like this, we can make the rifle easily cycle through at the end of every sequence.
        /// </summary>
        private static readonly RifleState[] Pattern = new RifleState[]
        {
               RifleState.Idle,
               RifleState.Fire,
               RifleState.Recoil,
               RifleState.Cycle
        };


        public float RotationOffset = 0;

        public const int MAX_CLIP_SIZE = 5;
       
        public Vector2[] clipPos = new Vector2[]{
            new Vector2(0),
            new Vector2(0)
        };
        public static readonly int MaxClips = 2;
        public List<Aoe_Rifle_Clip> clips => Owner.HeldItem.GetGlobalItem<Aoe_Rifle_ClipItem>().Clips;

        public float Recoil;
        public int AmmoStored
        {
            get => RiflePlayer.BulletCount;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 28;
        }
        Vector2[] RopeAnchors = new Vector2[]
        {
            new Vector2(90, 0),
            new Vector2(0, 0)
        };
        public override void SetDefaults()
        {
            rope = new Rope(RopeAnchors[0] + Projectile.Center, RopeAnchors[1] + Projectile.Center, 37, 1, Vector2.One);
            for(int i = 0; i< rope.segments.Length; i++)
            {
                rope.segments[i].position = Projectile.Center;
            }
            rope.segments[0].position = RopeAnchors[0].RotatedBy(Projectile.rotation + RotationOffset) + Projectile.Center + new Vector2(Recoil, 0);
            rope.segments[^1].position = RopeAnchors[1].RotatedBy(Projectile.rotation + RotationOffset) + Projectile.Center + new Vector2(Recoil, 0);
            rope.Update();
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.frame = 0;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.Size = Vector2.One;
        }
        public override void AI()
        {
            CheckConditions();



            Projectile.Center = Owner.Center;
            Projectile.rotation = Owner.Calamity().mouseWorld.DirectionFrom(Projectile.Center).ToRotation();
            Projectile.velocity = Projectile.rotation.ToRotationVector2();
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.MountedCenter.AngleTo(Projectile.Center + new Vector2(50, 0).RotatedBy(Projectile.rotation)) - MathHelper.PiOver2);
            Owner.direction = Projectile.velocity.X.DirectionalSign() != 0 ? Projectile.velocity.X.DirectionalSign() : 1;
            StateMachine();

            Time++;
        }

        public override void PostAI()
        {
            Projectile.extraUpdates = 1;

            RopeAnchors = new Vector2[]
            {
            new Vector2(90, 0),
            new Vector2(-20, 20 * Owner.direction)
            };
            Recoil = float.Lerp(Recoil, 0, 0.2f);
            rope.damping = 0.4f;
            rope.segments[0].position = RopeAnchors[0].RotatedBy(Projectile.rotation + RotationOffset) + Projectile.Center + new Vector2(Recoil, 0);
            rope.segments[^1].position = RopeAnchors[1].RotatedBy(Projectile.rotation + RotationOffset) + Projectile.Center + new Vector2(Recoil, 0);
            rope.Update();
            RibbonPhysics();
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            float GlowOpacity = LumUtils.InverseLerp(3, 8, RiflePlayer.Authority);
            if (RiflePlayer.Authority > 4)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);
                for (int i = 0; i < 6; i++)
                {
                    Main.EntitySpriteDraw(RopeTarget, Owner.Center - Main.screenPosition + new Vector2(3, 0).RotatedBy(Projectile.rotation + MathHelper.TwoPi * i / 6f + MathF.Cos(Main.GlobalTimeWrappedHourly)), null, Color.Crimson with { A = 0 } * GlowOpacity, 0, RopeTarget.Size() / 2, 2, 0);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);

            Main.EntitySpriteDraw(RopeTarget, Owner.Center - Main.screenPosition, null, Color.White, 0, RopeTarget.Size() / 2, 2, 0);

            Main.spriteBatch.ResetToDefault();
            SpriteEffects flip = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);
            Main.EntitySpriteDraw(ClipTarget, Owner.Center - Main.screenPosition, null, Color.White, Projectile.rotation, ClipTarget.Size() / 2 , 2, flip);
            Main.spriteBatch.ResetToDefault();

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Rectangle Frame = tex.Frame(1, 28, 0, Projectile.frame);


            Vector2 DrawPos = Projectile.Center - Main.screenPosition + new Vector2(0, 10);
            Vector2 Origin = new Vector2(30 + Recoil, Frame.Height / 2);

            if (RiflePlayer.Authority > 4)
                for (int i = 0; i < 6; i++)
                {

                    Main.EntitySpriteDraw(tex, DrawPos + new Vector2(3, 0).RotatedBy(Projectile.rotation + MathHelper.TwoPi * i / 6f + Main.GlobalTimeWrappedHourly), Frame, Color.Red with { A = 0 } * GlowOpacity, Projectile.rotation + RotationOffset, Origin, 1f, flip);
                }
           Main.EntitySpriteDraw(tex, DrawPos, Frame, lightColor, Projectile.rotation + RotationOffset, Origin, 1f, flip);

            //string msg = "";

            // msg += $"{Time}";

                

            //Utils.DrawBorderString(Main.spriteBatch, msg, DrawPos, Color.White, 1);
            return false;//base.PreDraw(ref lightColor);
        }



        public override void Load()
        {
            On_Main.CheckMonoliths += On_Main_CheckMonoliths;
            On_Main.CheckMonoliths += RenderRopePixelated;
        }



        #region helper/state


        void CheckConditions()
        {
            if (Owner.dead || Owner.HeldItem.type != ModContent.ItemType<Aoe_Rifle_Item>())
            {
                Projectile.active = false;
                return;
            }
            Owner.heldProj = this.Projectile.whoAmI;
            Projectile.timeLeft++;
        }

        RifleState PickNextAction()
        {
            Projectile.frame = 0;
            if (AttackStage > Pattern.Length - 1)
                AttackStage = 0;
            RifleState nextState = Pattern[AttackStage];

            AttackStage++;
            Time = -1;
            if (AmmoStored <= 0 && (CurrentState == RifleState.Recoil || CurrentState == RifleState.Idle))
                nextState = RifleState.Reload;

            return nextState;
        }


        void StateMachine()
        {
            switch (CurrentState)
            {
                case RifleState.pullOut:
                    HandlePullOut();
                    break;
                case RifleState.Idle:
                    HandleIdle();
                    break;

                case RifleState.Fire:
                    HandleFire();
                    break;

                case RifleState.Recoil:
                    HandleRecoil();
                    break;

                case RifleState.Cycle:
                    HandleCycle();
                    break;

                case RifleState.Reload:
                    HandleReload();
                    break;


            }
        }

        private void HandlePullOut()
        {
            CurrentState = RifleState.Idle;
        }
        private void HandleIdle()
        {
            if (Owner.controlUseItem && Owner.altFunctionUse != 2)
            {
                CurrentState = PickNextAction();
            }

        }

        private void HandleFire()
        {
            Owner.SetDummyItemTime(2);
            Item ChosenBullet = GetBulletFromClip();
            int baseDamage = Owner.GetWeaponDamage(Owner.HeldItem);
            float knockback = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            int finalDamage = baseDamage + ChosenBullet.damage;
            float finalKnockback = knockback + ChosenBullet.knockBack;
            int damage = finalDamage;
            if (RiflePlayer.BulletCount == 1)
            {
                damage *= 2;
            }
            float screenshakeStrength = 1 - LumUtils.InverseLerp(0, 10, RiflePlayer.BulletCount) + (RiflePlayer.BulletCount == 1 ? 2 : 1);
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f * screenshakeStrength, shakeStrengthDissipationIncrement: RiflePlayer.BulletCount != 1 ? 0.4f : 0.2f);
            Projectile a = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
            Projectile.velocity * 120, ModContent.ProjectileType<Aoe_Rifle_Laser>(), damage, finalKnockback);

            a.As<Aoe_Rifle_Laser>().PowerShot = RiflePlayer.BulletCount == 1;
            a.timeLeft += RiflePlayer.BulletCount == 1 ? 2 : 0;

           
            if (RiflePlayer.BulletCount > 1)
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundStrong with { Volume = 2, Pitch = 0.7f * (1 - LumUtils.InverseLerp(1, 9, RiflePlayer.BulletCount)), Type = SoundType.Sound }, Owner.Center).WithVolumeBoost(8);
            else
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundSuper with { Volume = 2 , Type = SoundType.Sound }, Owner.Center).WithVolumeBoost(13);
            
            Recoil += 30;
            for (int x = 0; x < 2; x++)
            {
                var clip = clips[x];

                if (clip.BulletCount > 0)
                {
                    clip.Bullets.RemoveAt(0);
                    clips[x] = clip;
                    break;
                }
            }

            CurrentState = PickNextAction();

        }


        private void HandleRecoil()
        {
            Owner.SetDummyItemTime(2);
            int finishRecoiling = 40 * Projectile.extraUpdates;

            if (Time == 0)
                RotationOffset += MathHelper.ToRadians(30 * -Owner.direction);
            else
                RotationOffset = RotationOffset.AngleLerp(0, 0.1f);

            if (Time % 6 == 0 && RiflePlayer.Authority > 3)
            {

                Aoe_Rifle_DeathParticle particle = new Aoe_Rifle_DeathParticle();
                particle.Prepare(Owner.MountedCenter + new Vector2(Main.rand.NextFloat(-10, 11), 0) * 10, 0, 120, Owner, Main.rand.Next(0, Aoe_Rifle_DeathParticle.SymbolList.Length + 1));

                //ParticleEngine.BehindProjectiles.Add(particle);

            }

            if (Time > finishRecoiling)
                CurrentState = PickNextAction();
        }

        private void HandleCycle()
        {
            Owner.SetDummyItemTime(2);
            int finishCycling = 60 * Projectile.extraUpdates;

            if (Time == 0)
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.CycleSound with { PitchVariance = 0.1f, Type = SoundType.Sound }, Owner.Center).WithVolumeBoost(0.4f);
            RotationOffset = MathHelper.ToRadians(20 * Owner.direction) * LumUtils.InverseLerpBump(0, finishCycling / 3, finishCycling / 2, finishCycling, Time);

            if(Projectile.frame == 6)
            {
                Dust a = Dust.NewDustDirect(Projectile.Center, 40, 40, DustID.Torch, 0, 0);
                a.velocity = Projectile.rotation.ToRotationVector2() * -10;
            }
            Projectile.frame = (int)(15 * LumUtils.InverseLerp(0, finishCycling - 5, Time));
            if (Time >= finishCycling)
                CurrentState = PickNextAction();
        }


        public PiecewiseCurve ReloadCurve;
        public BezierCurve reloadMovement;
        private void HandleReload()
        {
            //TODO: clip system with animation
            // mostly cosmetic, but its all about the fantasy of the weapon, no?
            //simple at low authority, with exponentially more powerful visuals as authority increases
            //(think sigils forming into existance and burning out, cracks appearing in space around the weapon as the clip is pushed in,
            //a screaming, horrifiying power emenating that almost begs for another magazine)

            //also serves a gameplay feature to partially balance the insane power of the weapon, so you can't just spam it

            //also, at high authority, the clips should take a second to push in after being placed properly. this is for impact, and doesn't affect reload time. 
            // sync up a big visual or damage for when the clip is slotted in, because we need to auramaxx.

            //preferably this would scale with the amount of clips the player can load in (realistically its only two, but the ability to do this procedurally would be very nice)
            int PauseBeforeReload = 60 * Projectile.extraUpdates;

            int StartReloadClip0 = PauseBeforeReload + 60 * Projectile.extraUpdates;
            int EndReloadClip0 = StartReloadClip0 + 60 * Projectile.extraUpdates;

            int StartReloadClip1 = EndReloadClip0 + 60 * Projectile.extraUpdates;
            int EndReloadClip1 = StartReloadClip1 + 60 * Projectile.extraUpdates;

            int FinishReload = EndReloadClip1 + 60 * Projectile.extraUpdates;
            

            if (Time == 0)
            {
                ReloadCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Sine, EasingType.In, 0.1f, 0.4f)
                    .Add(EasingCurves.Linear, EasingType.Out, 0, 0.6f)
                    .Add(EasingCurves.Circ, EasingType.InOut, 1, 0.7f);


                reloadMovement = new Core.BezierCurve(new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(30,-50),
                    new Vector2(35, -50),
                    new Vector2(36,0)


                });
            }

            if (!Owner.HasAmmo(Owner.HeldItem))
            {
                if (Owner.controlUseItem)
                {
                    CombatText.NewText(Projectile.getRect(), Color.Red, "No Ammo ");
                }
                return;
            }

            Owner.SetDummyItemTime(2);
            if (Time == 0)
            {
                

                clipPos[0] = new Vector2(0, 0);

                clipPos[1] = new Vector2(0, 0);
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.ReloadSound with { Type = SoundType.Sound }, Owner.Center).WithVolumeBoost(3);
                //Assemble clips from ammo in player inventory
                for (int x = 0; x < 2; x++)
                {
                    var clip = clips[x];
                        

                    int difference = MAX_CLIP_SIZE - clip.BulletCount;
                    //Main.NewText(difference);
                    for (int i = 0; i < difference; i++)
                    {
                        Item Chosen = Owner.ChooseAmmo(Owner.HeldItem);
                        if (Chosen != null)
                        {
                            Item Stored = Chosen.Clone();
                            Stored.stack = 1;
                            clip.Bullets.Add(Stored);
                            //Main.NewText($"Added {clip.Bullets[i]}, {x},{i}");
                            if (Chosen.consumable)
                                Owner.ConsumeItem(Chosen.type);
                        }
                    }

                    clips[x] = clip;

                }


               
            }

            if(Time < StartReloadClip0)
                Projectile.frame = 15+ (int)(9 * LumUtils.InverseLerp(0, StartReloadClip0, Time));

            if(Time > EndReloadClip1)
                Projectile.frame = 7 + (int)(3 * LumUtils.InverseLerp(EndReloadClip1, FinishReload, Time));
            if (Time > StartReloadClip0 && Time <= EndReloadClip0)
            {
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Owner.MountedCenter.AngleTo(clipPos[0] + Owner.MountedCenter) - MathHelper.PiOver2 + Projectile.rotation);
                clipPos[0] = reloadMovement.Evaluate(LumUtils.InverseLerp(StartReloadClip0, EndReloadClip0, Time));
                if (Time == EndReloadClip0)
                {
                    for(int i = 0; i< 2; i++)
                    {
                        Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, new Vector2(i % 2 == 0 ? -1:1,0), ModContent.ProjectileType<Aoe_Rifle_RealityTear>(), 10_000, 0);
                    }
                    int loaded = Owner.HeldItem.GetGlobalItem<Aoe_Rifle_ClipItem>().AssembleClip(Owner, 0);

                    //RiflePlayer.BulletCount += loaded;

                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.MagEmptySound with { Pitch = -0.3f }, Owner.Center).WithVolumeBoost(1);
                    //RiflePlayer.BulletCount += clips[0].BulletCount;
                }
            }

            if (Time > StartReloadClip1 && Time <= EndReloadClip1)
            {
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Owner.MountedCenter.AngleTo(clipPos[1] + Owner.MountedCenter) - MathHelper.PiOver2 + Projectile.rotation);
                clipPos[1] = reloadMovement.Evaluate(LumUtils.InverseLerp(StartReloadClip1, EndReloadClip1, Time));
                if (Time == EndReloadClip1)
                {
                    int loaded = Owner.HeldItem.GetGlobalItem<Aoe_Rifle_ClipItem>().AssembleClip(Owner, 0);

                    //RiflePlayer.BulletCount += loaded;

                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.MagEmptySound with { Pitch = 0.3f }, Owner.Center).WithVolumeBoost(1);
                    //RiflePlayer.BulletCount += clips[1].BulletCount;
                }

            }

            if (Time > FinishReload)
            {

                Time = -1;
                CurrentState = RifleState.Idle;
                AttackStage = 0;

            }

        }



        /// <summary>
        /// prepare clips for the rifle so that we have empty clips to fill + doesn't crash
        /// </summary>
       
        private Item GetBulletFromClip()
        {
            Item thing = null;
            for (int x = 0; x < 2; x++)
            {
                var clip = clips[x];

                if (clip.BulletCount > 0)
                {
                    thing = clip.Bullets[0].Clone();

                    break;
                }
            }

            return thing;
        }



        #region ribbon
        private Vector2[] ribbonPoints;

        private Vector2[] ribbonVels;

        public BasicEffect RibbonEffect;

        public List<VertexPositionColorTexture> RibbonVerts = new List<VertexPositionColorTexture>();
        public void RibbonPhysics()
        {
            var length = 6; // Number of ribbon segments
            var gravity = new Vector2(0, 0.7f); // Gravity to pull the ribbon downward
            var maxDistance = 3f; // Spacing between segments
            var dampening = 0.5f; // Damping factor to stabilize motion

            // Initialize velocities if null
            if (ribbonVels == null)
            {
                ribbonVels = new Vector2[length];
            }

            // Initialize ribbon points if null
            if (ribbonPoints == null)
            {
                ribbonPoints = new Vector2[length];

                for (var i = 0; i < ribbonPoints.Length; i++)
                {
                    ribbonPoints[i] = Projectile.Center;
                }
            }

            //ribbonPoints[0] = AnchorPosition();
            var drawScale = Projectile.scale;

            // Update ribbon segments with physics
            for (var i = 1; i < ribbonPoints.Length; i++)
            {
                // Apply velocity and gravity
                ribbonVels[i] *= dampening; // Reduce velocity slightly to stabilize
                ribbonVels[i] += gravity; // Apply gravity
                ribbonPoints[i] += ribbonVels[i]; // Update position based on velocity

                // Enforce distance constraint
                var direction = ribbonPoints[i] - ribbonPoints[i - 1];
                var currentDistance = direction.Length();

                if (currentDistance > maxDistance)
                {
                    // Correct positions to maintain spacing
                    var correction = direction.SafeNormalize(Vector2.Zero) * (currentDistance - maxDistance) * 0.5f;
                    ribbonPoints[i] -= correction;
                    ribbonPoints[i - 1] += correction;
                }

                // Apply rotation based on the segment's direction
                var segmentRotation = direction.ToRotation();

                if (i == ribbonPoints.Length - 1)
                {
                    // Store or apply the rotation for the last segment
                    ribbonVels[i] = segmentRotation.ToRotationVector2() * ribbonVels[i].Length();
                }

                ribbonPoints[0] = rope.segments[0].position;
            }
        }
        void buildRibbon(Aoe_Rifle_HeldProj proj)
        {
            var ribbonPoints = proj.ribbonPoints;

            if (ribbonPoints == null)
                return;
            float ropeWidth = 3f;
            int count = ribbonPoints.Length;

            for (int i = 0; i < count; i++)
            {
                Vector2 curr = ribbonPoints[i] - Main.screenPosition;

                Vector2 dir;
                if (i == 0)
                    dir = ribbonPoints[i + 1] - curr;
                else if (i == count - 1)
                    dir = curr - ribbonPoints[i - 1];
                else
                    dir = ribbonPoints[i + 1] - ribbonPoints[i - 1];

                if (dir.LengthSquared() < 0.001f)
                    continue;

                dir.Normalize();

                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float t = i / (float)(count - 1);

                Color color = Color.Lerp(
                    Color.Crimson,
                    Color.Crimson,
                    t
                );
                color = color.MultiplyRGB(Lighting.GetColor(ribbonPoints[i].ToTileCoordinates(), color));
                Vector2 left = curr - normal * ropeWidth * 0.5f;
                Vector2 right = curr + normal * ropeWidth * 0.5f;

                proj.verts.Add(new VertexPositionColorTexture(
                    new Vector3(left, 0f),
                    color,
                    new Vector2(0f, t)
                ));

                proj.verts.Add(new VertexPositionColorTexture(
                    new Vector3(right, 0),
                    color,
                    new Vector2(1f, t)
                ));
            }
        }
        void RenderPrimitiveRibbon(Aoe_Rifle_HeldProj proj)
        {

            if (Main.netMode == NetmodeID.Server)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            var RibbonEffect = proj.RibbonEffect;
            if (RibbonEffect == null)
            {
                RibbonEffect = new BasicEffect(gd)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,

                    Texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel
                };
                proj.RibbonEffect = RibbonEffect;
            }
            RibbonEffect.World = Matrix.Identity;
            RibbonEffect.View = Matrix.Identity;
            RibbonEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000, 1000);
            foreach (EffectPass pass in RibbonEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Main.instance.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    proj.RibbonVerts.ToArray(),
                    0,
                    proj.RibbonVerts.Count - 2
                );
            }
            proj.RibbonVerts.Clear();
        }
        void renderRibbon(Aoe_Rifle_HeldProj proj)
        {
            Texture2D stringRopeTexture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D beadRopeTexture = GennedAssets.Textures.SecondPhaseForm.Beads3;
            var ribbonPoints = proj.ribbonPoints;

            if (ribbonPoints != null)
            {
                for (var i = 0; i < ribbonPoints.Length - 1; i++)
                {
                    var direction = ribbonPoints[i + 1] - ribbonPoints[i];
                    var rotation = direction.ToRotation();
                    var segmentLength = direction.Length();

                    // Use scaling/stretching for the rope appearance
                    var stretch = new Vector2(1f, segmentLength / stringRopeTexture.Height);


                }

                var endPoint = ribbonPoints[ribbonPoints.Length - 1];

                // Calculate rotation for the last segment
                var lastDirection = endPoint - ribbonPoints[ribbonPoints.Length - 2]; // Direction from the second-to-last point to the last point
                var lastRotation = lastDirection.ToRotation();
                var ClothAnchorPoint = new Vector2(beadRopeTexture.Width / 2, 0);

                Main.EntitySpriteDraw
                (
                    beadRopeTexture,
                    endPoint - Main.screenPosition,
                    null,
                    Lighting.GetColor(endPoint.ToTileCoordinates()),
                    lastRotation - MathHelper.PiOver2,
                    ClothAnchorPoint,
                    0.13f,
                    SpriteEffects.None
                );
            }


            buildRibbon(proj);
            RenderPrimitiveRibbon(proj);
        }
        #endregion
        #region Clip Rendering
        public static RenderTarget2D ClipTarget { get; set; }
        private void On_Main_CheckMonoliths(On_Main.orig_CheckMonoliths orig)
        {
            if (ClipTarget == null || ClipTarget.IsDisposed)
                ClipTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            else if (ClipTarget.Size() != new Vector2(Main.screenWidth / 2, Main.screenHeight / 2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    ClipTarget.Dispose();
                    ClipTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(ClipTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Projectile projectile in Main.projectile.Where(n => n.active && n.ai[0] > 0 && n.type == ModContent.ProjectileType<Aoe_Rifle_HeldProj>()))
            {
                RenderClip(projectile);


            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();

        }
        void RenderClip(Projectile proj)
        {
            Aoe_Rifle_HeldProj gun = proj.ModProjectile as Aoe_Rifle_HeldProj;

            if (gun.CurrentState != RifleState.Reload)
                return;

            for (int x = 0; x < gun.clips.Count; x++)
            {
                Vector2 DrawPos = gun.Owner.Center + gun.clipPos[x] - Main.screenPosition;
                if (gun.clips[x].Bullets != null)
                    for (int i = 0; i < gun.clips[x].Bullets.Count; i++)
                    {
                        Color thing = Lighting.GetColor((proj.Center + new Vector2(x, 4 * i)).ToTileCoordinates());

                        var clip = gun.clips[x].Bullets[i];
                        if (clip != null)
                        {
                            if (clip.type != ItemID.EndlessMusketPouch)
                                Main.instance.LoadItem(clip.type);
                            else
                            {

                                Main.instance.LoadItem(ItemID.MusketBall);
                                clip.type = ItemID.MusketBall;
                            }
                            //Main.NewText($"{x},{i} type = {clip.Name}");
                            Texture2D bullet = TextureAssets.Item[clip.type].Value;
                             Main.EntitySpriteDraw(bullet, DrawPos + new Vector2(x, 4 * i), null, thing, MathHelper.PiOver2, new Vector2(bullet.Width / 2, bullet.Height), 0.5f, 0);
                        }
                        Utils.DrawLine(Main.spriteBatch, DrawPos + new Vector2(x, -2) + Main.screenPosition, DrawPos + new Vector2(x, 22) + Main.screenPosition, thing.MultiplyRGB(Color.Red), thing.MultiplyRGB(Color.Red), 3);
                    }

            }

        }
        #endregion

        #region Rope Rendering
        public static RenderTarget2D RopeTarget;
        private void RenderRopePixelated(On_Main.orig_CheckMonoliths orig)
        {
            if (RopeTarget == null || RopeTarget.IsDisposed)
                RopeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            else if (RopeTarget.Size() != new Vector2(Main.screenWidth / 2, Main.screenHeight / 2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    RopeTarget.Dispose();
                    RopeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(RopeTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Projectile projectile in Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<Aoe_Rifle_HeldProj>()))
            {
                RenderRope(projectile);
                renderRibbon(projectile.ModProjectile as Aoe_Rifle_HeldProj);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();

        }


        public void RenderRope(Projectile proj)
        {
            Aoe_Rifle_HeldProj rifle = proj.ModProjectile as Aoe_Rifle_HeldProj;
            buildRope(rifle);
            RenderPrimitiveRope(rifle);

            Texture2D orb = AssetDirectory.Textures.Items.Weapons.Ranger.Pearl.Value;

            for (int i = 0; i < rifle.RiflePlayer.Authority; i++)
            {
                int val = Math.Clamp(5 + i * 4, 0, rifle.rope.segments.Length);
                Main.EntitySpriteDraw(orb, rifle.rope.segments[val].position - Main.screenPosition, null, Lighting.GetColor(rifle.rope.segments[val].position.ToTileCoordinates()), proj.rotation, orb.Size() / 2, 0.2f, 0);


            }



        }
        public BasicEffect RopeEffect;
        public List<VertexPositionColorTexture> verts = new List<VertexPositionColorTexture>();
        void buildRope(Aoe_Rifle_HeldProj proj)
        {
            var rope = proj.rope;
            if (rope == null)
                return;
            float ropeWidth = 3f;
            int count = rope.segments.Length;

            for (int i = 0; i < count; i++)
            {
                Vector2 curr = rope.segments[i].position - Main.screenPosition;

                Vector2 dir;
                if (i == 0)
                    dir = rope.segments[i + 1].position - curr;
                else if (i == count - 1)
                    dir = curr - rope.segments[i - 1].position;
                else
                    dir = rope.segments[i + 1].position - rope.segments[i - 1].position;

                if (dir.LengthSquared() < 0.001f)
                    continue;

                dir.Normalize();

                Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

                float t = i / (float)(count - 1);

                Color color = Color.Lerp(
                    Color.Crimson,
                    Color.MediumAquamarine,
                    MathF.Cos(Main.GlobalTimeWrappedHourly + t * 8)
                );
                color = color.MultiplyRGB(Lighting.GetColor(rope.segments[i].position.ToTileCoordinates()));
                Vector2 left = curr - normal * ropeWidth * 0.5f;
                Vector2 right = curr + normal * ropeWidth * 0.5f;

                proj.verts.Add(new VertexPositionColorTexture(
                    new Vector3(left, 0f),
                    color,
                    new Vector2(0f, t)
                ));

                proj.verts.Add(new VertexPositionColorTexture(
                    new Vector3(right, 0),
                    color,
                    new Vector2(1f, t)
                ));
            }
        }
        void RenderPrimitiveRope(Aoe_Rifle_HeldProj proj)
        {

            if (Main.netMode == NetmodeID.Server)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            var RopeEffect = proj.RopeEffect;
            if (RopeEffect == null)
            {
                RopeEffect = new BasicEffect(gd)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,

                    Texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel
                };
                proj.RopeEffect = RopeEffect;
            }
            RopeEffect.World = Matrix.Identity;
            RopeEffect.View = Matrix.Identity;
            RopeEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1, 1);
            foreach (EffectPass pass in RopeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Main.instance.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    proj.verts.ToArray(),
                    0,
                    proj.verts.Count - 2
                );
            }
            proj.verts.Clear();
        }
        #endregion

        #endregion
    }
}
