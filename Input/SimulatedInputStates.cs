using System;
using System.Collections.Generic;

namespace DoveDraft.Input;

public class SimulatedInputStates
{
    //
    //  Classes
    //

    public class RawStatesDictionary : Dictionary<string, bool> { }
    public class InputStateDictionary : Dictionary<string, EntityInput.InputState> { }
    public class AnalogStateDictionary : Dictionary<string, float> { }
    
    private class State
    {
        /// <summary>
        /// A queue of raw states to be used.
        /// </summary>
        public List<RawStatesDictionary> QueuedRawStates { get; private set; } = new();

        /// <summary>
        /// The current state of inputs to supply when GatherInputs is called.
        /// </summary>
        public InputStateDictionary ButtonStates { get; private set; } = new();

        /// <summary>
        /// The current state of ANALOG input values to supply when GatherInputs is called.
        /// </summary>
        public AnalogStateDictionary AnalogStates { get; private set; } = new();
    }
    
    //
    //  Private Variables
    //

    /// <summary>
    /// Store simulated states by tick type so that we can support using this across the update tick AND the physics tick.
    /// </summary>
    private readonly Dictionary<EntityInput.TickType, State> _simulatedStates = new()
    {
        { EntityInput.TickType.Process, new State() },
        { EntityInput.TickType.ProcessPhysics, new State() },
    };
    
    //
    //  Public Methods
    //

    /// <summary>
    /// Get a queue of raw states to be used.
    /// </summary>
    /// <param name="tickType">The tick type that this is being called from.</param>
    /// <returns>The queue of raw states that belongs to the given tick type.</returns>
    public List<RawStatesDictionary> GetQueuedRawStates(EntityInput.TickType tickType) =>
        _simulatedStates[tickType].QueuedRawStates; 
    
    /// <summary>
    /// Gets the current state of inputs to supply when GatherInputs is called.
    /// </summary>
    /// <param name="tickType">The tick type that this is being called from.</param>
    /// <returns>The current state of inputs that belong to the given tick type.</returns>
    public InputStateDictionary GetButtonStates(EntityInput.TickType tickType) =>
        _simulatedStates[tickType].ButtonStates;

    /// <summary>
    /// Gets the current state of ANALOG input values to supply when GatherInputs is called.
    /// </summary>
    /// <param name="tickType">The tick type that this is being called from.</param>
    /// <returns>The current state of analog inputs that belong to the given tick type.</returns>
    public AnalogStateDictionary GetAnalogStates(EntityInput.TickType tickType) =>
        _simulatedStates[tickType].AnalogStates;
}