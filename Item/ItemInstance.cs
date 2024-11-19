using System;
using Godot;
using Godot.Collections;

namespace DoveDraft.Item;

[Tool, GlobalClass, Icon("../Icons/item.png")]
public partial class ItemInstance : Node
{
    //
    //  Enums
    //
    
    /// <summary>
    /// The state of an item's existence in space.
    /// </summary>
    public enum SpaceState
    {
        /// <summary>
        /// This item doesn't exist anywhere other than in memory!
        /// </summary>
        Nowhere = 0, 
        
        /// <summary>
        /// This item is in an inventory somewhere.
        /// </summary>
        Inventory = 1,
        
        /// <summary>
        /// This item is in the 2D game world somewhere.
        /// </summary>
        World2D = 2,
        
        /// <summary>
        /// This item is in the 3D game world somewhere.
        /// </summary>
        World3D = 3
    }

    /// <summary>
    /// Used to indicate if an item instance operation succeeded, or if there was some kind of error.
    /// </summary>
    public enum InstanceError
    {
        /// <summary>
        /// There is no error.
        /// </summary>
        Ok = 0,
        
        /// <summary>
        /// We don't know the error, but there was one.
        /// </summary>
        Unknown = -1,
        
        /// <summary>
        /// This item already exists.
        /// </summary>
        AlreadyExists = -2,
        
        /// <summary>
        /// The scene is in the descriptor is missing.
        /// </summary>
        SceneMissing = -3,
        
        /// <summary>
        /// A filter denied this.
        /// </summary>
        FilterDenied = -4,
        
        /// <summary>
        /// This didn't fit.
        /// </summary>
        NoFit = -5,
        
        /// <summary>
        /// This item isn't the same type.
        /// </summary>
        DifferentTypes = -6,
    }
    
    //
    //  Exports
    //

    /// <summary>
    /// Emitted just as this item is being free'd.
    /// </summary>
    [Signal]
    public delegate void ItemFreedEventHandler();

    /// <summary>
    /// Emitted when the item's stack size changes.
    /// </summary>
    [Signal]
    public delegate void StackSizeChangedEventHandler();
    
    //
    //  Public Variables
    //

    /// <summary>
    /// The descriptor that this instance came from.
    /// </summary>
    public ItemDescriptor Descriptor { get; private set; }

    /// <summary>
    /// The scripting for this item, if any.
    /// </summary>
    public ItemScriptBase ItemScript { get; private set; }

    /// <summary>
    /// Generally where is this object spatially?
    /// </summary>
    public SpaceState InSpace { get; private set; } = SpaceState.Nowhere;

    /// <summary>
    /// How many items are currently represented by this instance? Typically for equipment this is 1, though for materials this number could increase. Setting this value will not respect MaxStackSize - so use it carefully. When this value changes to something new, it emits the StackSizeChanged signal.
    /// </summary>
    public int StackSize
    {
        get => _stackSize;
        internal set
        {
            // If the stack size is different from what we already have, update the value and fire off a signal.
            if (value == _stackSize) return;
            _stackSize = value;
            EmitSignal(SignalName.StackSizeChanged, value);
        }
    }
    private int _stackSize = 1;
    
    /// <summary>
    /// The maximum number of items that can possibly be fit in this instance.
    /// </summary>
    public int MaxStackSize => Descriptor.MaxStackSize;

    /// <summary>
    /// The number of items that can currently still fit in this instance.
    /// </summary>
    public int StackSpaceLeft => MaxStackSize - StackSize;

    /// <summary>
    /// Is this stack of items full?
    /// </summary>
    public bool StackIsFull => StackSize >= MaxStackSize;

    /// <summary>
    /// If this instance is in the 2D game world, then this returns the object that represents it. Otherwise, this is null.
    /// </summary>
    public WorldItem2D WorldItem2D { get; private set; }

    /// <summary>
    /// If this instance is in the 3D game world, then this returns the object that represents it. Otherwise, this is null.
    /// </summary>
    public WorldItem3D WorldItem3D { get; private set; }

