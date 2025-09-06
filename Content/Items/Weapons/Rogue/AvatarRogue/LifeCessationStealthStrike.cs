using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Physics.VerletIntergration;
using NoxusBoss.Core.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.AvatarRogue
{
    public class LifeCessationStealthStrike : ModProjectile, IDrawSubtractive
    {
        public Rope Chain;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref float Time => ref Projectile.ai[0];

        //todo: make this sync with the held projectile so its easier on me. also make it line up with the actual CurrentState.
        public ref float state => ref Projectile.ai[2];
        public Projectile ParentProj;
        public ref Player Owner => ref Main.player[Projectile.owner];
        private int count;
        public enum StrikeState
        {
            Startup,
            Flail
        }

        public VerletSimulatedRope FlailChain
        {
            get;
            set;
        }


        public StrikeState CurrentState = StrikeState.Startup;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = false;
            
        }
        public override void SetDefaults()
        {
            Projectile.damage = 40;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle= 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            count++;
            if (Projectile.ai[2] == 2)
            {
                CurrentState = StrikeState.Startup;
            }
            if(ParentProj == null)
            {
                ParentProj = Main.projectile[Owner.heldProj];
            }
            if (Chain == null)
            {
                Chain = new Rope(Projectile.Center, ParentProj.Center, 20, 10f, Vector2.Zero);
            }

            if(FlailChain == null)
            {
                
            }
        }
        public override void AI()
        {
            //todo: if HeldProj exists and the Projectile is not in Flail, kill

            Projectile.timeLeft = 200;
            StateMachine();
            
            HandleChain();
            Time++;
            
        }
        private void StateMachine()
        {
            switch (CurrentState)
            {
                case(StrikeState.Startup):
                   HandleStartup();
                    break;
                case(StrikeState.Flail):
                    HandleFlail();
                    break;
            }
        }
        private void HandleStartup()
        {
            //todo: remember how to get the specific instance of the projectile so that you can access its internal values. this will hold an offset.
            //Main.projectile[ParentProj.identity];
            //for now, lets just manually set it.
            if (Main.projectile[ParentProj.identity].ModProjectile is AvatarRogueHeld AvatarRogueHeld)
            {
                
            }
            
            Vector2 ToMouse = Main.MouseWorld - Owner.Center;
            float thing = ToMouse.Length();
            Vector2 TargetLocation = Projectile.Center = Owner.Center + Vector2.UnitX * 100;


            Projectile.rotation = MathHelper.ToRadians(Time * 12);
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 12;
            
        }
        private void HandleFlail()
        {
            Vector2 ToMouse = Main.MouseWorld - Owner.Center;
            float thing = ToMouse.Length();
            Vector2 TargetLocation = ParentProj.Center + ParentProj.rotation.ToRotationVector2() * thing;

           
            Vector2 TargetPoint = ToMouse;

            Projectile.Center = Vector2.Lerp(Projectile.Center, ToMouse, 0.3f).SafeNormalize(Vector2.UnitY);

           
            

        }
        private void HandleChain()
        {
            FlailChain ??= new VerletSimulatedRope(ParentProj.Center, Vector2.Zero, 50, 100);
            
            FlailChain.Update(Projectile.Center, 2f);
            
            //FlailChain.Update(ParentProj.Center, 2f);
            
            
            
        }
        public override void OnKill(int timeLeft)
        {
            Main.NewText("I should die");
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Burning, 400, true);
            // KMS target.
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 40;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }



        //todo: make actually cool, make actually do something
        //actual functionality of the stealth strike:
        /*
         
                  

        so atm i have a few idea
        some kind of burst where one half of it freezes and the other half burns at 2 trillion kelvin
        some kind of sphere that swaps between freezing and burning
        a huge burst like the ones on the sun or the roche limit
        and then discount rainbow gun

        ink had an idea of it being a big flower that you spawn and then you can swing it around like a flail
         */
        public float RopeWidthFunction(float completionRatio)
        {
            float widthInterpolant = InverseLerp(0f, 0.16f, completionRatio, true) * InverseLerp(1f, 0.84f, completionRatio, true);
            widthInterpolant = MathF.Pow(widthInterpolant, 8f);
            float baseWidth = MathHelper.Lerp(120f, 124f, widthInterpolant);
            float pulseWidth = MathHelper.Lerp(0f, 150f, MathF.Pow(MathF.Sin(Main.GlobalTimeWrappedHourly * -5.6f + Projectile.whoAmI * 1.3f + completionRatio * 1.4f), 22f));
            return (baseWidth + pulseWidth) * 0.03f;
        }

        private void DrawChain()
        {
            if (FlailChain != null)
                FlailChain.DrawProjectionScuffed(GennedAssets.Textures.GreyscaleTextures.WhitePixel, Vector2.UnitY * 20f - Main.screenPosition, Projectile.identity % 2 == 0, _ => Color.DarkRed * Projectile.Opacity, RopeWidthFunction, lengthStretch: 0.707f);

        }
        public override bool PreDraw(ref Color lightColor)
        {

            DrawChain();
            Texture2D texture = GennedAssets.Textures.LoreItems.LoreAvatar;
            Rectangle silly = texture.Frame(1, 1, 0, 0);
            SpriteEffects None = SpriteEffects.None;
            float rot = (float)(Main.GlobalTimeWrappedHourly * 10.1f);
            Vector2 origin = new Vector2(texture.Width/2,texture.Height/2);
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            //Projectile.rotation = MathHelper.ToRadians(Time * 12);
            //Projectile.velocity = 
            Vector2 Offset = Vector2.Zero;
            for (int i = 0; i < 32; i++)
            {
                Offset = new Vector2((float)Math.Sin(i + Main.GlobalTimeWrappedHourly) * 10, (float)Math.Cos(i + Main.GlobalTimeWrappedHourly) * 10 );
                Main.EntitySpriteDraw(texture, DrawPos + Offset, silly, Color.AntiqueWhite, rot, origin, Projectile.scale, None, 0);
            }
           



            Utils.DrawBorderString(Main.spriteBatch, "Time: " + Time.ToString(), DrawPos - Vector2.UnitY*-100, lightColor );
            return false;
        }
        public void DrawSubtractive(SpriteBatch spriteBatch)
        {
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall, drawPosition, null, Color.White * Projectile.Opacity * Saturate(Projectile.scale) * 0.2f, 0f, GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall.Size() * 0.5f, Projectile.scale * 0.75f, 0, 0f);
            spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall, drawPosition, null, Color.White * Projectile.Opacity * Saturate(Projectile.scale) * 0.24f, 0f, GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall.Size() * 0.5f, Projectile.scale * 0.75f, 0, 0f);
        }

        public override bool? CanDamage()
        {
            return base.CanDamage();
        }
    }
    
}