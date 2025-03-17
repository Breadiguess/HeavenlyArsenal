using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Core.Globals;

public class ArsenalGlobalItem : GlobalItem
{
    public delegate void ModifyItemLootDelegate(Item item, ItemLoot loot);

    public static event ModifyItemLootDelegate? ModifyItemLootEvent;

    public override void ModifyItemLoot(Item item, ItemLoot loot)
    {
        ModifyItemLootEvent?.Invoke(item, loot);
    }
}
