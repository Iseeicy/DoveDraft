using System;
using Godot;

namespace DoveDraft.Item;

/// <summary>
///  An inventory that can hold items in it. Actually adding and removing items to this inventory is not meant to be handled manually - see the ItemInstance class for that!
/// </summary>
[Tool, GlobalClass]
public partial class ItemInventory : Node
{
    //
    //  Enums
    //

    /// <summary>
    /// Used to indicate if an inventory operation succeeded, or if there was some kind of error.
    /// </summary>
    public enum InventoryError
    {
        /// <summary>
        /// There is no error.
        /// </summary>
        Ok = 1,
        
        /// <summary>
        /// The slot is occupied.
        /// </summary>
        SlotOccupied = -1,
        
        /// <summary>
        /// The filter denied this.
        /// </summary>
        FilterDenied = -2,
        
        /// <summary>
        /// The entire amount could not fit.
        /// </summary>
        NoFit = -3,
        
        /// <summary>
        /// The two items are different types.
        /// </summary>
        DifferentTypes = -4,
    }
    
    //
    //  Exports
    //
    
    /// <summary>
    /// Emitted when the value of a slot is changed.
    /// </summary>
    [Signal]
    public delegate void SlotUpdatedEventHandler(int index, ItemInstance item);

    /// <summary>
    /// How many slots are there in this inventory?
    /// </summary>
    [Export]
    public int Size { get; set; } = 64;

    /// <summary>
    /// OPTIONAL. The filter that items must pass in order to be added to this inventory.
    /// </summary>
    [Export] public ItemFilterBase Filter { get; set; }

    //
    //  Public Variables
    //

    /// <summary>
    /// How many slots are occupied.
    /// </summary>
    public int TotalUsedSlots => AllItems.Count;
    
    /// <summary>
    /// How many slots are un-occupied.
    /// </summary>
    public int TotalUnusedSlots => Size - TotalUsedSlots;
    
    /// <summary>
    /// A list of every slot in this inventory, in order. Some slots may be empty, which means it will contain a null value.
    /// </summary>
    public Godot.Collections.Array<ItemInstance> AllSlots => new(_slots);
    
    /// <summary>
    /// All items that are stored in this inventory, mapped to their slot index in the inventory (ItemInstance -> int).
    /// </summary>
    public Godot.Collections.Dictionary<ItemInstance, int> AllItems
    {
        get
        {
            var foundItems = new System.Collections.Generic.Dictionary<ItemInstance, int>();
            for (var i = 0; i < _slots.Length; i++)
            {
                if(_slots[i] == null) continue;
                foundItems[_slots[i]] = i;
            }
            return new Godot.Collections.Dictionary<ItemInstance, int>(foundItems);
        }
    }

    /// <summary>
    /// The index of the last filled slot in the inventory. -1 if no slots are filled.
    /// </summary>
    public int LastFilledSlot
    {
        get
        {
            for (var i = _slots.Length - 1; i >= 0; i--)
            {
                if (_slots[i] != null) return i;
            }
            return -1;
        }
    }
    
    //
    //  Private Variables
    //

    /// <summary>
    /// The data structure backing this inventory.
    /// </summary>
    private ItemInstance[] _slots;
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Get the item found at the given slot.
    /// </summary>
    /// <param name="index">Which slot of this inventory to look in. Should be greater or equal to 0 and less than `Size`.</param>
    /// <returns>The found item, or null if there's nothing there.</returns>
    public ItemInstance GetAtSlot(int index) => _slots[index];

    /// <returns>The item found at the last filled slot, if there is any. null if there isn't one.</returns>
    public ItemInstance GetAtLastFilledSlot()
    {
        var index = LastFilledSlot;
        return index < 0 ? null : GetAtSlot(index);
    }

    /// <summary>
    /// Create a dictionary of items stored in this inventory that use the given descriptor.
    /// </summary>
    /// <param name="desc">The descriptor to filter by.</param>
    /// <returns>A dictionary where items using `desc` are mapped to their slot index (ItemInstance -> int)</returns>
    public Godot.Collections.Dictionary<ItemInstance, int> GetItemsWithDescriptor(ItemDescriptor desc)
    {
        var foundItems = new System.Collections.Generic.Dictionary<ItemInstance, int>();
        for (var i = 0; i < _slots.Length; i++)
        {
            if(_slots[i]?.Descriptor != desc) continue;
            foundItems[_slots[i]] = i;
        }
        return new Godot.Collections.Dictionary<ItemInstance, int>(foundItems);
    }

