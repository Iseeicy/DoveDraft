using DoveDraft.Editor;
using Godot;

namespace DoveDraft.Character;

[Tool, GlobalClass]
public partial class CharacterDefinitionSearchFilter : ResourceSearchFilter
{
    public override bool ShouldResourceBeIncluded(string path, Resource resource)
    {
        return resource is CharacterDefinition;
    }
}