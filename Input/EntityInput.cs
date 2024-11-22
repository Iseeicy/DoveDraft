using System;
using System.Collections.Generic;
using Godot;

namespace DoveDraft.Input;

/// <summary>
/// Used to manage gathering inputs from some arbitrary source. Intended to be extended by `PlayerInput` and `SimulatedInput`.
/// </summary>
[Tool, GlobalClass, Icon("../Icons/entity_input.png")]
public partial class EntityInput : Node
{
    //
    //  Enums
    //

    /// <summary>
    /// All the possible states than input can be simultaneously. Used as bitflags.
    /// </summary>
    [Flags]
    public enum InputState
    {
        None = 0,
        JustDown = 1 << 0,
        Pressed = 1 << 1,
        JustUp = 1 << 2,
        PressedOrJustDown = 1 << 1 | 1 << 0,
    }

    /// <summary>
    /// What kind of tick to reference.
    /// </summary>
    public enum TickType
    {
        Process = 0,
        ProcessPhysics = 1,
    }
    
    //
    //  Private Variables
    //

    /// <summary>
    /// The inputs that this entity is providing on the current frame.
    /// </summary>
    private Dictionary<string, InputState> _inputs = new();

    /// <summary>
    /// The analog inputs that this entity is providing on the current frame.
    /// </summary>
    private Dictionary<string, float> _analogInputs = new();
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Has the given action just STARTED being pressed on this frame?
    /// </summary>
    /// <param name="actionName">The name of the action to check</param>
    /// <returns>`true` if the given action was found and has just been started being pressed on this frame. `false` if the given action is not pressed.</returns>
    public bool IsActionJustPressed(string actionName)
    {
        InputState state = _inputs.GetValueOrDefault(actionName, InputState.None);
        return (state & InputState.JustDown) != InputState.None;
    }
    
    /// <summary>
    /// Is the given action actively being pressed / held down?
    /// </summary>
    /// <param name="actionName">The name of the action to check</param>
    /// <returns>`true` if the given action was found and is actively being pressed. May have been held down for many frames previously. `false` if the given action is not pressed.</returns>
    public bool IsActionPressed(string actionName)
    {
        InputState state = _inputs.GetValueOrDefault(actionName, InputState.None);
        return (state & InputState.PressedOrJustDown) != InputState.None;
    }
    
    /// <summary>
    /// Has the given action just STOPPED being pressed on this frame?
    /// </summary>
    /// <param name="actionName">The name of the action to check</param>
    /// <returns>`true` if the given action was found and was just been released on this frame. `false` if the given action is pressed.</returns>
    public bool IsActionJustReleased(string actionName)
    {
        InputState state = _inputs.GetValueOrDefault(actionName, InputState.None);
        return (state & InputState.JustUp) != InputState.None;
    }

    /// <summary>
    /// Get the value of some analog input on this frame.
    /// </summary>
    /// <param name="actionName">The name of the analog input to read.</param>
    /// <returns>A value between 0 and Infinity. If the action can't be found, this will always be 0.</returns>
    public float GetAnalog(string actionName) => _analogInputs.GetValueOrDefault(actionName, 0.0f);

    /// <summary>
    /// Reads the value of some 1D axis.
    /// </summary>
    /// <param name="axis">The definition of the axis to read.</param>
    /// <returns>A value from -Infinity to Infinity, inclusive.</returns>
    public float ReadAxis1D(InputAxis1D axis) => GetAnalog(axis.PositiveActionName) - GetAnalog(axis.NegativeActionName);

    /// <summary>
    /// Reads the value of some 2D axis.
    /// </summary>
    /// <param name="axis">The definition of the axis to read.</param>
    /// <returns>A Vector2 where X and Y are values from -Infinity to Infinity, inclusive.</returns>
    public Vector2 ReadAxis2D(InputAxis2D axis) => new Vector2(ReadAxis1D(axis.X), ReadAxis1D(axis.Y));

    /// <summary>
    /// Gathers all inputs from our input source, and clears previously gathered inputs. This should be called on the beginning of each _process tick & _physics_process tick by the node that uses this class.
    /// </summary>
    /// <param name="tick">What tick is this called on? _process or _physics_process?</param>
    public virtual void GatherInputs(TickType tick)
    {
        SweepInputs();
    }
    
    //
    //  Protected Methods
    //

    /// <summary>
    /// Marks that an button input event of some kind has happened on this frame.
    /// </summary>
    /// <param name="actionName">The name of the action to store an input for.</param>
    /// <param name="stateFlag">The bitflag values of the input state to set for the given action.</param>
    protected void RegisterInput(string actionName, InputState stateFlag)
    {
        // Use bitwise operations to merge the registered flag into the existing input state.
        InputState state = _inputs.GetValueOrDefault(actionName, InputState.None);
        _inputs[actionName] = state | stateFlag;
    }

    /// <summary>
    /// Marks that an analog input event of some kind has happened on this frame.
    /// </summary>
    /// <param name="actionName">The name of the analog action to store a strength for.</param>
    /// <param name="strength">The strength of the given input.</param>
    protected void RegisterAnalogInput(string actionName, float strength)
    {
        _analogInputs[actionName] = strength;
    }
    
    //
    //  Private Methods
    //

    /// <summary>
    /// Removes all saved input events from this frame. This should be performed at the end of each frame / tick to ensure that multiple inputs don't build up between frames.
    /// </summary>
    private void SweepInputs()
    {
        _inputs.Clear();
        _analogInputs.Clear();
    }
}