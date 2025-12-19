
using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    partial class Aoe_Rifle_HeldProj : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];

        public Aoe_Rifle_Player RiflePlayer => Owner.GetModPlayer<Aoe_Rifle_Player>();

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
        public struct Clip
        {
            public List<Item> Bullets = new List<Item>(MAX_CLIP_SIZE);
            public int BulletCount
            {
                get => Bullets != null ? Bullets.Count : 0;
            }

            public int bulletIndex;
            public Item NextBullet
            {
                get => Bullets[bulletIndex] != null ? Bullets[bulletIndex] : null;
            }


            public Clip(List<Item> insertedBullets)
            {
                this.Bullets = insertedBullets;
            }
        }

        public static readonly int MaxClips = 2;
        public List<Clip> clips = new List<Clip>(MaxClips);

        public int AmmoStored
        {
            get => RiflePlayer.BulletCount;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 19;
        }
        public override void SetDefaults()
        {
            BuildClips();
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.frame = 0;
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

        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Rectangle Frame = tex.Frame(1, 19, 0, Projectile.frame);


            Vector2 DrawPos = Projectile.Center - Main.screenPosition + new Vector2(0, 10);
            Vector2 Origin = new Vector2(50, Frame.Height / 2);

            SpriteEffects flip = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(tex, DrawPos, Frame, Color.White, Projectile.rotation + RotationOffset, Origin, 1f, flip);

            string msg = "";


            if (clips != null)
            {
                for (int x = 0; x < clips.Count; x++)
                {
                    if (clips[x].Bullets != null)
                        for (int i = 0; i < clips[x].Bullets.Count; i++)
                        {
                            var clip = clips[x].Bullets[i];
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
                                Main.EntitySpriteDraw(bullet, DrawPos + new Vector2(10 * i, x * 10 -200), null, Color.White, 0, Origin, 0.5f, 0);
                            }

                        }
                }
            }
            Utils.DrawBorderString(Main.spriteBatch, msg, DrawPos, Color.White, 1);
            return false;//base.PreDraw(ref lightColor);
        }

        public override void Load()
        {
            On_Main.CheckMonoliths += On_Main_CheckMonoliths;
        }

        private void On_Main_CheckMonoliths(On_Main.orig_CheckMonoliths orig)
        {
            throw new NotImplementedException();
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
            if (AttackStage > Pattern.Length - 1)
                AttackStage = 0;
            RifleState nextState = Pattern[AttackStage];

            AttackStage++;
            Time = -1;
            if (AmmoStored <= 0)
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
            if (Owner.controlUseItem)
            {
                CurrentState = PickNextAction();
            }
        }

        private void HandleFire()
        {
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 10, ModContent.ProjectileType<Aoe_Rifle_Laser>(), Owner.HeldItem.damage, Owner.HeldItem.knockBack);
            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundNormal, Owner.Center).WithVolumeBoost(3);
            RiflePlayer.BulletCount--;
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
            if (Time == 0)
                RotationOffset += MathHelper.ToRadians(30 * -Owner.direction);
            else
                RotationOffset = RotationOffset.AngleLerp(0, 0.2f);

            if (Time % 6 == 0 && RiflePlayer.Authority > 3)
            {

                Aoe_Rifle_DeathParticle particle = new Aoe_Rifle_DeathParticle();
                particle.Prepare(Owner.MountedCenter + new Vector2(Main.rand.NextFloat(-10, 11), 0) * 10, 0, 120, Owner, Main.rand.Next(0, Aoe_Rifle_DeathParticle.SymbolList.Length + 1));

                //ParticleEngine.BehindProjectiles.Add(particle);

            }

            if (Time > 40)
                CurrentState = PickNextAction();
        }

        private void HandleCycle()
        {
            if (Time == 0)
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.CycleSound);
            if (Time >= 40)
                CurrentState = PickNextAction();
        }

        private void HandleReload()
        {
            //TODO: clip system with animation
            // mostly cosmetic, but its all about the fantasy of the weapon, no?
            //simple at low authority, with exponentially more powerful visuals as authority increases
            //(think sigils forming into existance and burning out, cracks appearing in space around the weapon as the clip is pushed in,
            //a screaming, horrifiying power emenating that almost begs for another magazine)

            //also serves a gameplay feature to partially balance the insane power of the weapon, so you can't just spam it


            if (!Owner.HasAmmo(Owner.HeldItem))
            {
                return;
            }

            if (Time == 0)
            {
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.ReloadSound, Owner.Center).WithVolumeBoost(3);
                //Assemble clips from ammo in player inventory
                for (int x = 0; x < 2; x++)
                {
                    var clip = clips[x];
                    clip.Bullets = new List<Item>(MAX_CLIP_SIZE);

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
                            Main.NewText($"Added {clip.Bullets[i]}, {x},{i}");
                            if (Chosen.consumable)
                                Owner.ConsumeItem(Chosen.type);
                        }
                    }

                    clips[x] = clip;

                }


                RiflePlayer.BulletCount = clips[0].BulletCount + clips[1].BulletCount;
            }

            if(Time == 120)
            {
                Main.NewText("Sanity check!");
                for (int x = 0; x < 2; x++)
                {
                    var clip = clips[x];
                   
                    for (int i = 0; i < clip.Bullets.Count; i++)
                    {

                        Main.NewText(clip.Bullets[i].ToString() +", "+ i.ToString());
                    }


                }

            }
            if (Time > 120)
            {
                Time = -1;
                CurrentState = RifleState.Idle;
                AttackStage = 0;
            }

        }

        void BuildClips()
        {
            clips = new List<Clip>(MaxClips);

            for (int i = 0; i < MaxClips; i++)
            {
                clips.Add(new Clip());
            }
        }
        #endregion
    }
}
