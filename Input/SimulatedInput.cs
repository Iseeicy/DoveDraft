using System;
using System.Collections.Generic;
using Godot;

namespace DoveDraft.Input;

/// <summary>
/// Simulates inputs as an EntityInput. This means that you can write code triggering input events for things that would use EntityInput. This is super helpful for things like NPCs, which may want to trigger one-shot inputs without having to manually manipulate down / up events.
/// </summary>
[GlobalClass, Icon("../Icons/simulated_input.png")]
public partial class SimulatedInput : EntityInput
{
    //
    //  Private Methods
    //

    /// <summary>
    /// The simulated states to maintain. This stores states by their ticktype for easier access.
    /// </summary>
    private readonly SimulatedInputStates _simulatedStates = new();

    //
    //  Entity Input Methods
    //
    
    public override void GatherInputs(TickType tick)
    {
        base.GatherInputs(tick);
        
        // Move the current input states forward.
        ProgressStates(_simulatedStates.GetButtonStates(tick));
        
        // Apply the latest states to our current state dict, if there are any
        if (_simulatedStates.GetQueuedRawStates(tick).Count > 0)
        {
            // Pop the first item from rawStatesQueue
            var rawStatesQueue = _simulatedStates.GetQueuedRawStates(tick);
            SimulatedInputStates.RawStatesDictionary nextRawStates = rawStatesQueue[0];
            rawStatesQueue.RemoveAt(0);
            
            ApplyRawStates(_simulatedStates.GetButtonStates(tick), nextRawStates);
        }
        
        // Register our current inputs
        foreach ((var actionName, InputState state) in _simulatedStates.GetButtonStates(tick)) { RegisterInput(actionName, state); }
        
        // Register our analog values
        foreach (var (actionName, value) in _simulatedStates.GetAnalogStates(tick)) { RegisterAnalogInput(actionName, value); }
        
        // Clean up analog values. These should not persist between ticks.
        _simulatedStates.GetAnalogStates(tick).Clear();
    }
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Simulate quickly pressing a button and then releasing it. Marks the input as down, and then up on the next frame.
    /// </summary>
    /// <param name="actionName">The name of the Input Action to simulate.</param>
    public void SimulateActionOneShot(string actionName)
    {
        foreach (TickType tickType in Enum.GetValues<TickType>()) { AgnosticSimulateActionOneShot(_simulatedStates.GetQueuedRawStates(tickType), actionName); }
    }

    /// <summary>
    /// Simulate pressing a button down or releasing a button.
    /// </summary>
    /// <param name="actionName">The name of the Input Action to simulate.</param>
    /// <param name="isDown">Should we simulate the button being held down? Or released?</param>
    public void SimulateAction(string actionName, bool isDown)
    {
        foreach (TickType tickType in Enum.GetValues<TickType>()) { AgnosticSimulateAction(_simulatedStates.GetQueuedRawStates(tickType), actionName, isDown); }
    }

    /// <summary>
    /// Simulate some analog input's strength.
    /// </summary>
    /// <param name="actionName">The name of the Input Action to simulate analog values for.</param>
    /// <param name="strength">The strength of analog force to simulate. Should be between 0 and 1 inclusive.</param>
    public void SimulateAnalog(string actionName, float strength)
    {
        foreach (TickType tickType in Enum.GetValues<TickType>()) { AgnosticSimulateAnalog(_simulatedStates.GetAnalogStates(tickType), actionName, strength); }
    }

    /// <summary>
    /// Simulate some 1d analog axis.
    /// </summary>
    /// <param name="axis">The axis to simulate an input for.</param>
    /// <param name="value">The value to assign to the axis. Should be between -Infinity and Infinity.</param>
    public void SimulateAxis1D(InputAxis1D axis, float value)
    {
        if (value > 0)
        {
            SimulateAnalog(axis.PositiveActionName, value);
            SimulateAnalog(axis.NegativeActionName, 0);
        }
        else
        {
            SimulateAnalog(axis.PositiveActionName, 0);
            SimulateAnalog(axis.NegativeActionName, value);
        }
    }

    /// <summary>
    /// Simulate some 2d analog axis.
    /// </summary>
    /// <param name="axis">The axis to simulate and input for.</param>
    /// <param name="value">The value to assign to the axis.</param>
    public void SimulateAxis2D(InputAxis2D axis, Vector2 value)
    {
        SimulateAxis1D(axis.X, value.X);
        SimulateAxis1D(axis.Y, value.Y);
    }
    
    //
    //  Private Static Methods
    //

