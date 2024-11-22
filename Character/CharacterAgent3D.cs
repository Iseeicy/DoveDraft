using Godot;
using Godot.Collections;

namespace DoveDraft.Character;

[Tool, GlobalClass]
public partial class CharacterAgent3D : CharacterBody3D
{
    //
    //  Exports
    //

    /// <summary>
    /// OPTIONAL. The character that this agent represents. If not assigned, a default value will be assigned.
    /// </summary>
    [Export]
    public CharacterDefinition Character { get; set; }

    /// <summary>
    /// The player scripts that define this agent's behaviour.
    /// </summary>
    [Export] public Array<PackedScene> AgentScripts { get; set; } = new();
    
    // TODO - port entity input system
}