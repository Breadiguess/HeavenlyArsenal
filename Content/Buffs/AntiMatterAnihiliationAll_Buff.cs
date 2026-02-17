using Luminance.Core.Hooking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.DataStructures;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Buffs;

public class AntimatterAnnihilationAllBuff : ModBuff
{
    public const string DPSVariableName = "AntimatterAnnihilationDPS";

    public override string Texture => base.Texture;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        new ManagedILEdit
        (
            "Use custom death text for Antimatter Annihilation",
            Mod,
            edit => { IL_Player.KillMe += edit.SubscriptionWrapper; },
            edit => { IL_Player.KillMe -= edit.SubscriptionWrapper; },
            UseCustomDeathMessage
        ).Apply();
    }

    private static void UseCustomDeathMessage(ILContext context, ManagedILEdit edit)
    {
        var cursor = new ILCursor(context);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStfld<Player>("crystalLeaf")))
        {
            edit.LogFailure("Could not find the crystalLeaf storage");

            return;
        }

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<PlayerDeathReason>("GetDeathText")))
        {
            edit.LogFailure("Could not find the GetDeathText call");

            return;
        }

        cursor.Emit(OpCodes.Ldarg_0);

        cursor.EmitDelegate
        (
            (NetworkText text, Player player) =>
            {
                if (player.HasBuff<AntimatterAnnihilationAllBuff>())
                {
                    var deathText = Language.GetText($"Mods.NoxusBoss.Death.AntimatterAnnihilation{Main.rand.Next(5) + 1}");

                    return PlayerDeathReason.ByCustomReason(deathText.Format(player.name)).GetDeathText(player.name);
                }

                return text;
            }
        );
    }
}