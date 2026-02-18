using HeavenlyArsenal.Core.Globals;
using HeavenlyArsenal.Utilities.Extensions;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.LightCultist;

[AutoloadEquip(EquipType.Head)]
internal class LightCultist_Helmet : ModItem
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
}

internal class lightCultist_Drawlayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.FrontAccFront);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.armor[10].IsAir && drawInfo.drawPlayer.armor[0].ModItem is LightCultist_Helmet || drawInfo.drawPlayer.armor[10].ModItem is LightCultist_Helmet;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Vanity/LightCultist/Halo");

        var drawData = new DrawData
            (texture.Value, drawInfo.GetHeadDrawPosition(), drawInfo.drawPlayer.headFrame, drawInfo.colorHair, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect);

        //drawData.shader = drawInfo.hairDyePacked;
        drawInfo.DrawDataCache.Add(drawData);
    }
}