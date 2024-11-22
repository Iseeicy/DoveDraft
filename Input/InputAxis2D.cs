using Godot;

namespace DoveDraft.Input;

[Tool, GlobalClass]
public partial class InputAxis2D : RefCounted
{
    //
    //  Public Variables
    //

    /// <summary>
    /// The x-axis of this 2D axis.
    /// </summary>
    public InputAxis1D X { get; set; } = new();
    
    /// <summary>
    /// The y-axis of this 2D axis.
    /// </summary>
    public InputAxis1D Y { get; set; } = new();
    
    //
    //  Godot Methods
    //

    public InputAxis2D() { }
    
    public InputAxis2D(InputAxis1D x, InputAxis1D y)
    {
        X = x;
        Y = y;
    }
}