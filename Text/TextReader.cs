using System;

namespace DoveDraft.Text;
using Godot;

[Tool, GlobalClass, Icon("../Icons/text_reader.png")]
public partial class TextReader : Node
{
    //
    //  Exports
    //

    /// <summary>
    /// Emitted when the state of the text reader is updated.
    /// </summary>
    [Signal]
    public delegate void StateChangedEventHandler(TextReaderState newState);

    /// <summary>
    /// Emitted whenever the text being read is changed.
    /// </summary>
    [Signal]
    public delegate void TextChangedEventHandler(string rawTet, string strippedText);

    /// <summary>
    /// Emitted whenever the number of visible characters changed.
    /// </summary>
    [Signal]
    public delegate void VisibleCharsChangedEventHandler(int visibleCount, string character);

    /// <summary>
    /// Emitted when the text is now being read.
    /// </summary>
    [Signal]
    public delegate void ReadingStartedEventHandler(string rawText, string strippedText, TextReaderSettings settings);

    /// <summary>
    /// Emitted when reading is completed.
    /// </summary>
    [Signal]
    public delegate void ReadingFinishedEventHandler(TextReaderFinishReason reason);
    
    /// <summary>
    /// Values to use when no settings are provided.
    /// </summary>
    [Export]
    public TextReaderSettings DefaultSettings { get; set; }
    
    //
    //  Public Variables
    //

