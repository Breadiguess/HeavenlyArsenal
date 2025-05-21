using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.AvatarRogue
{
    class LifeAndCessation : ModItem
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
            Item.shoot = ModContent.ProjectileType<HeldLifeCessationProjectile>();
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
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<HeldLifeCessationProjectile>()] <= 0;

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

    class CessationPlayer : ModPlayer
    {
        public float CessationHeat;
        public int CessationHeatTimer;
        public override void ResetEffects()
        {
            CessationHeat = 0;
            CessationHeatTimer = 0;
        }
        public override void UpdateDead()
        {
            CessationHeat = 0;
            CessationHeatTimer = 0;
        }
    }

    class LifeAndCessationGlobalNPC : GlobalNPC
    {
        //so why is this here? as in, why use a global npc?
        //i would say this global NPC is here to allow me more freedom to apply any effects i want to happen after an NPC gets hit by life and cessation.
        //
        public bool IsFreezingToDeath;
        public bool IsBoilingAlive;

        public float HeatAmmount;
        public int warmupTimer;

        public float GoldilocksMin = 150;
        public float GoldilocksZone = 200;
        public float GoldilocksMax = 250;
        public override bool InstancePerEntity => true;
        public override bool PreAI(NPC npc)
        {
            //todo: rewrite all of this so that its not shit
            /*
            //todo: if not within min or max of golidlocks, begin attemping to heat up or cool down.

            //subtract the difference of heat amount - max or min and use that to determine whether heat amount should go up or down
            //when within the goldilocks zone, heat amount should quickly equal out to the goldilocks zone.
            // Check if HeatAmmount is outside the Goldilocks range and adjust accordingly.
            if (HeatAmmount < GoldilocksMin || HeatAmmount > GoldilocksMax)
            {
                // Increment or decrement HeatAmmount based on its position relative to the Goldilocks range.
                if (HeatAmmount < GoldilocksMin)
                {
                    if (warmupTimer >= 40)
                        HeatAmmount += 1; // Heat up if below the minimum.
                }
                else if (HeatAmmount > GoldilocksMax)
                {
                   if(warmupTimer >= 40)
                     HeatAmmount -= 1; // Cool down if above the maximum.
                }


                if (HeatAmmount > GoldilocksMin && HeatAmmount < GoldilocksMax)
                {
                    IsFreezingToDeath = false;
                    IsBoilingAlive = false;
                }
                    


                // Use a timer to control the rate of adjustment.
                if (warmupTimer >= 40)
                {
                    warmupTimer = 0;
                }
                
            }
            else 
            {
                if(warmupTimer >= 0)
                    HeatAmmount = MathHelper.Lerp(HeatAmmount, GoldilocksZone, 0.1f);
            }
            
            warmupTimer++;
            if(warmupTimer< -10)
            {
                warmupTimer = 0;
            }
            if (HeatAmmount < GoldilocksMin)
            {
                IsFreezingToDeath = true;
            }
            if(HeatAmmount > GoldilocksMax)
            {               
                 IsBoilingAlive = true;
            }

            if(IsBoilingAlive && IsFreezingToDeath)
            {
                IsBoilingAlive = false;
                IsFreezingToDeath = false;

                npc.takenDamageMultiplier = 4f;
            }

            if (IsBoilingAlive)
            {
                npc.AddBuff(ModContent.BuffType<Dragonfire>(), 60);
                npc.AddBuff(ModContent.BuffType<CalamityMod.Buffs.DamageOverTime.BrimstoneFlames>(), 60);
            }

            */
            return base.PreAI(npc);
        }


        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (IsFreezingToDeath)
            {
                npc.lifeRegen -= 10;
                npc.coldDamage = true;

            }

            if (IsBoilingAlive)
            {

            }
            base.UpdateLifeRegen(npc, ref damage);
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            HeatAmmount = 200;
            base.OnSpawn(npc, source);
        }

        



        public override void SetDefaults(NPC npc)
        {
           
            /*
            if (npc.type == NPCID.TargetDummy)
            {
                npc.damage = 0;
                npc.defense = 0;
                npc.lifeMax = 1;
                npc.value = 0;
            }
            */
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
           /*

            if (IsBoilingAlive)
            {
                Texture2D texture = AssetDirectory.Textures.BigGlowball.Value;
                spriteBatch.Draw(texture, npc.Center - screenPos, null, Color.Red, 0f, texture.Size() * 0.5f, 0.25f, SpriteEffects.None, 0f);
            }
            if (IsFreezingToDeath)
            {
                Texture2D texture = AssetDirectory.Textures.BigGlowball.Value;
                spriteBatch.Draw(texture, npc.Center - screenPos, null, Color.White, 0f, texture.Size() * 0.5f, 0.25f, SpriteEffects.None, 0f);
            }

            Utils.DrawBorderString(Main.spriteBatch, "| Is Freezing: " + IsFreezingToDeath.ToString(), npc.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| Is Boiling Alive: " + IsBoilingAlive.ToString(), npc.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| HeatAmmount: " + HeatAmmount.ToString() + " | Warmup Timer: " + warmupTimer.ToString(), npc.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
           */

            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }

      
     
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitByProjectile(npc, projectile, ref modifiers);
        }
    }
}

