using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using HeavenlyArsenal.Content.Rarities;
using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Core.GlobalInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon
{
    class UmbralLeechDrop : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.sellPrice(0, 18, 0, 0);
        }
        public override void SetStaticDefaults()
        {
            GlobalNPCEventHandlers.ModifyNPCLootEvent += (NPC npc, NPCLoot npcLoot) =>
            {
                if (npc.type == ModContent.NPCType<NewLeech>())
                {
                    LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.BeatAnyMechBoss());
                    {
                        normalOnly.OnSuccess(ItemDropRule.Common(Type));
                    }
                    npcLoot.Add(normalOnly);
                }
            };
           
        }
    }
}
