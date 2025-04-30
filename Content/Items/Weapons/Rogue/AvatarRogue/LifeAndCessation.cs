using CalamityMod;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.AvatarRogue
{
    class LifeAndCessation : ModItem
    {


        public override void SetStaticDefaults()
        {

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
            Item.shootSpeed = 0.1f; // The “held projectile” doesn’t really move. lmao.
            Item.autoReuse = true;

            // Sound/consumable details
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
                    //spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
                }
            }
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<HeldLifeCessationProjectile>()] <= 0;

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
        public bool IsFreezing;
        public bool IsBoilingAlive;

        public float HeatAmmount;
        public override bool InstancePerEntity => true;
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
            Utils.DrawBorderString(Main.spriteBatch, "| Is Freezing " + IsFreezing.ToString(), npc.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| Is Boiling Alive " + IsBoilingAlive.ToString(), npc.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);


            if (IsBoilingAlive)
            {
                Texture2D texture = AssetDirectory.Textures.BigGlowball.Value;
                spriteBatch.Draw(texture, npc.Center - screenPos, null, Color.Red, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            if (IsFreezing)
            {
                Texture2D texture = AssetDirectory.Textures.BigGlowball.Value;
                spriteBatch.Draw(texture, npc.Center - screenPos, null, Color.White, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override bool PreAI(NPC npc)
        {



            return base.PreAI(npc);
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if(IsFreezing)
            {
                npc.lifeRegen -= 10;
                damage = 0;
            }
            base.UpdateLifeRegen(npc, ref damage);
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitByProjectile(npc, projectile, ref modifiers);
        }
    }
}

