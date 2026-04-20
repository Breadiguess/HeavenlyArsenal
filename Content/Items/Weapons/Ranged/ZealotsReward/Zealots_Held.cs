using CalamityMod;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using CalRemix.Content.Items.Weapons;
using CalRemix.Content.NPCs.Bosses.Origen;
using Luminance.Core.Graphics;
using NoxusBoss.Core.SoundSystems;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_Held : ModProjectile
    {
        private enum State
        {
            None,
            Charge,
            Fire,
            Recoil,


            AltFireUse
        }
        public static readonly SoundStyle IceLaunch = new SoundStyle(Zealots_Item.Path + "_Spike");
        public static readonly SoundStyle IceCharge = new SoundStyle(Zealots_Item.Path + "_Charge_Ice");
        public static readonly SoundStyle Charging = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/FusionRifle/ZealotsCharge");
        public static readonly SoundStyle Fire = new SoundStyle(Zealots_Item.Path + "_Fire", 4);

        public static readonly SoundStyle ZealotsCharge = new SoundStyle(Zealots_Item.Path + "_Charge");
        #region setup
        public ref float ChargeInterp => ref Projectile.ai[2];

        private State CurrentState = State.None;

        public ref Player Owner => ref Main.player[Projectile.owner];

        public override string Texture => Zealots_Item.Path;


        public LoopedSoundInstance? ChargeSound
        {
            get;
            private set;
        }

        public override void SetStaticDefaults()
        {

        }
        public int Timer;
        public int MaxChargeTime
        {
            get
            {
                // Base charge time in seconds at 1.0x attack speed.
                float baseSeconds = 0.62f;
                if (Owner.HeldItem.ModItem is not Zealots_Item)
                {
                    return (int)(SecondsToFrames(baseSeconds));
                }


                // Total attack speed multiplier for the weapon's damage class.
                float attackSpeed = Owner.HeldItem.ModItem.UseSpeedMultiplier(Owner);

                attackSpeed = Math.Max(attackSpeed, 0.01f);

                return (int)(SecondsToFrames(baseSeconds) * Projectile.extraUpdates / attackSpeed);
            }
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }
        public Vector2 RotatedOffset = new Vector2(0, 0);
        public override void OnSpawn(IEntitySource source)
        {

        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        #endregion

        public int BurstCount
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        float recoil;

        public float HeatInterpolant;
        public override void AI()
        {
            CheckDespawnConditions();
            UpdateOwner();

            UpdateHeldPosition();
            stateMachine();

            Timer++;
        }

        #region stateMachine
        private void stateMachine()
        {
            switch (CurrentState)
            {
                case State.None:
                    {
                        if (ChargeInterp > 0)
                            ChargeInterp = float.Lerp(ChargeInterp, 0, 0.1f);

                        Timer = -1;
                        if (Owner.controlUseItem && Owner.altFunctionUse != 2)
                            CurrentState = State.Charge;

                        else if (Owner.altFunctionUse == 2 && Owner.GetModPlayer<Zealots_Player>().CanUseAltFire)
                        {
                            CurrentState = State.AltFireUse;
                            Owner.StartChanneling(Owner.HeldItem);
                        }

                    }
                    break;
                case State.Charge:
                    ManageCharge();
                    break;
                case State.Fire:
                    ManageFiring();
                    break;
                case State.Recoil:
                    ManageRecoil();
                    break;
                case State.AltFireUse:
                    ManageAltFire();
                    break;
                default:
                    break;
            }
        }


        void CheckDespawnConditions()
        {
            if (Owner.HeldItem.type != ModContent.ItemType<Zealots_Item>() || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
        }
        void UpdateHeldPosition()
        {
            RotatedOffset = Vector2.Lerp(RotatedOffset, new Vector2(2 - recoil * 10, 0), 0.2f);
            Projectile.timeLeft = 2;

            Projectile.Center = Owner.MountedCenter + new Vector2(4, 0).RotatedBy(Projectile.rotation) + new Vector2(0, 4) + new Vector2(0, Owner.gfxOffY);
            Projectile.velocity =Owner.Center.AngleTo(Owner.Calamity().mouseWorld).ToRotationVector2() * 9f;
            Projectile.velocity.SafeNormalize(Vector2.UnitX);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(4 * Owner.direction)
                + Math.Abs(recoil) * -Owner.direction + MathHelper.ToRadians(Math.Abs(AltFireRecoil) * -Owner.direction);



            Owner.SetCompositeArmBack(true, CalcStretch(Owner.Distance(Projectile.Center).Squared()),
                (Owner.MountedCenter).AngleTo(Projectile.Center + RotatedOffset * Owner.direction - new Vector2(1 * -Owner.direction, 0)) -
                MathHelper.PiOver2 + MathHelper.ToRadians(10 * -Owner.direction))   ;

            Owner.SetCompositeArmFront(true, CalcStretch(Owner.Distance(Projectile.Center).Squared()),
                (Owner.MountedCenter).AngleTo(Projectile.Center+RotatedOffset*Owner.direction - new Vector2(1 * -Owner.direction, 0)) - 
                MathHelper.PiOver2 + MathHelper.ToRadians(10 * -Owner.direction));

            HeatInterpolant = Math.Clamp(HeatInterpolant, 0, 1);
            HeatInterpolant = float.Lerp(HeatInterpolant, 0, 0.005f);
            recoil = float.Lerp(recoil, 0, 0.1f);
        }
        private Player.CompositeArmStretchAmount CalcStretch(float interp)
        {
           //Main.NewText(interp);
            if (interp > 10f && interp < 30)

                return Player.CompositeArmStretchAmount.ThreeQuarters;

            if (interp < 10f)
                return Player.CompositeArmStretchAmount.Quarter;


            return Player.CompositeArmStretchAmount.Full;
        }
        void UpdateOwner()
        {

            Owner.heldProj = Projectile.whoAmI;


            Owner.ChangeDir(Owner.Calamity().mouseWorldDeltaFromPlayer.X.DirectionalSign());
        }

        void ManageCharge()
        {

            if (!Owner.HasAmmo(Owner.HeldItem) || !Owner.controlUseItem)
            {
                CurrentState = State.None;
                return;
            }
            ChargeInterp = LumUtils.InverseLerp(0, MaxChargeTime, Timer);

            HeatInterpolant += 0.01f;
            if (ChargeInterp == 1)
            {
                Timer = -1;
                CurrentState = State.Fire;
            }
        }

        private void ManageFiring()
        {

            int baseInterval = 8;
            const int minInterval = 1;
            if (Timer == 0)
            {
                Owner.PickAmmo(Owner.HeldItem, out _, out _, out _, out _, out _, false);
                
            }


            int interval = Math.Max(minInterval, baseInterval - (BurstCount / 2));

            if (interval <= 0)
            {
                interval = minInterval;
            }

            if (Timer % interval == 0)
            {

                FireProjectile();
            }

            if (BurstCount >= 7)
            {
                ChargeInterp = 0;
                CurrentState = State.None;
                BurstCount = 0;
            }

        }


        void FireProjectile()
        {

            int damage = (int)(Owner.HeldItem.damage);


            float knockback = Projectile.knockBack;
            Vector2 spawnPosition = Projectile.Center + Projectile.rotation.ToRotationVector2().SafeNormalize(Vector2.Zero) * 20f + new Vector2(0, -4); ;


            float angleVariance = MathHelper.ToRadians(1f);
            float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance);
            Vector2 adjustedVelocity = Projectile.rotation.ToRotationVector2().RotatedBy(randomAngle);


            Projectile a = Projectile.NewProjectileDirect(Owner.HeldItem.GetSource_FromThis(), spawnPosition, adjustedVelocity * (12f),
               ModContent.ProjectileType<Zealots_BoltProj>(),
               damage,
               knockback,
               Projectile.owner
           );

            if (BurstCount % 1 == 0)
            {
                float difference = 7 - BurstCount;
                difference = SmoothStep(0, 1, difference / 7f);
                SoundEngine.PlaySound(Fire with { Pitch = -0.4f + LumUtils.InverseLerp(0, 7, BurstCount), MaxInstances = 0, Type = SoundType.Sound },
                    Projectile.Center).WithVolumeBoost(difference);
                Zealots_FireParticle particle = new(spawnPosition + Projectile.rotation.ToRotationVector2() * 24, Projectile.rotation, 20);
                GeneralParticleHandler.SpawnParticle(particle, false, CalamityMod.Enums.GeneralDrawLayer.AfterPlayers);


            }

            for (int i = 0; i < 2; i++)
            {
                float upwardVariation = Main.rand.NextFloat(-1.5f, 1.5f);
                MediumMistParticle mist = new MediumMistParticle(Projectile.position + new Vector2(20, 4*Owner.direction).RotatedBy(Projectile.rotation), (-Projectile.velocity + new Vector2(0, upwardVariation)) * Main.rand.NextFloatDirection(), // This velocity makes it slowly float upward
                Main.rand.NextBool(3) ? Color.LightSteelBlue : Color.SteelBlue, Color.CadetBlue, Main.rand.NextFloat(0.4f, 0.65f), 200);
                GeneralParticleHandler.SpawnParticle(mist, false, BurstCount % 2 == 0 ? CalamityMod.Enums.GeneralDrawLayer.BeforeProjectiles : CalamityMod.Enums.GeneralDrawLayer.AfterPlayers);


            }


            a.CritChance = Owner.HeldItem.crit + BurstCount;

            recoil += 0.055f * BurstCount;
            BurstCount++;


            Owner.PickAmmo(Owner.HeldItem, out _, out _, out _, out _, out _, false);
            Timer = 0;
        }
        public float AltFireChargeInterpolant = 0;
        public float AltFireRecoil;
        private void ManageAltFire()
        {
            float ChargeTime = 3 * 60 * Projectile.extraUpdates;
            Projectile.velocity += Main.rand.NextVector2Unit() * AltFireChargeInterpolant;
            if (!Owner.GetModPlayer<Zealots_Player>().CanUseAltFire)
            {
                AltFireChargeInterpolant = 0;
                Timer = -1;
                CurrentState = State.None;
                return;
            }
            if (Timer == 1)
            {
                SoundEngine.PlaySound(IceCharge with { PitchVariance = 0.2f, MaxInstances = 0 }, Projectile.Center);
            }
            Owner.SetDummyItemTime(2);
            HeatInterpolant += 0.12f;
            AltFireChargeInterpolant = LumUtils.InverseLerp(0, ChargeTime, Timer);

            if (Timer > ChargeTime)
            {
                Owner.SetDummyItemTime(0);
                Owner.GetModPlayer<Zealots_Player>().AltFireCooldown = Zealots_Player.MAXCOOLDOWN;
                AltFireChargeInterpolant = 0;
                CreateIceSpike();
                Main.gamePaused = true;
                Timer = -1;
                CurrentState = State.Recoil;
            }

        }

        void CreateIceSpike()
        {

            SoundEngine.PlaySound(IceLaunch, Owner.Center).WithVolumeBoost(4);
            int damage = (int)(Projectile.originalDamage * 7);

            float knockback = Projectile.knockBack;
            Vector2 spawnPosition = Projectile.Center + Projectile.rotation.ToRotationVector2().SafeNormalize(Vector2.Zero) * 50f + new Vector2(0, -4); ;
            Zealots_FireParticle particle = new(spawnPosition + Projectile.rotation.ToRotationVector2() * -4, Projectile.rotation, 60, true);
            GeneralParticleHandler.SpawnParticle(particle, false, CalamityMod.Enums.GeneralDrawLayer.AfterPlayers);


            float angleVariance = MathHelper.ToRadians(1f);
            float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance);
            Vector2 adjustedVelocity = (Projectile.rotation).ToRotationVector2().RotatedBy(randomAngle);

            // Spawn the projectile with the adjusted velocity
            Projectile a = Projectile.NewProjectileDirect(Owner.HeldItem.GetSource_FromThis(), spawnPosition, adjustedVelocity * (18f),
               ModContent.ProjectileType<Zealots_IceSpike>(),
               damage,
               knockback,
               Projectile.owner
           );
            Owner.velocity -= adjustedVelocity * 24;

        }



        private void ManageRecoil()
        {
            const float HoldTime = 90;


            if (Timer < HoldTime)
            {
                AltFireRecoil = 110 * QuintInOut(1 - LumUtils.InverseLerp(0, HoldTime - 10, Timer));
                return;
            }

            if (AltFireRecoil > 0)
                AltFireRecoil--;


            if (AltFireRecoil == 0)
                CurrentState = State.None;
        }
        #endregion
        private float _CachedInterpolant;
        public override bool PreDraw(ref Color lightColor)
        {



            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;


            Texture2D HeatTex = ModContent.Request<Texture2D>(Texture + "_Heat").Value;


            Vector2 DrawPos = Projectile.Center + RotatedOffset.RotatedBy(Projectile.rotation) - Main.screenPosition;

            Vector2 Origin = new Vector2(texture.Width / 2, texture.Height / 2);

            float Rot = Projectile.rotation;


            SpriteEffects flip = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;


            float scale = 0.75f;

            Main.EntitySpriteDraw(texture, DrawPos, null, lightColor, Rot, Origin, scale, flip);

            DrawIcicle(DrawPos, Rot, flip);

            DrawIceBuildup(DrawPos, Rot, Origin, scale, flip);
         
            Main.spriteBatch.UseBlendState(BlendState.Additive);

            for (int i = 0; i < 10; i++)
            {
                Color HeatColor = Color.Lerp(Color.White, Color.Aqua, i / 9f) * HeatInterpolant;
                Main.EntitySpriteDraw(HeatTex, DrawPos, null, HeatColor, Rot, Origin, scale, flip);

            }

            Main.spriteBatch.ResetToDefault();

            return false;
        }

        void DrawIcicle(Vector2 DrawPos, float Rot, SpriteEffects flip)
        {
            Main.spriteBatch.EnterShaderRegion();

            Texture2D IcicleTex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/ZealotsReward/Zealots_IceSpike").Value;

            float progress = MathF.Pow(AltFireChargeInterpolant, 0.7f);
            progress = MathHelper.SmoothStep(2f, 0, progress);

            Vector2 direction = new Vector2(-1f, 0f);
            if (direction != Vector2.Zero)
                direction.Normalize();

            //Main.NewText(progress);
            //float noiseStrength;
            //float gradientStrength;
            var Dissolve = ShaderManager.GetShader("HeavenlyArsenal.DirectionalDissolveShader");
            Dissolve.SetTexture(IcicleTex, 0);
            Dissolve.SetTexture(GennedAssets.Textures.Noise.CrackedNoiseA, 1);
            Dissolve.TrySetParameter("dissolveProgress", progress);//MathF.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly)));
            Dissolve.TrySetParameter("edgeWidth", 0.6f);
            Dissolve.TrySetParameter("opacity", 1-progress);
            Dissolve.TrySetParameter("noiseStrength", 1);
            Dissolve.TrySetParameter("gradientStrength", (1 - progress));
            Dissolve.TrySetParameter("edgeColor", Color.CadetBlue.ToVector4());
            Dissolve.TrySetParameter("dissolveDirection", direction);
            Dissolve.TrySetParameter("directionalStrength", 0.1f);
            Dissolve.Apply();

            Main.EntitySpriteDraw(IcicleTex, DrawPos + new Vector2(29, -2 * Projectile.direction).RotatedBy(Rot), null, Color.White, Rot, new Vector2(0, IcicleTex.Height / 2), 1f, flip);

            Main.spriteBatch.ResetToDefault();
        }

        void DrawIceBuildup(Vector2 DrawPos, float Rot, Vector2 Origin, float scale, SpriteEffects flip)
        {
            Main.spriteBatch.EnterShaderRegion();

            Texture2D FrostBuildUp = ModContent.Request<Texture2D>(Texture + "_FrostOverlay").Value;
            //this is mostly fine, but it doesn't look great hwen the ice just disapears entirely. 
            float inte = (Owner.GetModPlayer<Zealots_Player>().AltFireCooldown) / (float)Zealots_Player.MAXCOOLDOWN;
            _CachedInterpolant = MathHelper.SmoothStep(0f, 1f, inte);

            var FrostBuildup = ShaderManager.GetShader("HeavenlyArsenal.DissolveShader");

            Color edgeColor = Color.CadetBlue;
            FrostBuildup.SetTexture(FrostBuildUp, 0);
            FrostBuildup.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 1);
            FrostBuildup.TrySetParameter("dissolveProgress", Math.Abs(_CachedInterpolant * 1.1f));
            FrostBuildup.TrySetParameter("edgeWidth", 0.1f);

            FrostBuildup.TrySetParameter("opacity", Math.Clamp(1 - _CachedInterpolant, 0, 1.1f));
            FrostBuildup.TrySetParameter("edgeColor", edgeColor.ToVector4());
            FrostBuildup.Apply();
            Main.EntitySpriteDraw(FrostBuildUp, DrawPos, null, Color.White, Rot, Origin, scale, flip);

        }
        private static float QuintInOut(float x)
        {
            if (x < 0.5f)
                return 16 * x * x * x * x * x;
            else
                return 1 - MathF.Pow(-2 * x + 2, 5) / 2;
        }
    }
}
