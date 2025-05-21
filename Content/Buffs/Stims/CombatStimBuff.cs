using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Buffs.Stims
{
    class CombatStimBuff : ModBuff
    {
        internal bool notApplied
        {
            get;
            private set;
        }
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
                time = (int)(Math.Abs(stimsUsed - 160) * 10);


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
            if (HeavenlyArsenalClientConfig.Instance != null && HeavenlyArsenalClientConfig.Instance.StimVFX)
            {
                if (!GeneralScreenEffectSystem.ChromaticAberration.Active)
                    GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, HeavenlyArsenalClientConfig.Instance.ChromaticAbberationMultiplier, 0);
            }
            if (player.GetModPlayer<StimPlayer>().Addicted)
            {
                player.statDefense += 3;
                player.GetAttackSpeed<GenericDamageClass>() += 1f/1.5f;
                player.GetDamage<GenericDamageClass>() += 0.425f/1.5f;
                player.GetCritChance<GenericDamageClass>() += 5f/1.5f;
                player.GetKnockback<SummonDamageClass>() += 1f/1.5f;



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
            if(player.GetModPlayer<StimPlayer>().Withdrawl)
                player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());
        }

        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {

            base.PostDraw(spriteBatch, buffIndex, drawParams);
        }


    }
}
