using Godot;

namespace DoveDraft.Character;

[Tool, GlobalClass]
public partial class CharacterScript : Node
{
    //
    //  Public Variables
    //
    
    /// <summary>
    /// The CharacterAgent that this script belongs to. Can be `CharacterAgent3D`, `CharacterAgent2D`, or null.
    /// </summary>
    public Node Agent => Agent3D != null ? Agent3D : Agent2D;

    /// <summary>
    /// The 3D agent that we belong to, if there is one.
    /// </summary>
    public CharacterAgent3D Agent3D => Owner as CharacterAgent3D;

    /// <summary>
    /// The 2D agent that we belong to, if there is one.
    /// </summary>
    public CharacterAgent2D Agent2D => Owner as CharacterAgent2D;

    /// <summary>
    /// The script runner that belongs to our parent agent, if there is one.
    /// </summary>
    public CharacterScriptRunner ScriptRunner => Agent switch
    {
        CharacterAgent2D agent2d => agent2d.ScriptRunner,
        CharacterAgent3D agent3d => agent3d.ScriptRunner,
        _ => null
    };

    /// <summary>
    /// Is this character script executing?
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            SetProcess(value);
            SetPhysicsProcess(value);
            SetProcessUnhandledInput(value);
            SetBlockSignals(!value);
        }
    }
    private bool _isRunning = true;
    
    //
    //  Public Methods
    //

    public void CallAgentReady()
    {
        AgentReady();
    }

    public void CallAgentProcess(double delta)
    {
        if(IsRunning) AgentProcess(delta);
    }

    public void CallAgentPhysicsProcess(double delta)
    {
        if (IsRunning) AgentPhysicsProcess(delta);
    }
    
    //
    //  Protected Methods
    //

    protected virtual void AgentReady() { }
    
    protected virtual void AgentProcess(double delta) { }
    
    protected virtual void AgentPhysicsProcess(double delta) { }
}
