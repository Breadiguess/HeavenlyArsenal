using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Core.Items;

/// <summary>
///     Provides a system for handling registration and application of additional
///     <see cref="IItemDropRule" /> instances into existing <see cref="ItemLoot" /> instances.
/// </summary>
public sealed class ItemLootGlobalItem : GlobalItem
{
    /// <summary>
    ///     Stores item loot data indexed by item type.
    /// </summary>
    /// <remarks>
    ///     Each entry defines additional <see cref="IItemDropRule" /> instances that will be injected into
    ///     the item's <see cref="ItemLoot" /> during <see cref="GlobalItem.ModifyItemLoot" />.
    /// </remarks>
    private static Dictionary<int, ItemLootData> dataByType = new();

    public override void Unload()
    {
        base.Unload();
        
        dataByType?.Clear();
        dataByType = null;
    }

    public override void ModifyItemLoot(Item item, ItemLoot loot)
    {
        base.ModifyItemLoot(item, loot);

        if (!dataByType.TryGetValue(item.type, out var data))
        {
            return;
        }

        foreach (var rule in data.Rules)
        {
            loot.Add(rule);
        }
    }

    /// <summary>
    ///     Registers an item drop rule to be injected into the loot table of the specified item type.
    /// </summary>
    /// <param name="type">The item type whose loot table will receive the item drop rule.</param>
    /// <param name="rule">The <see cref="IItemDropRule" /> to register.</param>
    /// <typeparam name="T">The type of the <see cref="IItemDropRule" /> to register.</typeparam>
    public static void RegisterLoot<T>(int type, T rule) where T : IItemDropRule
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        if (!dataByType.TryGetValue(type, out _))
        {
            dataByType[type] = new ItemLootData();
        }

        dataByType[type].Rules.Add(rule);
    }
}