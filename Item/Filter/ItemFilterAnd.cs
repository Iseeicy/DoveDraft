using Godot;
using Godot.Collections;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items using a list of sub-filters. If an item passes through all the sub-filters, then it will pass through this filter. If an item fails to pass a SINGLE sub-filter, the item will be rejected.
/// </summary>
[Tool, GlobalClass, Icon("../../Icons/item_filter.png")]
public partial class ItemFilterAnd : ItemFilterBase
{
    //
    //  Exports
    //

    /// <summary>
    /// The filters to use. When this filter is evaluated, it will check each of these filters first.
    /// </summary>
    [Export]
    public Array<ItemFilterBase> Filters { get; set; } = new();
    
    //
    //  Item Filter Methods
    //

    public override FilterResult Evaluate(ItemInstance item, ItemInventory inventory)
    {
        for (var index = 0; index < Filters.Count; index++)
        {
            // If we fail a single filter, EXIT EARLY
            if (Filters[index].Evaluate(item, inventory) != FilterResult.Pass) return FilterResult.Reject;
        }

        // If we pass all filters, WE PASS!
        return FilterResult.Pass;
    }
}