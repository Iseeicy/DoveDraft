using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items using a RegEx on the category string of an item's descriptor.
/// </summary>
[Tool, GlobalClass, Icon("../../Icons/item_filter.png")]
public partial class ItemFilterByCategory : ItemFilterBase
{
    //
    //  Exports
    //

    /// <summary>
    /// The regex string to use when matching the category.
    /// </summary>
    [Export]
    public string RegExString
    {
        get => _regExString;
        set
        {
            _regExString = value;
            CompiledRegEx = RegEx.CreateFromString(value);
        }
    }
    private string _regExString = ".";
    
    /// <summary>
    /// The RegEx compiled from `RegExString`.
    /// </summary>
    public RegEx CompiledRegEx { get; private set; }
    
    //
    //  Item Filter Methods
    //

    public override FilterResult Evaluate(ItemInstance item, ItemInventory inventory)
    {
        // Run the regex on the category string
        var foundMatch = CompiledRegEx.Search(item.Descriptor.Category) != null;
        return foundMatch ? FilterResult.Pass : FilterResult.Reject;
    }
}