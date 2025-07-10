using CalamityMod.Items.Weapons.Magic;
using CalamityMod;
using CalamityMod.Projectiles.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HeavenlyArsenal.Content.Items.Misc;
using Terraria.GameContent;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;
using static HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear.AvatarLonginusHeld;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using Luminance.Common.Utilities;
using Terraria.Audio;
using NoxusBoss.Assets;
using Luminance.Core.Graphics;
using Humanizer;
using Terraria.ID;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Buffs.Stims;

namespace HeavenlyArsenal.Content.Projectiles.Misc
{
    class ChaliceOfFunHoldout :ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Rancor>();
        public Player Owner => Main.player[Projectile.owner];

        public bool InUse => Owner.controlUseItem && Owner.altFunctionUse == 0;
        public ref float Time => ref Projectile.ai[0];

        public ref float drinkProgress => ref Projectile.ai[1];

        public bool isDraining;
        public override void SetDefaults()
        {
      
            Projectile.width = Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.timeLeft = 90000;
            Projectile.aiStyle = 0;
            Projectile.noEnchantmentVisuals = true;
            Projectile.manualDirectionChange = true;
        }
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            
        }
        public override void AI()
        {
            AdjustPlayerHoldValues();
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            
            
            Projectile.rotation = Projectile.DirectionTo(armPosition).ToRotation();

            if (Owner.HeldItem?.type != ModContent.ItemType<ChaliceOfFun>())
            {
                Projectile.Kill();
                return;
            }


            Lighting.AddLight(Projectile.Center, Vector3.One);
            Time++;

            /*
            // Handle frames.
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
                Projectile.frameCounter = 0;
            }
            */
            
            //drinkProgress++;
            if (Owner.altFunctionUse == 1)
            {
              
            }
            else if (InUse)
            {
                Drink(Owner);
            }
            if(!InUse)
            {
                if (drinkProgress > 0)
                {
                 
                    drinkProgress-= 5;
                    //Main.NewText($"{drinkProgress}", Color.AntiqueWhite);
                }
                else
                    drinkProgress = 0;
            }

            Projectile.Center = armPosition;// + Vector2.UnitX * Owner.direction * 8f;

        }

        public void LetsGoGambling(int random = 0)
        {
            if(random == -1)
            {
                //do random numbers if random is unassigned
            }
        }
        public void Drink(Player player)
        {
            if (drinkProgress < 90)
            {


                drinkProgress++;
               // Main.NewText($"{drinkProgress}", Color.AntiqueWhite);
            }
            else
            {
                player.GetModPlayer<StimPlayer>().Addicted = false;
                player.GetModPlayer<StimPlayer>().Withdrawl = false;
                player.GetModPlayer<StimPlayer>().stimsUsed = 0;
                player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());
                Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { MaxInstances = 0, PitchVariance = 1f});
                player.AddBuff(ModContent.BuffType<BloodflareBloodFrenzy>(),1200,true,false);
                player.AddBuff(ModContent.BuffType<BrutalCarnage>(), 1200, true, false);
                player.AddBuff(ModContent.BuffType<DivineBless>(), 1200, true, false);
                // rip kami player.AddBuff(ModContent.BuffType<KamiBuff>(), 1200, true, false);
                player.AddBuff(ModContent.BuffType<TarraLifeRegen>(), 1200, true, false);
                player.AddBuff(ModContent.BuffType<TarragonCloak>(), 1200, true, false);
                player.AddBuff(ModContent.BuffType<TarragonImmunity>(), 1200, true, false);
                Projectile.rotation = Projectile.DirectionTo(armPosition).ToRotation();

                Vector2 dustLocation = new Vector2 (Projectile.Center.X, Projectile.Center.Y-20);

                for (int i =0; i < 6; i++)
                {
                    Dust.NewDust(dustLocation, 5, 5, 324, (1 + Main.rand.NextFloat(0, 5)) * -player.direction, -1 +Main.rand.NextFloat(0,5), 1, default, 1);
                    
                }
                //Main.NewText($"Dust: {dustLocation}", Color.AntiqueWhite);
                //drained();
            }
        }
        public void drained()
        {
            drinkProgress = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;
            Texture2D Juice = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice").Value;

            float scale = 0.75f;
            Vector2 offset = new Vector2(0, 0);
                
            Vector2 origin = new Vector2(texture.Width / 2 + 20 *-Owner.direction, texture.Height - 1.6f * texture.Height / 4);
            Vector2 Gorigin = new Vector2(glow.Width / 2 + 125 * -Owner.direction, glow.Height - 1.6f * glow.Height / 4);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float rotation = MathHelper.ToRadians( (drinkProgress-15 )*-Owner.direction); //drinkProgress;

            SpriteEffects direction = SpriteEffects.None;
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, scale, direction, 0f);

            Vector2 glowPosition = (Projectile.Center- origin) - Main.screenPosition;
            Main.spriteBatch.Draw(glow, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, Gorigin, 0.1f, direction, 0f);

            

            Main.spriteBatch.Draw(Juice, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, scale, direction, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            

            return false;
        }

        public void AdjustPlayerHoldValues()
        {
            Projectile.spriteDirection = Owner.direction;
            Projectile.timeLeft = 2;
            Owner.heldProj = Projectile.whoAmI;
            //Owner.itemTime = 2;
            //Owner.itemAnimation = 2;
            Owner.itemRotation = 0f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians( (65 + drinkProgress )* -Owner.direction));
        }

        public override bool? CanDamage() => false;
    }
}

