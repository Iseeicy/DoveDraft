using Godot;
using Godot.Collections;

namespace DoveDraft.Item;

/// <summary>
/// Represents an item's view model, which acts as a visual representation of an ItemInstance when it is currently being held by a player. Has an identical API to ItemViewModel3D - but this is a 3D node instead.
/// </summary>
[GlobalClass]
public partial class ItemViewModel3D : Node3D
{
    //
    //  Constants
    //

    private const string ItemReadyParam = "parameters/item_ready/request";
    
    //
    //  Exports
    //
    
    /// <summary>
    /// Emitted when this node is being freed.
    /// </summary>
    [Signal]
    public delegate void ViewModelFreedEventHandler();
    
    /// <summary>
    /// The animation tree used to control this view model.
    /// </summary>
    [Export] public AnimationTree AnimationTree { get; set; }
    
    //
    //  Public Varaibles
    //

    public ItemInstance ParentInstance { get; private set; }
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Tells the view model that it is on screen and ready to be used.
    /// </summary>
    public void ItemReady()
    {
        if (AnimationTree == null) return;
        AnimationTree.Active = true;
        AnimationTree.Set(ItemReadyParam, (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    
    //
    //  Godot Methods
    //

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what != NotificationPredelete) return;

        // When this object is free'd, tell other nodes (especially ParentInstance) about it
        EmitSignal(SignalName.ViewModelFreed);
    }

    //
    //  Internal Methods
    //

    /// <summary>
    /// Setup this view model. Intended to be called by the ItemInstance that this model belongs to.
    /// </summary>
    /// <param name="item">The ItemInstance that this model belongs to.</param>
    internal void Setup(ItemInstance item)
    {
        ParentInstance = item;
    }
}