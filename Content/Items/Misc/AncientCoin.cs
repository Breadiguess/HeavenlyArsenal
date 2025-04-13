using CalamityMod.NPCs.CeaselessVoid;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using NoxusBoss.Core.World.WorldSaving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Misc
{
    class AncientCoin : ModItem
    {
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Quest;

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
                string specialTooltip = this.GetLocalizedValue("postCeaseless");
                TooltipLine[] tooltipLines = specialTooltip.Split('\n').Select((t, index) =>
                {
                    //Item.rare = ModContent.RarityType<AvatarRarity>();
                    return new TooltipLine(Mod, $"postCeaseless{index + 1}", t);
                }).ToArray();

                // Color the last tooltip line.
               // tooltipLines.Last().OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
                tooltips.AddRange(tooltipLines);
                return;
            }
            //grrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr

            // Make the final tooltip line about needing to pass the test use Nameless' dialog.
            TooltipLine tooltip = tooltips.FirstOrDefault(t => t.Name == "postCeasless");
            if (tooltip is not null)
                tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
        }

    }
}