    /// <summary>
    /// The current state of this TextReader.
    /// </summary>
    public TextReaderState State
    {
        get => _state;
        private set
        {
            if (value == _state) return;
            _state = value;

            switch (value)
            {
                case TextReaderState.Empty:
                    NumOfCharsVisible = 0; // Hide all text
                    RawText = ""; // Reset our text field
                    break;
                case TextReaderState.Reading:
                    NumOfCharsVisible = 0; // Hide all text
                    _timeUntilNextChar = 0;
                    break;
                case TextReaderState.HasRead:
                    NumOfCharsVisible = -1; // Reveal ALL TEXT
                    break;
                
                case TextReaderState.Paused:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            EmitSignal(SignalName.StateChanged, (int)value);
        }
    }
    private TextReaderState _state = TextReaderState.Empty;
    
    /// <summary>
    /// The settings that this reader is actively using.
    /// </summary>
    public TextReaderSettings Settings { get; private set; }

    /// <summary>
    /// The raw value of the text being read. This string is not processed at all - meaning bbcodes may be present.
    /// </summary>
    public string RawText 
    { 
        get => _rawText;
        private set
        {
            _rawText = value;
            StrippedText = StripBbCode(value);
            _maxVisibleChars = StrippedText.Length;
            EmitSignal(SignalName.TextChanged, RawText, StrippedText);
        }
    }
    private string _rawText = "";

    /// <summary>
    /// The stripped value of the text being read. No bbcodes will be present.
    /// </summary>
    public string StrippedText { get; private set; } = "";

    /// <summary>
    /// The number of characters of the visible stripped text. If this is negative, all characters are shown.
    /// </summary>
    public int NumOfCharsVisible
    {
        get => _numOfCharsVisible;
        private set
        {
            var newCount = value;
            
            // If we set to a negative value, we want to show all chars
            if (newCount < 0)
            {
                newCount = StrippedText.Length;
            }
            // If we set to a value too large, cap it
            else if (newCount > StrippedText.Length)
            {
                newCount = StrippedText.Length;
            }

            _numOfCharsVisible = newCount;
            var actualChar = "";
            
            // If we are actually displaying a character, then cache it so that we can emit it below.
            if (StrippedText.Length > 0) actualChar = StrippedText[(_numOfCharsVisible - 1)..];

            LastVisibleChar = actualChar;
            EmitSignal(SignalName.VisibleCharsChanged, newCount, actualChar);
        }
    }
    private int _numOfCharsVisible = 0;
    
    /// <summary>
    /// The last character of the visible stripped text.
    /// </summary>
    public string LastVisibleChar { get; private set; }

    /// <summary>
    /// Is the show sequence currently paused?
    /// </summary>
    public bool IsShowPaused
    {
        get => State == TextReaderState.Paused;
        set
        {
            switch (State)
            {
                case TextReaderState.Reading:
                case TextReaderState.Paused:
                    State = value ? TextReaderState.Paused : TextReaderState.Reading;
                    break;
                
                default:
                case TextReaderState.Empty:
                case TextReaderState.HasRead:
                    break;
            }
        }
    }
    
    //
    //  Private Variables
    //

    /// <summary>
    /// How much time to wait between the current character and the next character.
    /// </summary>
    private double _timeUntilNextChar = 0;

    /// <summary>
    /// The maximum number of characters that can be visible.
    /// </summary>
    private int _maxVisibleChars = 0;

    /// <summary>
    /// The RegEx that matches to all BBCodes in a string.
    /// </summary>
    private RegEx _bbCodeRegEx;
    
    //
    //  Public Methods
    //

    public void StartReading(string newText, TextReaderSettings settings)
    {
        // Check if we need to use default values, then apply the settings
        settings ??= DefaultSettings ?? new TextReaderSettings();
        Settings = settings;

        // Check if we need to set the sounds too
        if (Settings.Sounds == null && DefaultSettings?.Sounds != null) Settings.Sounds = DefaultSettings.Sounds;
        
        // Update what the text actually is
        RawText = newText;
        State = TextReaderState.Reading;
        EmitSignal(SignalName.ReadingStarted, RawText, StrippedText, Settings);
    }
    
    public void SkipToReadingEnd()
    {
        if (State == TextReaderState.HasRead) return;

        State = TextReaderState.HasRead;
        EmitSignal(SignalName.ReadingFinished, (int)TextReaderFinishReason.Skipped);
    }
    
    public void CancelReading()
    {
        if (State == TextReaderState.Empty) return;

        State = TextReaderState.Empty;
        EmitSignal(SignalName.ReadingFinished, (int)TextReaderFinishReason.Canceled);
    }
    
    //
    //  Godot Methods
    //

    public override void _Process(double delta)
    {
        if (State == TextReaderState.Reading) HandleShowSequence(delta);
    }

    //
    //  Private Methods
    //

    private void HandleShowSequence(double delta)
    {
        _timeUntilNextChar -= delta; // Count down the timer...
        
        // If we should display the next character NOW...
        if (_timeUntilNextChar <= 0)
        {
            // Display the char & reset timer, then address end of dialog if needed
            HandleShowNextChar();
            if(NumOfCharsVisible >= _maxVisibleChars) HandleReachEndOfDialog();
        }
    }

    private void HandleShowNextChar()
    {
        if (StrippedText.Length <= 0) return;
        
        // Visibly show the next character
        NumOfCharsVisible++;
        
        // Reset the character timer
        _timeUntilNextChar = GetCharDisplaySpeed(LastVisibleChar, StrippedText, NumOfCharsVisible - 1);
    }

    private void HandleReachEndOfDialog()
    {
        State = TextReaderState.HasRead;
        EmitSignal(SignalName.ReadingFinished, (int)TextReaderFinishReason.EndOfText);
    }

    private double GetCharDisplaySpeed(string character, string dialogText, int index)
    {
        // If there's multiple characters in a row, then treat this as a normal char
        if (index + 1 < dialogText.Length)
        {
            if (character == dialogText.Substring(index + 1, 1))
            {
                return Settings.CharacterShowDelay;
            }
        }

        return dialogText switch
        {
            " " => 0,
            "." or "!" or "," or "?" or "-" or ":" or ";" or "\n" => Settings.PunctuationShowDelay,
            _ => Settings.CharacterShowDelay
        };
    }
    
    /// <summary>
    /// Strip the BBCodes out from some text.
    /// </summary>
    /// <param name="textToStrip">The text containing BBCodes to strip.</param>
    /// <returns>The given text WITHOUT BBCodes.</returns>
    private string StripBbCode(string textToStrip)
    {
        // Lazy populate the regex
        if (_bbCodeRegEx == null)
        {
            // Thanks to https://github.com/godotengine/godot-proposals/issues/5056#issuecomment-1203033323 !
            _bbCodeRegEx = new RegEx();
            _bbCodeRegEx.Compile(@"\[.+?\]");
        }
        
        return _bbCodeRegEx.Sub(textToStrip, "", true);
    }
}