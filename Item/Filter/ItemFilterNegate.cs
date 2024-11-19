using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items using some filter, but negates the result of the filter's evaluate function.
/// </summary>
[Tool, GlobalClass]
public partial class ItemFilterNegate : ItemFilterBase
{
    //
    //  Exports
    //

    /// <summary>
    /// The filter to negate.
    /// </summary>
    [Export]
    public ItemFilterBase Filter { get; set; }
    
    /// <summary>
    /// The RegEx compiled from `RegExString`.
    /// </summary>
    public RegEx CompiledRegEx { get; private set; }
    
    //
    //  Item Filter Methods
    //

    public override FilterResult Evaluate(ItemInstance item, ItemInventory inventory)
    {
        return Filter.Evaluate(item, inventory) == FilterResult.Pass ? FilterResult.Reject : FilterResult.Pass;
    }
}