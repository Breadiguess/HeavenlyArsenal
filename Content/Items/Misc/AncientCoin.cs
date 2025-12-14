using System.Collections.Generic;
using System.Linq;
using CalamityMod.NPCs.CeaselessVoid;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.World.WorldSaving;

namespace HeavenlyArsenal.Content.Items.Misc;

internal class AncientCoin : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Quest;
        Item.value = 0;
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // Rewrite tooltips post-Nameless Deity.
        if (BossDownedSaveSystem.HasDefeated<CeaselessVoid>())
        {
            // Remove the default tooltips.
            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));

            // Generate and use custom tooltips.
            var specialTooltip = this.GetLocalizedValue("postCeaseless");

            var tooltipLines = specialTooltip.Split('\n')
                .Select
                (
                    (t, index) =>
                    {
                        //Item.rare = ModContent.RarityType<AvatarRarity>();
                        return new TooltipLine(Mod, $"postCeaseless{index + 1}", t);
                    }
                )
                .ToArray();

            // Color the last tooltip line.
            // tooltipLines.Last().OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
            tooltips.AddRange(tooltipLines);

            return;
        }
        //grrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr

        // Make the final tooltip line about needing to pass the test use Nameless' dialog.
        var tooltip = tooltips.FirstOrDefault(t => t.Name == "postCeasless");

        if (tooltip is not null)
        {
            tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
        }
    }
}