    /// <summary>
    /// Given an item assumed to be in this inventory, return the index of the slot that it's in. 
    /// </summary>
    /// <param name="item">The item instance to look for in this inventory.</param>
    /// <returns>The index of the item if found. -1 if not found.</returns>
    public int IndexOf(ItemInstance item) => Array.IndexOf(_slots, item);

    /// <summary>
    /// Is this item in this inventory?
    /// </summary>
    /// <param name="item">The item to look for.</param>
    /// <returns>True if this inventory contains the given item. False otherwise.</returns>
    public bool Contains(ItemInstance item) => IndexOf(item) >= 0;

    /// <summary>
    /// Is there an item that uses the given descriptor in the inventory?
    /// </summary>
    /// <param name="descToLookFor">The descriptor to look for within items in this inventory.</param>
    /// <returns>True if at least a single item uses the given descriptor. False otherwise.</returns>
    public bool ContainsDescriptor(ItemDescriptor descToLookFor)
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            ItemDescriptor foundDesc = _slots[i]?.Descriptor;
            if (foundDesc == descToLookFor) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Find the index of the next filled slot in the inventory, in a given direction.
    /// </summary>
    /// <param name="startIndex">The index to start searching from (exclusive).</param>
    /// <param name="direction"> Which direction to search in. `1` for forward, `-1` for backward.</param>
    /// <param name="wrap">Should the search wrap around the inventory if the index runs out of bounds?</param>
    /// <returns>
    /// - The index of the next filled slot in the given direction.
    /// - `-1` if start_index is out of bounds.
    /// - `-1` if direction is `0`.
    /// - `-1` if a filled slot could not be found.
    /// </returns>
    public int FindFilledSlotInDirection(int startIndex, int direction, bool wrap = false)
    {
        // Ensure that the direction is either -1, 0, or 1.
        direction = Mathf.Clamp(Mathf.RoundToInt(direction), -1, 1);
        // If we're not looking in a direction for some reason, EXIT EARLY
        if (direction == 0) return -1;
        
        // Modify our start index so we don't accidentally look at it and give a false positive.
        // If our starting index is out of bounds, EXIT EARLY
        if (wrap)
        {
            if (startIndex < 0 || startIndex >= Size) return -1;
            startIndex = Mathf.PosMod(startIndex + direction, Size);
        }
        else
        {
            startIndex += direction;
            if (startIndex < 0 || startIndex >= Size) return -1;
        }

        if (wrap)
        {
            // Return the first slot index we find that is filled, making sure to wrap around the array if we hit the end.
            for (var x = 0; x < Size - 1; x++)
            {
                var wrappedIndex = Mathf.PosMod(startIndex + (direction * x), Size);
                if (GetAtSlot(wrappedIndex) != null) return wrappedIndex;
            }
        }
        else
        {
            // Return the first slot index we find that is filled, making sure to stop at the end index.
            if (direction > 0)
            {
                for (var x = startIndex; x < Size; x++)
                {
                    if (GetAtSlot(x) != null) return x;
                }
            }
            else
            {
                for (var x = startIndex; x >= 0; x--)
                {
                    if (GetAtSlot(x) != null) return x;
                }
            }
        }
        
        // OTHERWISE, if we get here then there are NO filled slots after the start index in the requested direction.
        return -1;
    }

    /// <summary>
    /// Find the index of the next filled slot AFTER the given slot in the inventory.
    /// </summary>
    /// <param name="startIndex">The index to start searching from (exclusive).</param>
    /// <param name="wrap">Should the search wrap around the inventory if the index runs out of bounds?</param>
    /// <returns>
    /// - The index of the next filled slot.
    /// - `-1` if start_index is out of bounds.
    /// - `-1` if a filled slot could not be found.
    /// </returns>
    public int FindFilledSlotAfter(int startIndex, bool wrap = false) =>
        FindFilledSlotInDirection(startIndex, 1, wrap);
    
    /// <summary>
    /// Find the index of the next filled slot BEFORE the given slot in the inventory.
    /// </summary>
    /// <param name="startIndex">The index to start searching from (exclusive).</param>
    /// <param name="wrap">Should the search wrap around the inventory if the index runs out of bounds?</param>
    /// <returns>
    /// - The index of the next filled slot.
    /// - `-1` if start_index is out of bounds.
    /// - `-1` if a filled slot could not be found.
    /// </returns>
    public int FindFilledSlotBefore(int startIndex, bool wrap = false) =>
        FindFilledSlotInDirection(startIndex, -1, wrap);
    
