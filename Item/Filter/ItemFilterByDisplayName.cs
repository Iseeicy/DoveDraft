using Godot;

namespace DoveDraft.Item.Filter;

/// <summary>
/// Filters items using a RegEx on the return of `DisplayName` on an item's descriptor.
/// </summary>
[Tool, GlobalClass]
public partial class ItemFilterByDisplayName : ItemFilterBase
{
    //
    //  Exports
    //

    /// <summary>
    /// The regex string to use when matching the display name.
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
        var foundMatch = CompiledRegEx.Search(item.Descriptor.DisplayName) != null;
        return foundMatch ? FilterResult.Pass : FilterResult.Reject;
    }
}