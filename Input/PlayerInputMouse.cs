using System;
using Godot;

namespace DoveDraft.Input;

/// <summary>
/// A node representation of some configurable mouse movement action. Making this node based means we can allow a designer in Godot to define what Input Actions should be listened for IN EDITOR, rather than modifying code. The name of this node should be the name of the Input Action to use.
///
/// Thanks a ton to https://yosoyfreeman.github.io/ for the amazing insights on attaining good mouselook in Godot!
/// </summary>
[Tool, GlobalClass, Icon("../Icons/player_input.png")]
public partial class PlayerInputMouse : Node
{
    //
    //  Exports
    //

    /// <summary>
    /// The name of the analog action to use when looking up.
    /// </summary>
    [Export] public string UpActionName { get; set; } = DoveDraftInputs.Player.Look.Up;

    /// <summary>
    /// The name of the analog action to use when looking down.
    /// </summary>
    [Export] public string DownActionName { get; set; } = DoveDraftInputs.Player.Look.Down;

    /// <summary>
    /// The name of the analog action to use when looking left.
    /// </summary>
    [Export] public string LeftActionName { get; set; } = DoveDraftInputs.Player.Look.Left;

    /// <summary>
    /// The name of the analog action to use when looking right.
    /// </summary>
    [Export] public string RightActionName { get; set; } = DoveDraftInputs.Player.Look.Right;

    /// <summary>
    /// Should we ignore mouse events if the mouse is not captured by the window?
    /// </summary>
    [Export] public bool IgnoreWhenUnCaptured { get; set; } = true;
    
    //
    //  Private Variables
    //
    
    private Vector2 _accumulatedProcessInput = Vector2.Zero;
    private Vector2 _accumulatedPhysicsInput = Vector2.Zero;
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Read the accumulated mouse movement values.
    /// </summary>
    /// <param name="tickType">The tick that this is being called from.</param>
    /// <returns>The accumulated mouse movement values for the given TickType.</returns>
    public Vector2 ReadAccumulated(EntityInput.TickType tickType) => tickType switch
    {
        EntityInput.TickType.Process => _accumulatedProcessInput,
        EntityInput.TickType.ProcessPhysics => _accumulatedPhysicsInput,
        _ => Vector2.Zero
    };

    /// <summary>
    /// Clear any accumulated mouse movement values.
    /// </summary>
    /// <param name="tickType">The tick that this is being called from.</param>
    public void ClearAccumulated(EntityInput.TickType tickType)
    {
        switch (tickType)
        {
            case EntityInput.TickType.Process:
                _accumulatedProcessInput = Vector2.Zero;
                break;
            case EntityInput.TickType.ProcessPhysics:
                _accumulatedPhysicsInput = Vector2.Zero;
                break;
            
            default:
                break;
        }
    }
    
    //
    //  Godot Methods
    //

    public override void _Input(InputEvent @event)
    {
        // If the mouse isn't captured, ignore this event
        if (Godot.Input.MouseMode != Godot.Input.MouseModeEnum.Captured && IgnoreWhenUnCaptured) return;
        
        // If the mouse moved, store this movement to process later
        if (@event is InputEventMouseMotion mouseEvent)
        {
            // Get the viewport and use that to scale the input correctly
            Vector2 transformedRelative = ((InputEventMouseMotion)mouseEvent.XformedBy(GetTree().Root.GetFinalTransform()))
                .Relative;

            _accumulatedPhysicsInput += transformedRelative;
            _accumulatedPhysicsInput += transformedRelative;
        }
    }
}