    /// <summary>
    /// All 2D view models that have been spawned.
    /// </summary>
    public Array<ItemViewModel2D> ViewModels2D { get; private set; } = new();
    
    /// <summary>
    /// All 3D view models that have been spawned.
    /// </summary>
    public Array<ItemViewModel3D> ViewModels3D { get; private set; } = new();
    
    /// <summary>
    /// If this instance is in an inventory, then this is that inventory. Otherwise, this is null.
    /// </summary>
    public ItemInventory ParentInventory { get; private set; }
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Merge our stack of items into another given stack of items. It's up to whatever calls this to interpret the aftermath. This allows our item to go to 0 stack size, which means it should be removed.
    /// </summary>
    /// <param name="item">The item to merge our stack into. If this item's descriptor does not match our own, nothing happens.</param>
    public void MergeStackInto(ItemInstance item)
    {
        if (item.Descriptor != Descriptor) return;
        
        // Fit as much as we can into the given item, and subtract from our stack
        var canFitCount = Mathf.Min(StackSize, StackSpaceLeft);
        item.StackSize += canFitCount;
        StackSize -= canFitCount;
    }

    /// <summary>
    /// Split our stack of items into two stacks. This will create a new item instance that is immediately placed nowhere.
    /// </summary>
    /// <param name="newStackSize">The stack size of the new ItemInstance to split into.</param>
    /// <returns>The new ItemInstance with a StackSize of `newStackSize`. Otherwise, null if there is not enough to split into a new ItemInstance and still have stack in this instance.</returns>
    public ItemInstance SplitStack(int newStackSize)
    {
        // If they don't want any, EXIT EARLY
        if (newStackSize <= 0) return null;
        // If we don't have any, EXIT EARLY
        if (StackSize <= 0) return null;
        // If we are to use all of or more than what we have, EXIT EARLY
        if (StackSize - newStackSize <= 0) return null;
        
        // OTHERWISE, we can safely split the stack across this instance and a new instance.
        ItemInstance newInstance = Descriptor.CreateInstance();
        StackSize -= newStackSize;
        newInstance.StackSize = newStackSize;
        return newInstance;
    }

    /// <summary>
    /// Spawn and return a new instance of this item's view model, if there is one. 
    /// </summary>
    /// <returns>null if there isn't a 2D view model. Otherwise, returns an ItemViewModel2D.</returns>
    public ItemViewModel2D InstantiateViewModel2D()
    {
        // TODO - practice DRY
        if (Descriptor.ViewModel2DScene == null) return null;

        var newViewModel = Descriptor.ViewModel2DScene.Instantiate<ItemViewModel2D>();
        newViewModel.Setup(this);
        
        // Put this viewmodel in the cache of existing view models, and configure it so that it will be removed from the cache when the viewmodel is free'd.
        newViewModel.ViewModelFreed += () => ViewModels2D.Remove(newViewModel);
        ViewModels2D.Add(newViewModel);
        return newViewModel;
    }
    
    /// <summary>
    /// Spawn and return a new instance of this item's view model, if there is one. 
    /// </summary>
    /// <returns>null if there isn't a 3D view model. Otherwise, returns an ItemViewModel3D.</returns>
    public ItemViewModel3D InstantiateViewModel3D()
    {
        // TODO - practice DRY
        if (Descriptor.ViewModel3DScene == null) return null;

        var newViewModel = Descriptor.ViewModel3DScene.Instantiate<ItemViewModel3D>();
        newViewModel.Setup(this);
        
        // Put this viewmodel in the cache of existing view models, and configure it so that it will be removed from the cache when the viewmodel is free'd.
        newViewModel.ViewModelFreed += () => ViewModels3D.Remove(newViewModel);
        ViewModels3D.Add(newViewModel);
        return newViewModel;
    }

