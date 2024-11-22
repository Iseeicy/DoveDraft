using Godot;
using Godot.Collections;

namespace DoveDraft.Character;

[Tool, GlobalClass]
public partial class CharacterScriptRunner : Node
{
    //
    //  Public Variables
    //

    /// <summary>
    /// The scripts to run. These are the children of this node.
    /// </summary>
    public Array<CharacterScript> Scripts { get; private set; } = new();
    
    //
    //  Public Methods
    //

    public void Setup()
    {
        Scripts.Clear();
        FindScriptsInChildren(Scripts);
    }

    public void ScriptsReady()
    {
        foreach (CharacterScript script in Scripts)
        {
            script.CallAgentReady();
        }
    }

    public void ScriptsProcess(double delta)
    {
        foreach (CharacterScript script in Scripts)
        {
            script.CallAgentProcess(delta);
        }
    }
    
    public void ScriptsPhysicsProcess(double delta)
    {
        foreach (CharacterScript script in Scripts)
        {
            script.CallAgentPhysicsProcess(delta);
        }
    }
    
    //
    //  Private Methods
    //

    /// <summary>
    /// Sort through the top level child nodes and compile a list of scripts.
    /// </summary>
    /// <param name="results">Where to store the found CharacterScripts.</param>
    private void FindScriptsInChildren(Array<CharacterScript> results)
    {
        foreach (Node node in GetChildren())
        {
            if(node is CharacterScript script) results.Add(script);
        }
    }
}
