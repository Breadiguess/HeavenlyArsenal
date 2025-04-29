using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using HeavenlyArsenal.ArsenalPlayer;
using NoxusBoss.Assets.Fonts;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Buffs.Stims
{
    class CombatStimBuff : ModBuff
    {
        internal bool notApplied = true;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }


        public override bool ReApply(Player player, int time, int buffIndex)
        {
           
                player.GetModPlayer<StimPlayer>().UseStim();
                notApplied = false;
                float addiction = player.GetModPlayer<StimPlayer>().addictionChance;
                float stimsUsed = player.GetModPlayer<StimPlayer>().stimsUsed;
               // Main.NewText($"Reapply: Addiction chance: {addiction}, stims used: {stimsUsed}", Color.AntiqueWhite);
           
            
            
            return base.ReApply(player, time, buffIndex);
        }

        private void RenderNameWithSpecialFont(On_Main.orig_MouseText_DrawBuffTooltip orig, Main self, string buffString, ref int X, ref int Y, int buffNameHeight)
        {
            orig(self, buffString, ref X, ref Y, buffNameHeight);
            if (buffString == this.GetLocalizedValue("Description"))
            {
                DynamicSpriteFont vanillaFont = FontAssets.MouseText.Value;
                Vector2 vanillaTextSize = vanillaFont.MeasureString(buffString);

                DynamicSpriteFont font = FontRegistry.Instance.AvatarPoemText;
                string text = this.GetLocalizedValue("NameText");
                Vector2 drawPosition = new Vector2(X + (int)vanillaTextSize.X + 6f, Y + 42f);
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, drawPosition, new Color(252, 37, 74), 0f, font.MeasureString(text) * Vector2.UnitY * 0.5f, Vector2.One * 0.5f, -1f, 1f);
            }
        }

       
        public override void Update(Player player, ref int buffIndex)
        {
            /* Exquisitely stuffed stats, for reference.
            player.wellFed = true;
            player.statDefense += 4;
            player.meleeCrit += 4;
            player.meleeDamage += 0.1f;
            player.meleeSpeed += 0.1f;
            player.magicCrit += 4;
            player.magicDamage += 0.1f;
            player.rangedCrit += 4;
            player.rangedDamage += 0.1f;
            player.moveSpeed += 0.4f;
            player.pickSpeed -= 0.15f;
            */
            //
            /*
            if (notApplied)
            {
                player.GetModPlayer<StimPlayer>().UseStim();
                notApplied = false;
                float addiction = player.GetModPlayer<StimPlayer>().addictionChance;
                float stimsUsed = player.GetModPlayer<StimPlayer>().stimsUsed;
                Main.NewText($"Addiction chance: {addiction}, stims used: {stimsUsed}", Color.AntiqueWhite);
            }
            */
            
            // TODO: Make this only apply if config option is turned on
            /*
            if (!GeneralScreenEffectSystem.ChromaticAberration.Active)
                GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 0.75f, 0);
            */
            player.wellFed = true;


            if (player.GetModPlayer<StimPlayer>().Addicted)
            {
                player.statDefense += 2;
                player.GetAttackSpeed<GenericDamageClass>() += 0.5f;
                player.GetDamage<GenericDamageClass>() += 0.255f;
                player.GetCritChance<GenericDamageClass>() += 2f;
                player.GetKnockback<SummonDamageClass>() += 0.5f;


                player.moveSpeed += 0.68f;
                player.pickSpeed -= 0.2f;
                player.jumpSpeedBoost += 1f;
            }
            else
            {
                player.statDefense += 5;
                player.GetAttackSpeed<GenericDamageClass>() += 1f;
                player.GetDamage<GenericDamageClass>() += 0.425f;
                player.GetCritChance<GenericDamageClass>() += 5f;
                player.GetKnockback<SummonDamageClass>() += 1f;


                player.moveSpeed += 1f;
                player.pickSpeed -= 0.2f;
                player.jumpSpeedBoost += 1f;
            }
            player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());

        }

        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {

            base.PostDraw(spriteBatch, buffIndex, drawParams);
        }


    }
}
