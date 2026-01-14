using HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Utilities;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Buffs;

public class ButterflyMinionBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;

        PlayerDataManager.ResetEffectsEvent += ResetMinionState;
    }

  

    private void ResetMinionState(PlayerDataManager p)
    {
        p.Player.GetValueRef<bool>("HasSolyn").Value = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        var assassinID = ModContent.ProjectileType<ButterflyMinion>();
        var hasMinion = player.GetValueRef<bool>("HasSolyn");

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == assassinID && projectile.owner == player.whoAmI)
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