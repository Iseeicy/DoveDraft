using Godot;
using Godot.Collections;

namespace DoveDraft.Item;

[GlobalClass, Icon("../Icons/item_view_model.png")]
public partial class ItemScriptBase : Node
{
    //
    //  Classes
    //

    public partial class ItemInput : RefCounted
    {
        /// <summary>
        /// Is this input JUST being pressed, right now?
        /// </summary>
        public bool JustDown { get; set; }
        
        /// <summary>
        /// Is this input actively being held down?
        /// </summary>
        public bool Pressing { get; set; }
        
        /// <summary>
        /// Is this input JUST being released, right now?
        /// </summary>
        public bool JustUp { get; set; }

        public void Reset()
        {
            JustDown = false;
            Pressing = false;
            JustUp = false;
        }
    }
    
    //
    //  Exports
    //

    /// <summary>
    /// Emitted when this item has been selected.
    /// </summary>
    [Signal]
    public delegate void SelectedStartEventHandler();

    /// <summary>
    /// Emitted when this item has been unselected.
    /// </summary>
    [Signal]
    public delegate void SelectedStopEventHandler();
    
    //
    //  Public Variables
    //

    /// <summary>
    /// The key for the primary "use item" action. Traditionally left mouse. Typically, values are provided in some PlayerScript.
    /// </summary>
    public ItemInput Use0Input { get; set; } = new();
    
    /// <summary>
    /// The key for the secondary "use item" action. Traditionally right mouse. Typically, values are provided in some PlayerScript.
    /// </summary>
    public ItemInput Use1Input { get; set; } = new();
    
    /// <summary>
    /// Is this item currently selected by an `ItemInteractor`?
    /// </summary>
    public bool IsSelected { get; private set; }
    
    /// <summary>
    /// The `ItemInstance` that this script belongs to.
    /// </summary>
    public ItemInstance ParentInstance { get; private set; }
    
    /// <summary>
    /// The `ItemInteractor` that is interacting with this item.
    /// </summary>
    public ItemInteractor Interactor { get; private set; }

    /// <summary>
    /// The `CharacterDefinition` that is using this item currently.
    /// </summary>
    public CharacterDefinition Character => Interactor?.Character;
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Call to trigger `item_selected_start` on subclasses.
    /// </summary>
    /// <param name="parentInteractor">The interactor that's using this item.</param>
    public void CallSelectedStart(ItemInteractor parentInteractor)
    {
        IsSelected = true;
        Interactor = parentInteractor;
        ItemSelectedStart();
        EmitSignal(SignalName.SelectedStart);
    }

    /// <summary>
    /// Call to trigger `item_selected_process` on subclasses.
    /// </summary>
    /// <param name="delta">How much time has passed between calls.</param>
    public void CallSelectedProcess(float delta)
    {
        ItemSelectedProcess(delta);
    }

    /// <summary>
    /// Call to trigger `item_selected_stop` on subclasses.
    /// </summary>
    public void CallSelectedStop()
    {
        IsSelected = false;
        ItemSelectedStop();
        Use0Input.Reset();
        Use1Input.Reset();
        Interactor = null;
        EmitSignal(SignalName.SelectedStop);
    }

    /// <summary>
    /// Trigger a oneshot animation on all viewmodels.
    /// </summary>
    /// <param name="key">The name of the oneshot parameter to trigger. NOT the entire path.</param>
    public void ViewmodelAnimationOneShot(string key)
    {
        ParentInstance.SetViewModelAnimParam($"parameters/{key}/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    /// <summary>
    /// Called when an `ItemInteractor` begins selecting this item. 
    /// </summary>
    public virtual void ItemSelectedStart() { }
    
    /// <summary>
    /// Called each frame that an `ItemInteractor`is selecting this item. 
    /// </summary>
    /// <param name="delta">How much time has passed between calls.</param>
    public virtual void ItemSelectedProcess(float delta) { }
    
    /// <summary>
    /// Called when an `ItemInteractor` stops selecting this item. 
    /// </summary>
    public virtual void ItemSelectedStop() { }

    //
    //  Internal Methods
    //

    internal void Setup(ItemInstance item)
    {
        ParentInstance = item;
    }
}