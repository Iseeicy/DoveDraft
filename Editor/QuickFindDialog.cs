using Godot;

namespace DoveDraft.Editor;

[Tool]
public partial class QuickFindDialog : ConfirmationDialog
{
    //
    //  Exports
    //

    /// <summary>
    /// Emitted when the path to a resource has been confirmed / picked.
    /// </summary>
    [Signal]
    public delegate void ConfirmedPathEventHandler(string path);
    
    /// <summary>
    /// The filter to use for this dialog's resource list.
    /// </summary>
    [Export]
    public ResourceSearchFilter Filter { get; set; }
    
    //
    //  Public Variables
    //

    /// <summary>
    /// The currently selected path. This isn't necessarily the confirmed path, until the `ConfirmedPath` signal is called.
    /// </summary>
    public string SelectedPath { get; private set; } = "";
    
    //
    //  Private Variables
    //

    /// <summary>
    /// The search list used to display all resource options.
    /// </summary>
    private ResourceSearchList _searchList;
    
    //
    //  Godot Methods
    //

    public override async void _Ready()
    {
        Confirmed += OnConfirmed;
        GetOkButton().Disabled = true;
        
        // # Wait for the parent node of this to be ready. We do this so parent nodes have a chance to override our search_list_scene variable :3
        await ToSignal(GetParent(), "ready");

        _searchList = GetNode<ResourceSearchList>("VBoxContainer/ResourceSearchList");
        _searchList.Filter = Filter;
        _searchList.ItemSelected += OnResourceSearchListItemSelected;
        _searchList.ScanFileSystem();
    }
    
    //
    //  Private Methods
    //

    private void OnLineEditTextChanged(string newText)
    {
        _searchList.SearchQuery = newText;
    }
    
    private void OnConfirmed()
    {
        EmitSignal(SignalName.ConfirmedPath, SelectedPath);
    }

    private void OnResourceSearchListItemSelected(long index)
    {
        GetOkButton().Disabled = false;
        SelectedPath = _searchList.GetItemText((int)index);
    }
}