    /// <summary>
    /// Sets an animation parameter in every viewmodel that exists for this specific item instance.
    /// </summary>
    /// <param name="paramKey">The raw key for the AnimationTree parameter to set. If this doesn't match a valid param, nothing will happen.</param>
    /// <param name="paramValue">The value to set the param to, if found.</param>
    public void SetViewModelAnimParam(string paramKey, Variant paramValue)
    {
        foreach (ItemViewModel2D viewModel in ViewModels2D)
        {
            viewModel.AnimationTree.Set(paramKey, paramValue);
        }
        foreach (ItemViewModel3D viewModel in ViewModels3D)
        {
            viewModel.AnimationTree.Set(paramKey, paramValue);
        }
    }

    /// <summary>
    /// Places this item instance in the 2D game world. If it was in an inventory, it is removed from that inventory.
    /// </summary>
    /// <param name="worldRoot">The root of the world to place in. If not provided, `get_window()` is used.</param>
    /// <returns></returns>
    public InstanceError PutInWorld2D(Node worldRoot)
    {
        // If the item is already in a world, EXIT EARLY
        if (InSpace is SpaceState.World2D or SpaceState.World3D) return InstanceError.AlreadyExists;
        // If we don't have any way to represent this item in a 2D world, EXIT EARLY
        if (Descriptor.WorldItem2DScene == null) return InstanceError.SceneMissing;
        
        // Use the window as world root, if not provided
        worldRoot ??= GetWindow();
        
        // Remove ourselves from an inventory if we're in one
        RemoveFromInventory();
        
        // Spawn and set up the new 2D world item
        var worldItem = Descriptor.WorldItem2DScene.Instantiate<WorldItem2D>();
        worldItem.Setup(this);
        worldRoot.AddChild(worldItem);
        WorldItem2D = worldItem;
        
        // Reparent us to the world item to make it easier to visualize where this item is in the editor.
        ChangeParent(WorldItem2D);

        InSpace = SpaceState.World2D;
        return InstanceError.Ok;
    }
    
    /// <summary>
    /// Places this item instance in the 3D game world. If it was in an inventory, it is removed from that inventory.
    /// </summary>
    /// <param name="worldRoot">The root of the world to place in. If not provided, `get_window()` is used.</param>
    /// <returns></returns>
    public InstanceError PutInWorld3D(Node worldRoot)
    {
        // If the item is already in a world, EXIT EARLY
        if (InSpace is SpaceState.World2D or SpaceState.World3D) return InstanceError.AlreadyExists;
        // If we don't have any way to represent this item in a 3D world, EXIT EARLY
        if (Descriptor.WorldItem3DScene == null) return InstanceError.SceneMissing;
        
        // Use the window as world root, if not provided
        worldRoot ??= GetWindow();
        
        // Remove ourselves from an inventory if we're in one
        RemoveFromInventory();
        
        // Spawn and set up the new 3D world item
        var worldItem = Descriptor.WorldItem3DScene.Instantiate<WorldItem3D>();
        worldItem.Setup(this);
        worldRoot.AddChild(worldItem);
        WorldItem3D = worldItem;
        
        // Reparent us to the world item to make it easier to visualize where this item is in the editor.
        ChangeParent(WorldItem3D);

        InSpace = SpaceState.World3D;
        return InstanceError.Ok;
    }

