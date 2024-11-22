using System;
using System.Collections.Generic;
using Godot;

namespace DoveDraft.Input;

[GlobalClass, Icon("../Icons/player_input.png")]
public partial class PlayerInput : EntityInput
{
    //
    //  Private Variables
    //

    /// <summary>
    /// All known input actions underneath us.
    /// </summary>
    private PlayerInputAction[] _inputActions = Array.Empty<PlayerInputAction>();
    
    /// <summary>
    /// All known analog inputs underneath us.
    /// </summary>
    private PlayerInputAnalog[] _inputAnalog = Array.Empty<PlayerInputAnalog>();

    /// <summary>
    /// All known mouse inputs underneath us.
    /// </summary>
    private PlayerInputMouse[] _inputMice = Array.Empty<PlayerInputMouse>();
    
    //
    //  Entity Input Methods
    //

    public override void GatherInputs(TickType tick)
    {
        base.GatherInputs(tick);
        
        // Go through all the inputs stored in `_inputActions` and register them as our `EntityInput` inputs.
        foreach (PlayerInputAction input in _inputActions)
        {
            InputState state = input.GetInputState();
            if(state != InputState.None) RegisterInput(input.Name, state);
        }

        foreach (PlayerInputAnalog analog in _inputAnalog)
        {
            RegisterAnalogInput(analog.Name, analog.GetAnalogStrength());
        }

        foreach (PlayerInputMouse mouse in _inputMice)
        {
            Vector2 value = mouse.ReadAccumulated(tick);

            if (value.Y > 0)
            {
                RegisterAnalogInput(mouse.UpActionName, value.Y);
            }
            else
            {
                RegisterAnalogInput(mouse.DownActionName, -value.Y);
            }

            if (value.X > 0)
            {
                RegisterAnalogInput(mouse.RightActionName, value.X);
            }
            else
            {
                RegisterAnalogInput(mouse.LeftActionName, -value.X);
            }
            
            mouse.ClearAccumulated(tick);
        }
    }
    
    //
    //  Godot Methods
    //

    public override void _Ready()
    {
        // Recurse through child nodes to get all input action nodes
        _inputActions = FindChildNodesOfType<PlayerInputAction>(this).ToArray();
        _inputAnalog = FindChildNodesOfType<PlayerInputAnalog>(this).ToArray();
        _inputMice = FindChildNodesOfType<PlayerInputMouse>(this).ToArray();
    }
    
    //
    //  Private Methods
    //

    private List<T> FindChildNodesOfType<T>(Node startNode) where T : Node
    {
        var results = new List<T>();
        FindChildNodesOfType<T>(startNode, results);
        return results;
    }
    
    private void FindChildNodesOfType<T>(Node startNode, List<T> results) where T : Node
    {
        if(startNode is T correctTypeStart) results.Add(correctTypeStart);

        // Recurse
        foreach (Node child in startNode.GetChildren()) { FindChildNodesOfType<T>(child, results); }
    }
}