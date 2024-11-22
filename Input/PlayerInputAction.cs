using System;
using Godot;

namespace DoveDraft.Input;

/// <summary>
/// A node representation of some configurable player input action. Making this node based means we can allow a designer in Godot to define what Input Actions should be listened for IN EDITOR, rather than modifying code. The name of this node should be the name of the Input Action to use.
/// </summary>
[Tool, GlobalClass, Icon("../Icons/player_input.png")]
public partial class PlayerInputAction : Node
{
    //
    //  Public Methods
    //

    /// <summary>
    /// Get the input state of this action.
    /// </summary>
    /// <returns>The `InputState` of the action this node represents. If this isn't in the action map, returns `InputState.None`.</returns>
    public EntityInput.InputState GetInputState()
    {
        var state = EntityInput.InputState.None;
        
        // If this action doesn't exist, EXIT EARLY
        if (!InputMap.HasAction(Name)) return state;
        
        // Manipulate the state according to actual user input
        if (Godot.Input.IsActionJustPressed(Name)) state = state | EntityInput.InputState.JustDown;
        if (Godot.Input.IsActionPressed(Name)) state = state | EntityInput.InputState.Pressed;
        if (Godot.Input.IsActionJustReleased(Name)) state = state | EntityInput.InputState.JustUp;

        return state;
    }
}