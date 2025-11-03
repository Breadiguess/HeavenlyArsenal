using CalamityMod;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.AvatarRogue
{
    public class LifeAndCessation : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;


            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.damage = 3002;
            Item.crit = -30;
            Item.knockBack = 2f;
            Item.useTime = 5;
            Item.useAnimation = 5;

            // Important for channeling (charging)
            Item.channel = true;
            Item.useTurn = true;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<AvatarRogueHeld>();
            Item.shootSpeed = 1;
            Item.autoReuse = true;

            
            Item.UseSound = SoundID.Item1;
            Item.consumable = false;


        }

        private bool HoldingBowl(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!HoldingBowl(player))
                {
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                
                }
            }
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<AvatarRogueHeld>()] <= 0;

        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

            //float animProgress = Math.Abs(player.itemTime / (float)player.itemTimeMax);
            //float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            //if (animProgress < 0.7f)
            //    rotation += -0.45f * (float)Math.Pow((0.4f - animProgress) / 0.4f, 2) * player.direction;
            //Main.NewText($"AnimProg: {animProgress}, rotation: {rotation}");
            //player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            int style = 1;
            Texture2D bar = AssetDirectory.Textures.Bars.Bar[style].Value;
            Texture2D barCharge = AssetDirectory.Textures.Bars.BarFill[style].Value;


            Rectangle chargeFrame = new Rectangle(0, 0, (int)(barCharge.Width * Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat), barCharge.Height);
            Color barColor = Color.Lerp(Color.MediumOrchid, Color.Turquoise, Utils.GetLerpValue(0.3f, 0.8f, Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, true));
            barColor.A = 128;
            spriteBatch.Draw(bar, position + new Vector2(0, 35) * scale, bar.Frame(), Color.DarkSlateBlue, 0, bar.Size() * 0.5f, scale * 1.2f, 0, 0);
            spriteBatch.Draw(barCharge, position + new Vector2(0, 35) * scale, chargeFrame, barColor, 0, barCharge.Size() * 0.5f, scale * 1.2f, 0, 0);
        }


    }

    public class AvatarRogueHeld : ModProjectile
    {
        public enum AvatarRogueHeldState
        {
            Spawn,
            Idle,
            //todo: basic fire
            PreWhip,
            Whip
            
        }

        public float SwingProgress;
        public int SwingStage;
        public AvatarRogueHeldState CurrentState = AvatarRogueHeldState.Spawn;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
       
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.width = Projectile.height = 6;
            Projectile.timeLeft = 2;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
        }

        public override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;

            if (Owner.HeldItem.type != ModContent.ItemType<LifeAndCessation>() || Owner.CCed || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = Owner.Center + new Vector2(10 * Owner.direction, 0);
            Projectile.timeLeft = 2;
            StateMachine();
            ManagePlayer();
            Time++;
            
        
        }
        private void StateMachine()
        {
            switch (CurrentState)
            {
                case AvatarRogueHeldState.Spawn:
                    Projectile.rotation = 0;//(Owner.Center - Owner.Calamity().mouseWorld).ToRotation() + MathHelper.PiOver2;
                    CurrentState = AvatarRogueHeldState.Idle;
                    break;
                case AvatarRogueHeldState.Idle:
                    HandleIdleState();
                    break;
                case AvatarRogueHeldState.PreWhip:
                    HandlePreWhip();
                    break;
                case AvatarRogueHeldState.Whip:
                    HandleWhipState();
                    break;
            }
        
        }
        private void HandleIdleState()
        {
            SwingProgress = 0;
            Vector2 ToMouse = Main.MouseWorld - Owner.Center;
            
            Vector2 SpawnOffset = Projectile.Center + ToMouse;
            if (Owner.controlUseItem)
            {
                Time = 0;
                CurrentState = AvatarRogueHeldState.PreWhip; 
            }
        }
       
        private void HandlePreWhip()
        {
            Vector2 TargetLocation = Owner.Center + Vector2.UnitX * 100;
            if (Time== 1)
            {
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), TargetLocation, Vector2.Zero, ModContent.ProjectileType<LifeCessationStealthStrike>(), 10, 0, default, default, default, 2);
            }
            //Owner.SetDummyItemTime(2);
            Vector2 handOffset = new Vector2(10 * Owner.direction, 6 * Owner.gravDir); // Offset from player center to hand
                                                                                       // Rotate the offset by the projectile's rotation
            Vector2 rotatedOffset = handOffset.RotatedBy(Projectile.rotation);
            // Set the projectile's center to the hand position
            Projectile.Center = Owner.Center + rotatedOffset;
            Projectile.rotation = float.Lerp(Projectile.rotation, MathHelper.ToRadians(Owner.direction * -120)  + MathHelper.ToRadians(Owner.direction * 180) * SwingProgress,0.2f);
            if (Time > 30)
            {
                Time = 0;
                CurrentState = AvatarRogueHeldState.Whip;
            }


        }
        private void HandleWhipState()
        {
            //Projectile.rotation = (Owner.Center - Owner.Calamity().mouseWorld).ToRotation() + MathHelper.PiOver2;
            
            Vector2 ToMouse = Main.MouseWorld - Owner.Center;

            float thing = ToMouse.Length();
            Vector2 TargetLocation = Projectile.Center + Projectile.rotation.ToRotationVector2() * thing;
            Dust.NewDustPerfect(TargetLocation, DustID.Cloud, Vector2.Zero, 100);
            

            //todo: set this once and then not again
            float InitialMouse = 0;
            if (Time == 1)
            {
                
                InitialMouse = ToMouse.ToRotation();
            }
                
            
            switch (SwingStage)
            {
                case 0:

                    Projectile.rotation = MathHelper.ToRadians(Owner.direction*-120) + InitialMouse + MathHelper.ToRadians(Owner.direction*180)* SwingProgress;
                    
                    //Projectile.velocity = Projectile.rotation.ToRotationVector2()*20;
                    //todo: set position of the projectile to the hand. the hand is rotated by projectile.
                    Vector2 handOffset = new Vector2(10 * Owner.direction, 6 * Owner.gravDir); // Offset from player center to hand
                    // Rotate the offset by the projectile's rotation
                    Vector2 rotatedOffset = handOffset.RotatedBy(Projectile.rotation);
                    // Set the projectile's center to the hand position
                    Projectile.Center = Owner.Center + rotatedOffset;
                    SwingProgress = float.Lerp(SwingProgress, 1, 0.2f);
                    if (SwingProgress >= 0.99 && Time > 40)
                        CurrentState = AvatarRogueHeldState.Idle;


                    break;
                case 1:
                    break;
            }
        }
        private void ManagePlayer()
        {
            //Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;


            float frontArmRotation = Projectile.rotation - MathHelper.PiOver2;

            //frontArmRotation = MathHelper.PiOver2;// - frontArmRotation;
            //frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);// Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        }
        public override bool? CanCutTiles() => false;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = new Vector2(texture.Width / 2, texture.Height / 2);

            float Rot = Projectile.rotation + MathHelper.PiOver2;

            //Utils.DrawBorderString(Main.spriteBatch, "SwingStage: " + SwingStage.ToString() + ", SwingProgress: " + SwingProgress.ToString(), DrawPos - Vector2.UnitY * 100, Color.AntiqueWhite);
            //Utils.DrawBorderString(Main.spriteBatch, "State: " + CurrentState.ToString() + ", Rotation: " + MathHelper.ToDegrees(Projectile.rotation), DrawPos - Vector2.UnitY * 120, Color.AntiqueWhite);
            //Utils.DrawBorderString(Main.spriteBatch, "Time: " + Time.ToString(), DrawPos - Vector2.UnitY * 80, lightColor);
            Main.EntitySpriteDraw(texture, DrawPos, null, lightColor, Rot, Origin, 0.4f, SpriteEffects.None, 0);
            return false;
        }
    }
}

