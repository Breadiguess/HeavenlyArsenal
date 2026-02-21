using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Core.Items;

/// <summary>
///     Represents a collection of item drop rules associated with an item type.
/// </summary>
public readonly struct ItemLootData
{
    /// <summary>
    ///     Gets the item drop rules applied to the <see cref="ItemLoot"/> of the associated item type.
    /// </summary>
    public List<IItemDropRule> Rules { get; } = new();

    public ItemLootData() { }
}