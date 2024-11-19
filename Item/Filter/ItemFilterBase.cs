using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// The base class for all item filters. Not meant to be used directly.
/// </summary>
[Tool, GlobalClass, Icon("../../Icons/item_filter.png")]
public partial class ItemFilterBase : Resource
{
    //
    //  Enums
    //

    public enum FilterResult
    {
        Pass = 1,
        Reject = 0
    }

    /// <summary>
    /// Check to see if the given item passes through the filter for a given inventory. Extend this method!
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <param name="inventory">The inventory to consider</param>
    /// <returns>FilterResult.Pass if the item passes the filter. FilterResult.Reject otherwise.</returns>
    public virtual FilterResult Evaluate(ItemInstance item, ItemInventory inventory) => FilterResult.Reject;
}