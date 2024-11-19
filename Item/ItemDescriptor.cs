using System.Collections.Generic;
using Godot;

namespace DoveDraft.Item;

/// <summary>
/// Represents a certain type of item. Acts as a factory for ItemInstances of the item type it represents.
/// </summary>
[Tool, GlobalClass, Icon("../Icons/item_descriptor.png")]
public partial class ItemDescriptor : Resource
{
    //
    //  Exports
    //
    
    /// <summary>
    /// The name of the item used for keys. Should use snake case. No spaces!
    /// </summary>
    [Export]
    public string ItemName { get; set; } = "";

    /// <summary>
    /// For stacks of this item, how many items can be stored in a single stack?
    /// </summary>
    [Export]
    public int MaxStackSize { get; set; } = 1;

    /// <summary>
    /// The scene that contains the scripting for this item. Root node should inherit from `ItemScriptBase`.
    /// </summary>
    [ExportGroup("Scenes")]
    [Export]
    public PackedScene ItemScriptScene { get; set; }

    /// <summary>
    /// The scene that contains the view model for this item. Root node should inherit from ItemViewModel2D.
    /// </summary>
    [ExportSubgroup("2D")]
    [Export]
    public PackedScene ViewModel2DScene { get; set; }
    
    /// <summary>
    /// The scene that contains the object to be spawned in the world for this item. Root node should inherit from WorldItem2D.
    /// </summary>
    [Export]
    public PackedScene WorldItem2DScene { get; set; }
    
    /// <summary>
    /// The scene that contains the view model for this item. Root node should inherit from ItemViewModel3D.
    /// </summary>
    [ExportSubgroup("3D")]
    [Export]
    public PackedScene ViewModel3DScene { get; set; }
    
    /// <summary>
    /// The scene that contains the object to be spawned in the world for this item. Root node should inherit from WorldItem3D.
    /// </summary>
    [Export]
    public PackedScene WorldItem3DScene { get; set; }

    /// <summary>
    /// The name to use when displaying this item's name to the user. If empty, then `item_name` will be used instead.
    /// </summary>
    [ExportGroup("UI")]
    [Export]
    public string DisplayName
    {
        get => string.IsNullOrEmpty(_displayName) ? ItemName : _displayName;
        set => _displayName = value;
    }
    private string _displayName = "";
    
    /// <summary>
    /// A string used to categorize this item. This is typically used by the inventory UI in some way to display this item in a specific way.
    /// </summary>
    [Export]
    public string Category { get; set; } = "";
    
    /// <summary>
    /// An icon used to preview this item, usually in an inventory.
    /// </summary>
    [Export]
    public Texture2D PreviewIcon { get; set; }
    
    //
    //  Methods
    //
    
    /// <returns>A new instance of this kind of item.</returns>
    public ItemInstance CreateInstance()
    {
        var instance = new ItemInstance();
        instance.Setup(this);
        instance.Name = $"{ItemName}_{GetInstanceId()}";
        return instance;
    }

    /// <summary>
    /// Constructs many new stacked instances of this kind of item, respecting max stack size.
    /// </summary>
    /// <param name="count">How many items to create. If an item's max stack size is greater than one, then this will not necessarily equal the number of instances that are created.</param>
    /// <returns>An array of new ItemInstances where the combined stack size equals the count parameter.</returns>
    public Godot.Collections.Array<ItemInstance> CreateManyInstances(int count)
    {
        var result = new List<ItemInstance>();
        
        // While we still have items to stack...
        while (count > 0)
        {
            ItemInstance instance = CreateInstance();
            
            // Figure out how much we can fit in this instance, and fit it
            var stackSize = Mathf.Min(instance.MaxStackSize, count);
            instance.StackSize = stackSize;
            
            // Keep track of how many we have left to fit
            count -= stackSize;
            result.Add(instance);
        }

        return new Godot.Collections.Array<ItemInstance>(result);
    }
}