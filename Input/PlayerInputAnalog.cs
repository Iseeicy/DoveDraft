using System;
using Godot;

namespace DoveDraft.Input;

/// <summary>
/// A node representation of some configurable player input action. Making this node based means we can allow a designer in Godot to define what Input Actions should be listened for IN EDITOR, rather than modifying code. The name of this node should be the name of the Input Action to use.
/// </summary>
[Tool, GlobalClass, Icon("../Icons/player_input_analog.png")]
public partial class PlayerInputAnalog : PlayerInputAction
{
    //
    //  Public Methods
    //

    /// <summary>
    /// Get the analog strength for this input.
    /// </summary>
    /// <returns>A value between 0 and 1, inclusive. If this isn't in the action map, returns `0`.</returns>
    public float GetAnalogStrength() => InputMap.HasAction(Name) ? Godot.Input.GetActionStrength(Name) : 0;
}