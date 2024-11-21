using System;

namespace DoveDraft.Text;
using Godot;

[Tool, GlobalClass, Icon("../Icons/text_speaker.png")]
public partial class TextSounds : Resource
{
    //
    //  Exports
    //

    [Export] public AudioStream Default { get; set; }
    [Export] public AudioStream Period { get; set; }
    [Export] public AudioStream Exclam { get; set; }
    [Export] public AudioStream Comma { get; set; }
    [Export] public AudioStream Question { get; set; }

    //
    //  Public Methods
    //

    /// <summary>
    /// Get the AudioStream resource for a given TextSoundType.
    /// </summary>
    /// <param name="type">The type of TextSound to get the AudioStream for</param>
    /// <returns>An AudioStream corresponding to the given TextSoundType. null if there isn't one.</returns>
    public AudioStream GetStream(TextSoundType type)
    {
        switch (type)
        {
            case TextSoundType.Default:
                return Default;
            case TextSoundType.Period:
                return Period;
            case TextSoundType.Comma:
                return Comma;
            case TextSoundType.Question:
                return Question;
            case TextSoundType.Exclam:
                return Exclam;
            
            case TextSoundType.None:
            default:
                return null;
        }
    }
}
