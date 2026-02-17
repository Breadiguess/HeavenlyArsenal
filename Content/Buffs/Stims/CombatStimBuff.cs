using CalamityMod.Items.Weapons.Rogue;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Configuration;
using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Buffs.Stims;

internal class CombatStimBuff : ModBuff
{
    internal bool notApplied { get; private set; }

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
        var addiction = player.GetModPlayer<StimPlayer>().addictionChance;
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
            var vanillaFont = FontAssets.MouseText.Value;
            var vanillaTextSize = vanillaFont.MeasureString(buffString);

            var font = FontRegistry.Instance.AvatarPoemText;
            var text = this.GetLocalizedValue("NameText");
            var drawPosition = new Vector2(X + (int)vanillaTextSize.X + 6f, Y + 42f);

            ChatManager.DrawColorCodedStringWithShadow
                (Main.spriteBatch, font, text, drawPosition, new Color(252, 37, 74), 0f, font.MeasureString(text) * Vector2.UnitY * 0.5f, Vector2.One * 0.5f, -1f, 1f);
        }
    }

    public override void Update(Player player, ref int buffIndex)
    {
        var isNotHoldingCursedItems = player.HeldItem.type != ModContent.ItemType<ExecutionersBlade>() &&
                                      player.HeldItem.type != ModContent.ItemType<Hypothermia>() &&
                                      player.HeldItem.type != ModContent.ItemType<Wrathwing>();

        if (ClientSideConfiguration.Instance != null && ClientSideConfiguration.Instance.StimVFX)
        {
            if (!GeneralScreenEffectSystem.ChromaticAberration.Active)
            {
                GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, ClientSideConfiguration.Instance.ChromaticAbberationMultiplier, 0);
            }
        }

        if (player.GetModPlayer<StimPlayer>().Addicted)
        {
            player.statDefense += 3;

            if (isNotHoldingCursedItems)
            {
                player.GetAttackSpeed<GenericDamageClass>() += 1f / 1.5f;
            }

            player.GetDamage<GenericDamageClass>() += 0.425f / 1.5f;
            player.GetCritChance<GenericDamageClass>() += 5f / 1.5f;
            player.GetKnockback<SummonDamageClass>() += 1f / 1.5f;

            player.moveSpeed += 0.68f;
            player.pickSpeed -= 0.2f;
            player.jumpSpeedBoost += 1f;
        }
        else
        {
            player.statDefense += 5;

            if (isNotHoldingCursedItems)
            {
                player.GetAttackSpeed<GenericDamageClass>() += 1f;
            }

            player.GetDamage<GenericDamageClass>() += 0.425f;
            player.GetCritChance<GenericDamageClass>() += 5f;
            player.GetKnockback<SummonDamageClass>() += 1f;

            player.moveSpeed += 1f;
            player.pickSpeed -= 0.2f;
            player.jumpSpeedBoost += 1f;
        }

        if (player.GetModPlayer<StimPlayer>().Withdrawl)
        {
            player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
    {
        base.PostDraw(spriteBatch, buffIndex, drawParams);
    }
}