using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimit : ModItem
{
    public override void SetStaticDefaults()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
            {
                LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }
                npcLoot.Add(normalOnly);
            }
        };
        ArsenalGlobalItem.ModifyItemLootEvent += (item, loot) =>
        {
            if (item.type == NamelessDeityBoss.TreasureBagID)
                loot.Add(ItemDropRule.Common(Type));
        };
    }

    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 12;
        Item.DamageType = DamageClass.Magic;
        Item.damage = 12000;
        Item.knockBack = 3f;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.autoReuse = true;
        Item.mana = 32;
        Item.holdStyle = 0;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<RocheLimitBlackHole>();
        Item.shootSpeed = 10f;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.buyPrice(gold: 2);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
}
