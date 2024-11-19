using Godot;
using Godot.Collections;

namespace DoveDraft.Item;

/// <summary>
/// The physical representation of an ItemInstance when placed into the game world. Has an identical API to WorldItem2D - but this is a 3D rigidbody instead.
/// </summary>
[GlobalClass, Icon("../Icons/world_item_2d.png")]
public partial class WorldItem3D : RigidBody3D
{
    //
    //  Public Variables
    //

    public bool CanPickup { get; private set; } = true;
    
    public ItemInstance ParentInstance { get; private set; }
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Start the pickup timer, making it so that the player can not pick up this item until timeout. This prevents the player from picking up items as soon as they're dropped.
    /// </summary>
    /// <param name="pickupTimeout">How long the timer should last, in seconds.</param>
    public void StartPickupTimer(float pickupTimeout = 2)
    {
        CanPickup = false;
        GetTree().CreateTimer(pickupTimeout).Timeout += () => { CanPickup = true; };
    }
    
    //
    //  Internal Methods
    //

    /// <summary>
    /// Setup this world item. Intended to be called by the ItemInstance that this world item represents.
    /// </summary>
    /// <param name="item">The ItemInstance that this WorldItem represents.</param>
    internal void Setup(ItemInstance item)
    {
        ParentInstance = item;
    }
}