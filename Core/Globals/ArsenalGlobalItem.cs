using NoxusBoss.Content.Items;

namespace HeavenlyArsenal.Core.Globals;

public class ArsenalGlobalItem : GlobalItem
{
    public delegate void ModifyItemLootDelegate(Item item, ItemLoot loot);

    public static event ModifyItemLootDelegate? ModifyItemLootEvent;

    public override void SetDefaults(Item entity)
    {
        if (entity.type == ModContent.ItemType<MetallicChunk>())
        {
            entity.value = 0_000_030;
        }
    }

    public override void ModifyItemLoot(Item item, ItemLoot loot)
    {
        ModifyItemLootEvent?.Invoke(item, loot);
    }

    // TODO: try to mess around with the Items name while empowered
    /*
    public override void SetDefaults(Item item)
    {

        if (item.netID == ModContent.ItemType<AvatarLonginus>())
        {
            foreach (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active in )
            {
                if (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active)
                    item.SetNameOverride("");
            }

        }
    }
    */
}