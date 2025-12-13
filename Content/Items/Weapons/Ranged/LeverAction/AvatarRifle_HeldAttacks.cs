using CalamityMod;
using CalamityMod.Particles;
using HeavenlyArsenal.Common.Graphics;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction
{
    partial class AvatarRifle_Held
    {
        public float LeverCurveOutput;
        public PiecewiseCurve LeverCurve;
        public float t;

        public float GunSwingCurveOutput
        {
            get => GunSwingCurve != null ? GunSwingCurve.Evaluate(t2) : 0;
        }
        public PiecewiseCurve GunSwingCurve;
        public float t2;
        public enum State
        {
            debug,
            Idle,
            Holster,

            Fire,
            Cycle,
            Reload,

            Bunt
        }
        public State CurrentState;
        public void StateMachine()
        {
            switch (CurrentState)
            {
                case State.debug:

                    CurrentState = State.Idle;
                    break;
                case State.Idle:
                    ManageIdle();
                    break;
                case State.Fire:
                    ManageFire();
                    break;
                case State.Cycle:
                    ManageCycle();
                    break;
                case State.Reload:
                    ManageReload();
                    break;
            }
        }

        void ManageIdle()
        {
            Time = -1;
            float Rot = Projectile.rotation - MathHelper.PiOver2;
            Rot += MathHelper.ToRadians(10);
            Owner.SetCompositeArmFront(true, Terraria.Player.CompositeArmStretchAmount.Full, Rot);
            if (Owner.controlUseItem)
            {
                CurrentState = State.Fire;
            }
        }
        void ManageFire()
        {
            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundStrong with { Volume = 2}, Owner.Center);
            int bulletAMMO = ProjectileID.Bullet;
            Owner.PickAmmo(Owner.ActiveItem(), out bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out int _);
            Vector2 Velocity = Owner.AngleTo(Owner.Calamity().mouseWorld).ToRotationVector2() * 10;

            Main.NewText(riflePlayer.ShotCount);
            Projectile shot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Velocity, bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
            AvatarRifle_MuzzleFlash MuzzleFlash = AvatarRifle_MuzzleFlash.pool.RequestParticle();
            Vector2 tip = Projectile.Center + new Vector2(10, 0).RotatedBy(Projectile.rotation);
            MuzzleFlash.Prepare(tip, Projectile.rotation, 40);

            ParticleEngine.Particles.Add(MuzzleFlash);
            riflePlayer.ShotCount--;
            CurrentState = State.Cycle;
            Owner.SetDummyItemTime(18);
            Time = -1;
        }
        void ManageCycle()
        {
            if (riflePlayer.ShotCount <= 0)
            {
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.CycleEmptySound);
                CurrentState = State.Reload;
                return;
            }

            float Rot = Projectile.rotation - MathHelper.PiOver2 - RotationOffset * LeverCurveOutput * 2;
            Rot += MathHelper.ToRadians(10);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Rot);
            if (Time < 3)
                return;
            if (Time == 4)
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.CycleSound);
            LeverCurve = new PiecewiseCurve()
                .Add(EasingCurves.Sine, EasingType.InOut, 1, 0.4f)
                .Add(EasingCurves.Linear, EasingType.Out, 1, 0.6f)
                .Add(EasingCurves.Sine, EasingType.Out, 0, 1f);

            if (t < 0.75f)
                RotationOffset = float.Lerp(RotationOffset, MathHelper.ToRadians(-40), 0.2f);
            else
                RotationOffset = float.Lerp(RotationOffset, 0, 0.3f);
                LeverCurveOutput = LeverCurve.Evaluate(t);


            t = Math.Clamp(t + 0.04f, 0, 1);
            Owner.SetDummyItemTime(2);
            if (t == 1)
            {
                t = 0;
                CurrentState = State.Idle;
            }
        }

        void ManageReload()
        {
            //float Rot = Projectile.rotation - MathHelper.PiOver2 - RotationOffset * LeverCurveOutput * 2;
            //

            Projectile.Center = Owner.MountedCenter + new Vector2(10, 2);
            float Rot = Owner.MountedCenter.AngleTo(Projectile.Center) - MathHelper.PiOver2;

            GunSwingCurve = new PiecewiseCurve()
                .Add(EasingCurves.Exp, EasingType.Out, 1, 0.4f)
                .Add(EasingCurves.Sine, EasingType.Out, 1, 1f);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Rot);

            float BackRot = -MathHelper.ToRadians(110) * Math.Clamp(MathF.Sin(Time/10.1f), 0.5f, 1);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, BackRot);
            
            RotationOffset = GunSwingCurveOutput * -MathHelper.ToRadians(360+130);
            Projectile.rotation = RotationOffset;
            //Main.NewText($"{Projectile.rotation}, offset = {RotationOffset}, gunswingcurve: {GunSwingCurveOutput}");
            t2 = Math.Clamp(t2 + 0.004f, 0, 1);
            
            if(t2 == 1)
            {
                Time = 0;
                t2 = 0;
                CurrentState = State.Idle;
                riflePlayer.ShotCount = 7;
            }
        }
    }
}
