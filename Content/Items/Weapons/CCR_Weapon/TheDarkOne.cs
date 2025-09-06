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
        Pullout,
        Idle,
        Charge,
        Exhume,
        Putaway
    }
    public class TheDarkOne : ModProjectile
    {
        #region setup  

        private Vector2 BowTop;
        private Vector2 BowMiddle;
        private Vector2 BowBottom;

        public ref float Time => ref Projectile.ai[0];
        public ref float Charge => ref Projectile.ai[1];


        private int ChargeCap = 5;
        private DarkOneState CurrentState = DarkOneState.Pullout;

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
            Projectile.scale = 0;
            BowTop = Projectile.Center + new Vector2(0 + Projectile.velocity.X, 300).RotatedBy(Projectile.rotation);
            BowBottom = Projectile.Center + new Vector2(0, -30).RotatedBy(Projectile.rotation);
            BowMiddle = (BowTop + BowBottom) / 2 - new Vector2(10, -10 * Projectile.direction).RotatedBy(Projectile.rotation) * 0;
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

          
            Projectile.Center =  Owner.MountedCenter;

            StateMachine();
            
            Time++;

            BowTop = Projectile.Center + new Vector2(15, -60).RotatedBy(Projectile.rotation);
            BowBottom = Projectile.Center + new Vector2(15, 60).RotatedBy(Projectile.rotation);
            BowMiddle = (BowTop + BowBottom) / 2 - new Vector2(10, -10).RotatedBy(Projectile.rotation) * 0;
        }

        private void StateMachine()
        {
           switch (CurrentState)
            {
                case DarkOneState.Pullout:
                    HandlePullout();
                    break;
                case DarkOneState.Idle:
                    HandleIdle();
                    break;
                case DarkOneState.Charge:
                    HandleCharge();
                    break;
                case DarkOneState.Exhume:
                    HandleExhume();
                    break;
                case DarkOneState.Putaway:
                    HandlePutaway();
                    break;
            }
        }

       
        /// <summary>
        /// When the projectile is first created, it will start small, and then rapidly increase in size until its at normal size.
        /// </summary>
        private void HandlePullout()
        {

            Projectile.scale += 0.125f;//(float)Math.Round( MathHelper.Lerp(Projectile.scale, 1, 0.42f));
            if (Projectile.scale >= 1 && !Owner.controlUseItem)
                CurrentState = DarkOneState.Idle;
            else if (Projectile.scale >= 1 && Owner.controlUseItem)
                CurrentState = DarkOneState.Charge;
        }
        private void HandleIdle()
        {
            if (Owner.controlUseItem)
            {
                CurrentState = DarkOneState.Charge;
                Time = 0;
            }
            if (Charge > 0)
                Charge--;

            Projectile.rotation = Projectile.rotation.AngleLerp(new Vector2(10*Owner.direction,10).ToRotation(), 0.35f);
        }
        private void HandleCharge()
        {
            //todo: lerp projectile rotation to face 90 degrees up, with partial rotation towards the mouse
            float MouseX = Utils.AngleTo(Projectile.Center, Main.MouseWorld);// + MathHelper.PiOver2;//MathHelper.SmoothStep(Owner.Center.X - Main.MouseWorld.X, -5,5);
            MouseX = MouseX.AngleLerp(-MathHelper.PiOver2, 0.5f);
            Projectile.rotation = Projectile.rotation.AngleLerp(MouseX , 0.1f);
            if(Time % 40 == 0 &&  Owner.controlUseItem && Charge < 5) // ChargeCap)
            {
                Charge++;
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = Charge/5 });
            }


            if (!Owner.controlUseItem && Charge <= 0)
                CurrentState = DarkOneState.Idle;
            if (!Owner.controlUseItem && Charge >= 1)
            {
                CurrentState = DarkOneState.Exhume; 
                Time = 0;
            }
        }
        private void HandleExhume()
        {
            float MouseX = Utils.AngleTo(Projectile.Center, Main.MouseWorld);// + MathHelper.PiOver2;//MathHelper.SmoothStep(Owner.Center.X - Main.MouseWorld.X, -5,5);
            MouseX = MouseX.AngleLerp(-MathHelper.PiOver2, 0.5f);
            Projectile.rotation = Projectile.rotation.AngleLerp(MouseX, 0.1f);
            int CrystalAmount = Charge > 4 ? (int)Charge/2  : (int)Charge;
            float spawnHeight = 600f;
            float horizSpread = 200f;
            int delayPerCrystal = 3;  // ticks between each

            // center X of spawn is player.Center.X, not adding MouseWorld.X twice
            for (int i = 0; i < CrystalAmount; i++)
            {
                // random X offset
                float offsetX = Main.rand.NextFloat(-horizSpread, horizSpread);
                Vector2 spawnPos = new Vector2(Main.MouseWorld.X + offsetX, (Main.MouseWorld.Y + Owner.Center.Y) / 2 - spawnHeight);


                Vector2 aimDir = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.UnitY);
                aimDir = Vector2.Lerp(Vector2.UnitY, aimDir, 0.5f);
                Vector2 projVel = aimDir * Owner.HeldItem.shootSpeed;


                float startDelay = i * delayPerCrystal;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                projVel,
                    ModContent.ProjectileType<EntropicCrystal>(),
                    Owner.HeldItem.damage,
                    Owner.HeldItem.knockBack,
                    Owner.whoAmI,
                    ai0: startDelay,
                    ai1: 0f
                );

            }
            if (Time > 30 * Charge)
                CurrentState = DarkOneState.Idle;
            
        }

        public void HandlePutaway()
        {

        }



        #endregion


        public override bool? CanDamage() => false;

        private void drawVoid()
        {

        }
        private void drawString(ref Color Thing, Vector2 StartPos, Vector2 EndPos)
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/TheDarkOne").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;// + new Vector2(Projectile.width / 2, Projectile.height / 2);
            SpriteEffects effects = Owner.direction == -1 ? SpriteEffects.FlipVertically: SpriteEffects.None;
            Vector2 origin = new Vector2(texture.Width/8, texture.Height / 2);
            float chargeOffset = Charge * Projectile.scale * 2f;
            if (Charge > 0)
            {
                for (int i = 0; i < ChargeCap; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 3f).ToRotationVector2() * chargeOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, Color.Purple, Projectile.rotation, origin, Projectile.scale, effects, 0f);
                }
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            Utils.DrawLine(Main.spriteBatch, BowMiddle, BowTop, Color.Purple);
            Utils.DrawLine(Main.spriteBatch, BowMiddle, BowBottom,Color.Purple);


            /*
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            ManagedShader PortalShader = ShaderManager.GetShader("HeavenlyArsenal.thing");

            //PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);

            
            //PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.Spikes, 0);


            //PortalShader.Apply();
            
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            */


            //Utils.DrawBorderString(Main.spriteBatch, "| State: " + CurrentState.ToString() + " | Charge: " + Charge.ToString() + " | Scale: " + Projectile.scale + ", " + Projectile.rotation, drawPosition + Vector2.UnitY * -100, Color.AntiqueWhite);
            //Utils.DrawBorderString(Main.spriteBatch, "| Time: "+ Time.ToString(), drawPosition + Vector2.UnitY * -80, Color.AntiqueWhite);
            return false;
        }
    }
}
