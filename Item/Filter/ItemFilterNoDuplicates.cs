using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items by enforcing that any given item may not already have an item of the same type exist already in the given inventory.
/// </summary>
[Tool, GlobalClass]
public partial class ItemFilterNoDuplicates : ItemFilterBase
{
    //
    //  Item Filter Methods
    //

    public override FilterResult Evaluate(ItemInstance item, ItemInventory inventory)
    {
        // If the inventory already has this item's type, EXIT EARLY
        return inventory.ContainsDescriptor(item.Descriptor) ? FilterResult.Reject : FilterResult.Pass;
    }
}