using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Materials;

public sealed class AvatarMaterialGlobalItem : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == AvatarOfEmptiness.TreasureBagID;
    }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        base.ModifyItemLoot(item, itemLoot);

        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AvatarMaterial>(), 3, 3));
    }
}