    private static void AgnosticSimulateActionOneShot(List<SimulatedInputStates.RawStatesDictionary> queuedRawStates, string actionName)
    {
        // If there's not enough queue'd ticks of action for the next tick and the tick after, add the dictionaries for them!
        while (queuedRawStates.Count < 2)
        {
            queuedRawStates.Add(new SimulatedInputStates.RawStatesDictionary());
        }
        
        // In the next tick, queue that our input is to be processed. In the tick AFTER the next tick, queue that our input is to be released.
        queuedRawStates[0][actionName] = true;
        queuedRawStates[1][actionName] = false;
    }

    private static void AgnosticSimulateAction(List<SimulatedInputStates.RawStatesDictionary> queuedRawStates, string actionName,
        bool isDown)
    {
        // If there's nothing in the queue of states for the next tick, make a dictionary for it!
        if(queuedRawStates.Count < 1) queuedRawStates.Add(new SimulatedInputStates.RawStatesDictionary());
        
        // In the next tick, queue our action
        queuedRawStates[0][actionName] = isDown;
    }

    private static void AgnosticSimulateAnalog(SimulatedInputStates.AnalogStateDictionary analogStates, string actionName,
        float strength)
    {
        analogStates[actionName] = strength;
    }

    /// <summary>
    /// Go through a dictionary of input states and progress inputs. Keys with JustDown will become Pressed. Keys with JustUp will be removed.
    /// </summary>
    /// <param name="buttonStates">The dictionary containing input states by their action name.</param>
    private static void ProgressStates(SimulatedInputStates.InputStateDictionary buttonStates)
    {
        var toPressed = new List<string>();
        var toRemove = new List<string>();
        
        // Go through the current state and progress any inputs
        foreach ((var actionName, InputState state) in buttonStates)
        {
            // If this was just down, now it's pressed
            if ((state & InputState.JustDown) == InputState.JustDown) toPressed.Add(actionName);
            // If this was just up, now it shouldn't be here
            if ((state & InputState.JustDown) == InputState.JustDown) toRemove.Add(actionName);
        }
        
        // Actually press the states that were marked above
        foreach (var actionNameToPress in toPressed)
        {
            buttonStates[actionNameToPress] = InputState.Pressed;
        }
        
        // Actually remove the states that were marked above
        foreach (var actionNameToRemove in toRemove)
        {
            buttonStates.Remove(actionNameToRemove);
        }
    }

    /// <summary>
    /// Apply the queued raw states to some given input states, making sure to update stuff like JustDown and JustUp correctly.
    /// </summary>
    /// <param name="buttonStates">The current states of the buttons, to be modified.</param>
    /// <param name="rawStates">The raw states to apply to buttonStates</param>
    private static void ApplyRawStates(SimulatedInputStates.InputStateDictionary buttonStates,
        SimulatedInputStates.RawStatesDictionary rawStates)
    {
        // If there's nothing queued, EXIT EARLY
        if (rawStates == null || rawStates.Count == 0) return;

        var toRemove = new List<string>();
        var toSet = new Dictionary<string, InputState>();
        
        foreach (var (actionName, shouldBePressed) in rawStates)
        {
            InputState currentState = buttonStates[actionName];

            // Calculate what the next should be given our current state and desired button press state.
            InputState nextState = CalculateNextInputState(currentState, shouldBePressed);
            
            // If this action doesn't have a state anymore (it's unpressed), then mark it to be removed.
            if (nextState == InputState.None)
            {
                toRemove.Add(actionName);
            }
            // If the action DOES have a state, mark it to be applied.
            else
            {
                toSet[actionName] = nextState;
            }
        }
        
        // Actually remove inputs as marked above
        foreach (var actionName in toRemove) { buttonStates.Remove(actionName); }
        
        // Actually set inputs as marked above
        foreach ((var actionName, InputState nextState) in toSet) { buttonStates[actionName] = nextState; }
    }

    private static InputState CalculateNextInputState(InputState currentState, bool shouldBePressed)
    {
        // If we don't have a state for this yet...
        if (currentState == InputState.None) return shouldBePressed ? InputState.JustDown : InputState.None;
        
        // If this button was pressed and isn't anymore...
        if ((currentState & InputState.PressedOrJustDown) != InputState.None && !shouldBePressed)
            return InputState.JustUp;

        // If the button wasn't pressed and IS now...
        if ((currentState & InputState.PressedOrJustDown) == InputState.None && shouldBePressed)
            return InputState.JustDown;

        // OTHERWISE - the state stays
        return currentState;
    }
}