using System.Linq;
using Godot;

namespace DoveDraft.Editor;

[Tool, GlobalClass, Icon("../Icons/resource_field.png")]
public partial class ResourceField : HBoxContainer
{
    //
    //  Exports
    //

    [Signal]
    public delegate void TargetResourceUpdatedEventHandler(Resource resource);

    /// <summary>
    /// The filter to use when sorting through what resources this field allows.
    /// </summary>
    [Export]
    public ResourceSearchFilter Filter { get; set; }
    
    //
    //  Public Variables
    //

    public Resource TargetResource
    {
        get => _targetResource;
        set
        {
            if (value == _targetResource) return;
            
            _targetResource = value;
            OurMenuButton.Text = value == null ? "<empty>" : value.ResourcePath.Split('/').LastOrDefault();;
            OurMenuButton.GetPopup().SetItemDisabled(ClearId, value == null);
            EmitSignal(SignalName.TargetResourceUpdated, value);
        }
    }
    private Resource _targetResource;
    
    //
    //  Private Variables
    //
    
    private const int QuickFindId = 0;
    private const int LoadId = 1;
    private const int ClearId = 2;

    private MenuButton OurMenuButton => GetNode<MenuButton>("MenuButton");
    private QuickFindDialog OurQuickFindDialog => GetNode<QuickFindDialog>("QuickFindDialog");

    //
    //  Godot Methods
    //

    public override void _Ready()
    {
        OurMenuButton.GetPopup().IdPressed += OnMenuButtonIdPressed;
        OurQuickFindDialog.Filter = Filter;
    }
    
    //
    //  Public Methods
    //

    private void OnMenuButtonIdPressed(long id)
    {
        switch (id)
        {
            case QuickFindId:
                OurQuickFindDialog.Show();
                return;
            case ClearId:
                TargetResource = null;
                return;
        }
    }

    private void OnArrowButtonPressed() => OurMenuButton.ShowPopup();

    private void OnQuickFindDialogConfirmedPath(string path) => TargetResource = GD.Load(path);
}