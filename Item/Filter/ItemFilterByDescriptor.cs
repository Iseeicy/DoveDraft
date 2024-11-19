using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items by trying to match an item's descriptor to a certain descriptor.
/// </summary>
[Tool, GlobalClass]
public partial class ItemFilterByDescriptor : ItemFilterBase
{
    //
    //  Exports
    //

    /// <summary>
    /// The descriptor to match when filtering.
    /// </summary>
    [Export]
    public ItemDescriptor Descriptor { get; set; }
    
    /// <summary>
    /// The RegEx compiled from `RegExString`.
    /// </summary>
    public RegEx CompiledRegEx { get; private set; }
    
    //
    //  Item Filter Methods
    //

    public override FilterResult Evaluate(ItemInstance item, ItemInventory inventory)
    {
        var matchesDesc = item.Descriptor == Descriptor;
        return matchesDesc ? FilterResult.Pass : FilterResult.Reject;
    }
}