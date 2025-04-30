using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Buffs.Stims;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Consumables
{
    class CombatStim : ModItem
    {
        public override void SetStaticDefaults()
        {
           // DisplayName.SetDefault("Combat Stim");
            //Tooltip.SetDefault("A powerful combat stimulant that enhances your abilities.");
        }
        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(0, 43, 10, 0);
            Item.rare = ItemRarityID.Quest;
            Item.autoReuse = true;
            Item.buffTime = 60;
            Item.buffType = ModContent.BuffType<CombatStimBuff>();
            Item.UseSound = SoundID.DoubleJump;
        }
        //public override void 
        public override void OnConsumeItem(Player player)
        {
            
            
            
            if (Main.myPlayer == player.whoAmI)
            {
                if (player.GetModPlayer<StimPlayer>().Addicted)
                {
                    player.HealEffect(-150, true);
                    player.statLife -= 150;

                }
                else 
                {
                    player.HealEffect(-50, true);
                    player.statLife -= 50;
                }
                GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 3f, 10);
                GeneralScreenEffectSystem.RadialBlur.Start(player.Center, 1, 60);
                //player.GetModPlayer<StimPlayer>().UseStim();
            }
            if (player.statLife <= 0)
            {


                if (player.GetModPlayer<StimPlayer>().Addicted)
                {
                    string deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStimAddicted" + Main.rand.Next(1, 5 + 1), player.name);
                    player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0, false);
                }
                else if (player.GetModPlayer<StimPlayer>().Withdrawl)
                {
                    string deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 3 + 1), player.name);
                    player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0, false);
                }
                else
                {
                    string deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 4 + 1), player.name);
                    player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0, false);
                }



            }
            player.AddBuff(ModContent.BuffType<CombatStimBuff>(), (int)(Math.Abs(player.GetModPlayer<StimPlayer>().stimsUsed - 160) * 10), true, false);

        }

        public override void UseAnimation(Player player)
        {
           player.itemLocation = new Vector2(player.Center.X-40, player.Center.Y+30);
           player.itemRotation = MathHelper.ToRadians(45f*player.direction);
           player.itemWidth = 14;
            
            
           
        }



        public override void AddRecipes()
        {
            CreateRecipe(20)
                .AddIngredient<YharonSoulFragment>(3)
                .AddIngredient<AstralInjection>(6)
               // .AddIngredient(ItemID.BottledWater)
                .AddIngredient<BloodOrb>(20)
                .AddIngredient<BloodSample>(10)
                .AddIngredient<Bloodstone>(5)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
    
    
}
