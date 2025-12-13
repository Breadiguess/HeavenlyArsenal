using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.LightCultist
{
    [AutoloadEquip(EquipType.Body)]
    class LightCultist_Robe : ModItem
    {
        public override string LocalizationCategory => "Items.Armor.Vanity.LightCultist";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();
            Item.value = 0;
            Item.vanity = true;
            Item.maxStack = 1;
        }

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/Vanity/LightCultist/LightCultist_Waist", EquipType.Waist, this);
            }
        }
        public override void SetStaticDefaults()
        {
            // HidesHands defaults to true which we don't want.
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ItemID.Sets.ItemNoGravity[Type] = true;
            GlobalNPCEventHandlers.ModifyNPCLootEvent += (NPC npc, NPCLoot npcLoot) =>
            {
                if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
                {
                    LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
                    {
                        normalOnly.OnSuccess(ItemDropRule.Common(Type, minimumDropped: 1, maximumDropped: 1));
                    }
                    npcLoot.Add(normalOnly);
                }
            };
            ArsenalGlobalItem.ModifyItemLootEvent += (Item item, ItemLoot loot) =>
            {
                if (item.type == NamelessDeityBoss.TreasureBagID)
                    loot.Add(ItemDropRule.Common(Type, minimumDropped: 1, maximumDropped: 1));
            };
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            robes = true;
            equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            //ArmorIDs.Legs.Sets.
        }
    }
}
