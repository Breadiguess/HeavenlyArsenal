using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using Luminance.Common.Utilities;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Utilities;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Buffs;

public class AntishadowAssassinBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;

        PlayerDataManager.ResetEffectsEvent += ResetMinionState;
        On_Main.MouseText_DrawBuffTooltip += RenderNameWithSpecialFont;
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

    private void ResetMinionState(PlayerDataManager p)
    {
        p.Player.GetValueRef<bool>("HasAntishadowAssassin").Value = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        var assassinID = ModContent.ProjectileType<AntishadowAssassin>();
        var hasMinion = player.GetValueRef<bool>("HasAntishadowAssassin");

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == assassinID && projectile.owner == player.whoAmI && projectile.As<AntishadowAssassin>().State != AntishadowAssassin.AssassinState.Leave)
            {
                hasMinion.Value = true;

                break;
            }
        }

        if (!hasMinion.Value)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
        else
        {
            player.buffTime[buffIndex] = 3;
        }
    }
}