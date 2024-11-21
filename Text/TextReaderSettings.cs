namespace DoveDraft.Text;
using Godot;

[Tool, GlobalClass, Icon("../Icons/text_reader_settings.png")]
public partial class TextReaderSettings : Resource
{
    //
    //  Exports
    //

    /// <summary>
    /// How many seconds to wait after displaying a normal character.
    /// </summary>
    [Export] public float CharacterShowDelay { get; set; } = 0.03f;

    /// <summary>
    /// How many seconds to wait after displaying a punctuation character.
    /// </summary>
    [Export] public float PunctuationShowDelay { get; set; } = 0.4f;
    
    /// <summary>
    /// The sounds to use when reading text.
    /// </summary>
    [Export] public TextSounds Sounds { get; set; }
}