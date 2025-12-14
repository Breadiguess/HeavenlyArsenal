using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.LightCultist;

[AutoloadEquip(EquipType.Legs)]
public class LightCultist_Pants : ModItem
{
    public override string LocalizationCategory => "Items.Armor.Vanity.LightCultist";

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = Item.sellPrice(gold: 10);
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = 0;
        Item.vanity = true;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Type] = true;

        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
            {
                var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());

                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type, minimumDropped: 1, maximumDropped: 1));
                }

                npcLoot.Add(normalOnly);
            }
        };

        ArsenalGlobalItem.ModifyItemLootEvent += (item, loot) =>
        {
            if (item.type == NamelessDeityBoss.TreasureBagID)
            {
                loot.Add(ItemDropRule.Common(Type, minimumDropped: 1, maximumDropped: 1));
            }
        };
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Generic) += 0.20f;
        player.moveSpeed += 0.5f;
        player.runAcceleration *= 1.2f;
        player.maxRunSpeed *= 1.2f;
        player.accRunSpeed *= 0.5f;
        player.runSlowdown *= 2f;
    }
}