    /// <summary>
    /// Places this item instance in an inventory. If it was in the world, it is removed from the game world. If it was in a different inventory, it is removed from that inventory.
    /// </summary>
    /// <param name="inventory">The inventory to place this item instance inside.</param>
    /// <param name="slot">OPTIONAL. The specific slot of the inventory to place the item instance in.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public InstanceError PutInInventory(ItemInventory inventory, int slot = -1)
    {
        ItemInventory.InventoryError error;
        
        // If we should just put this item ANYWHERE in the inventory, then try to push the item in
        if (slot < 0)
        {
            error = inventory.PushItem(this);
        }
        // If we should put this item in a specific slot of the inventory, then try to put the item into the specific slot
        else
        {
            error = inventory.PutItemInSlot(slot, this);
        }
        
        // Handle any errors
        switch (error)
        {
            case ItemInventory.InventoryError.Ok:
                break;
                
            case ItemInventory.InventoryError.FilterDenied:
                return InstanceError.FilterDenied;
            case ItemInventory.InventoryError.NoFit:
                return InstanceError.NoFit;
            case ItemInventory.InventoryError.DifferentTypes:
                return InstanceError.DifferentTypes;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // Reparent us to the inventory to make it easier to visualize where the item is in the editor.
        RemoveFromInventory();
        RemoveFromWorld();
        ChangeParent(inventory);

        InSpace = SpaceState.Inventory;
        ParentInventory = inventory;
        FreeIfEmpty();
        return InstanceError.Ok;
    }

    /// <summary>
    /// Places this item back into a limbo state, existing mostly just in memory. If it was in the world, it is removed from the game world. If it was in an inventory, it is removed from that inventory.
    /// </summary>
    public void PutNowhere()
    {
        RemoveFromWorld();
        RemoveFromInventory();
        InSpace = SpaceState.Nowhere;
        ChangeParent(null);
    }
    
    //
    //  Godot Methods
    //

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what != NotificationPredelete) return;
        
        // When this object is free'd, remove it from existing anywhere
        PutNowhere();
        EmitSignal(SignalName.ItemFreed);
    }
    
    //
    //  Internal Methods
    //

    /// <summary>
    /// Setup this item. Intended to be called by the ItemDescriptor that this item belongs to.
    /// </summary>
    /// <param name="descriptor">The descriptor that this item instance belongs to.</param>
    internal void Setup(ItemDescriptor descriptor)
    {
        Descriptor = descriptor;
        
        // Spawn the scripting for this item as a child
        if (Descriptor.ItemScriptScene != null)
        {
            ItemScript = Descriptor.ItemScriptScene.Instantiate<ItemScriptBase>();
            AddChild(ItemScript);
            ItemScript.Setup(this);
        }

        PutNowhere();
    }
    
    //
    //  Private Methods
    //

    /// <summary>
    /// Remove this instance from its parent inventory, if it's in one.
    /// </summary>
    /// <returns>`true` if it was removed from an inventory. `false` if there was nothing to remove it from</returns>
    private bool RemoveFromInventory()
    {
        if (ParentInventory == null) return false;

        var slotIndex = ParentInventory.IndexOf(this);
        if (slotIndex >= 0)
        {
            ParentInventory.TakeItemFromSlot(slotIndex);
        }

        ParentInventory = null;
        InSpace = SpaceState.Nowhere;
        return slotIndex >= 0;
    }

    /// <summary>
    /// Remove this instance from the game world, if it's placed there.
    /// </summary>
    /// <returns>`true` if it was removed from the world, and the WorldItem was free'd. `false` if there was no WorldItem, and we're not in the game world.</returns>
    private bool RemoveFromWorld()
    {
        var actuallyRemoved = false;
        
        // Remove our 2D world item, if there is one
        if (WorldItem2D != null)
        {
            WorldItem2D.QueueFree();
            WorldItem2D = null;
            actuallyRemoved = true;
        }
        // Remove our 3D world item, if there is one
        if (WorldItem3D != null)
        {
            WorldItem3D.QueueFree();
            WorldItem3D = null;
            actuallyRemoved = true;
        }

        InSpace = SpaceState.Nowhere;
        return actuallyRemoved;
    }
    
    private void ChangeParent(Node newParent)
    {
        if (GetParent() != null)
        {
            if (newParent == null)
            {
                GetParent().RemoveChild(this);
            }
            else
            {
                Reparent(newParent);
            }
        }
        else if (newParent != null)
        {
            newParent.AddChild(this);
        }
    }

    /// <summary>
    /// Queues freeing this instance ONLY if there are no items left in this stack.
    /// </summary>
    private void FreeIfEmpty()
    {
        if (StackSize > 0) return;
        QueueFree();
    }
}