    //
    //  Godot Methods
    //

    public override void _Ready()
    {
        base._Ready();

        // Allocate enough memory for our size
        _slots = new ItemInstance[Size];
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        // TODO - add some behavior for getting rid of items? perhaps they're put into the game world when the inventory is free'd?
        // if what == NOTIFICATION_PREDELETE:
        //  for item in get_all_items().keys():
        // 	    Do something
    }
    
    //
    //  Internal Methods
    //

    /// <summary>
    /// Meant to only be called by ItemInstance. Pushes an item into the inventory wherever it will fit. If there are slots the item can stack into, then it will stack before taking up an empty slot. This may modify the stack size of `item` - potentially making it zero if entirely merged into other stacks - so make sure to act accordingly.
    /// </summary>
    /// <param name="item">The item to push into the inventory</param>
    /// <returns>`InventoryError.OK` if the item was pushed into this inventory. `InventoryError.FILTER_DENIED` if the item did not pass through the filter. `InventoryError.NO_FIT` if some or all of the item stack could not fit</returns>
    internal InventoryError PushItem(ItemInstance item)
    {
        // If this does not pass through the inventory filters, EXIT EARLY
        if (!DoesItemPassFilter(item)) return InventoryError.FilterDenied;

        // Loop through the slots that we can merge this stack into and try to fit all items in
        foreach (ItemInstance itemToStackOn in GetItemsWithDescriptor(item.Descriptor).Keys)
        {
            // If we have nothing left to stack, then BREAK THE LOOP
            if(item.StackSize <= 0) break;
            
            // Merge our item into this stack
            item.MergeStackInto(itemToStackOn);
        }
        
        // If we don't have anything left to stack, EXIT EARLY
        if (item.StackSize <= 0) return InventoryError.Ok;
        
        // Find the first empty slot
        var emptySlotIndex = IndexOf(null);
        // If we don't have an empty slot to fit the rest in, EXIT EARLY
        if (emptySlotIndex == -1) return InventoryError.NoFit;
        // At this point, we for sure have an empty slot to put the rest of this item in - so do it!
        return PutItemInSlot(emptySlotIndex, item);
    }

    /// <summary>
    /// Meant to only be called by ItemInstance. Takes an item out of the given slot `index`, and removes it from the inventory.
    /// </summary>
    /// <param name="index">The slot to take an item from.</param>
    /// <returns>The item found in the slot - or null if there isn't one.</returns>
    internal ItemInstance TakeItemFromSlot(int index)
    {
        ItemInstance foundItem = GetAtSlot(index);
        _slots[index] = null;
        EmitSignal(SignalName.SlotUpdated, index, default);
        return foundItem;
    }

    /// <summary>
    /// Meant to only be called by ItemInstance. Puts an item into the given slot `index`. This may modify the stack size of `item` - potentially making it zero if entirely merged into the desired slot - so make sure to act accordingly.
    /// </summary>
    /// <param name="index">The slot to put the item in.</param>
    /// <param name="item">The item to place in this inventory</param>
    /// <returns></returns>
    internal InventoryError PutItemInSlot(int index, ItemInstance item)
    {
        // If this does not pass through the inventory filters, EXIT EARLY
        if (!DoesItemPassFilter(item)) return InventoryError.FilterDenied;
        
        // Find out what's already in this slot
        var exitingItem = GetAtSlot(index);
        
        // If there's nothing in this slot, just put it in!
        if (exitingItem == null)
        {
            _slots[index] = item;
            EmitSignal(SignalName.SlotUpdated, index, item);
            return InventoryError.Ok;
        }
        
        // OTHERWISE, there IS something in the slot so...
        // If these items are different types, EXIT EARLY
        if (exitingItem.Descriptor != item.Descriptor) return InventoryError.DifferentTypes;
        
        // Merge the stacks
        item.MergeStackInto(exitingItem);
        
        // If we still have items that aren't merged, EXIT EARLY
        if (item.StackSize > 0) return InventoryError.NoFit;
        
        // OTHERWISE, we merged entirely!
        return InventoryError.Ok;
    }
    
    //
    //  Private Methods
    //

    private bool DoesItemPassFilter(ItemInstance item)
    {
        if (Filter == null) return true;
        return Filter.Evaluate(item, this) == ItemFilterBase.FilterResult.Pass;
    }
}