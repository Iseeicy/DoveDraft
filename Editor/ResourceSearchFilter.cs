using Godot;

namespace DoveDraft.Editor;

[Tool, GlobalClass]
public partial class ResourceSearchFilter : Resource
{
    /// <summary>
    /// Should the given resource pass through the filter?
    /// </summary>
    /// <param name="path">The absolute path of the resource.</param>
    /// <param name="resource">The temporarily loaded resource itself.</param>
    /// <returns>true if the resource passes through the filter, false otherwise.</returns>
    public virtual bool ShouldResourceBeIncluded(string path, Resource resource)
    {
        return resource != null;
    }
}