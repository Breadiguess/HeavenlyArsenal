using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Buffs;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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
            Item.maxStack = 999;
            Item.value = Item.buyPrice(0, 43, 10, 0);
            Item.rare = ItemRarityID.Quest;
            Item.autoReuse = true;
            Item.buffTime = 60;
            Item.buffType = ModContent.BuffType<CombatStimBuff>();
            Item.UseSound = SoundID.DoubleJump;
        }
        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(ModContent.BuffType<CombatStimBuff>(), (int)(Math.Abs(player.GetModPlayer<StimPlayer>().stimsUsed-160) * 10), true, false);

            
            player.statLife -= 50;
            if (Main.myPlayer == player.whoAmI)
            {
                player.HealEffect(-50, true);
                GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 3f, 10);
                GeneralScreenEffectSystem.RadialBlur.Start(player.Center, 1, 60);
                player.GetModPlayer<StimPlayer>().UseStim();
            }
            if (player.statLife <= 0)
            {
               player.KillMe(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AstralInjection" + Main.rand.Next(1, 2 + 1)).Format(player.name)), 1000.0, 0, false);
            }
        }

        public override void UseAnimation(Player player)
        {
            player.itemLocation = new Vector2(player.Center.X-40, player.Center.Y+10);
           player.itemRotation = MathHelper.ToRadians(45f*player.direction);
            player.itemWidth = 14;
           
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<YharonSoulFragment>(3)
                .AddIngredient(ItemID.BottledWater)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
    
    
}
