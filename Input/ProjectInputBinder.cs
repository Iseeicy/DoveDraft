using Godot;
using Godot.Collections;

namespace DoveDraft.Input;

[Tool, GlobalClass]
public partial class ProjectInputBinder : RefCounted
{
    //
    //  Structs
    //

    public struct BindKeyCodeOptions
    {
        public bool ShiftPressed { get; set; } = false;
        public bool AltPressed { get; set; } = false;
        public bool CtrlPressed { get; set; } = false;
        public bool MetaPressed { get; set; } = false;
        
        public BindKeyCodeOptions() { }
    }
    
    //
    //  Public Variables
    //

    /// <summary>
    /// The name / key of the action that we are mapping with this class.
    /// </summary>
    public string ActionName { get; set; } = "";
    
    //
    //  Public Static Methods
    //

    public static void Remove(string actionName)
    {
        if (ProjectSettings.HasSetting(GetSettingName(actionName)))
        {
            ProjectSettings.SetSetting(GetSettingName(actionName), default);
        }
    }

    public static string GetSettingName(string actionName) => $"input/{actionName}";
    
    //
    //  Public Methods
    //

    public ProjectInputBinder BindKeyCode(Key physicalKeyCode, BindKeyCodeOptions options = default)
    {
        // Create event that corresponds to the given key
        var key = new InputEventKey();
        key.PhysicalKeycode = physicalKeyCode;
        key.ShiftPressed = options.ShiftPressed;
        key.AltPressed = options.AltPressed;
        key.CtrlPressed = options.CtrlPressed;
        key.MetaPressed = options.MetaPressed;
        
        // Add to map
        AddEventToSettings(key);
        return this;
    }

    public ProjectInputBinder BindMouseButton(MouseButton buttonIndex)
    {
        // Create the event that corresponds to the given mouse button
        var key = new InputEventMouseButton();
        key.ButtonIndex = buttonIndex;
        
        // Add to map
        AddEventToSettings(key);
        return this;
    }
    
    //
    //  Godot Methods
    //

    public ProjectInputBinder(string actionName)
    {
        ActionName = actionName;
        
        // If we don't have this input in the project settings yet, add it!
        if (!ProjectSettings.HasSetting(GetSettingName(actionName)))
        {
            var inputDict = new Dictionary();
            inputDict.Add("deadzone", 0.5f);
            inputDict.Add("events", new Array());
            ProjectSettings.SetSetting(GetSettingName(actionName), inputDict);
        }
    }
    
    //
    //  Private Methods
    //

    private void AddEventToSettings(InputEvent inputEvent)
    {
        var events = (Array)(ProjectSettings.GetSetting(GetSettingName(ActionName)).AsGodotObject().Get("events"));
        events.Add(inputEvent);
    }
        
}