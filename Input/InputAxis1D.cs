using Godot;

namespace DoveDraft.Input;

[Tool, GlobalClass]
public partial class InputAxis1D : RefCounted
{
    //
    //  Public Variables
    //

    /// <summary>
    /// The name of the action that, when pressed all the way, pulls this axis as far negative as possible.
    /// </summary>
    public string NegativeActionName { get; set; } = "";
    
    /// <summary>
    /// The name of the action that, when pressed all the way, pushes this axis as far positive as possible.
    /// </summary>
    public string PositiveActionName { get; set; } = "";
    
    //
    //  Godot Methods
    //

    public InputAxis1D() { }
    
    public InputAxis1D(string negativeActionName, string positiveActionName)
    {
        NegativeActionName = negativeActionName;
        PositiveActionName = positiveActionName;
    }
}