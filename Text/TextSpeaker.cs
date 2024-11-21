using System;
using System.Collections.Generic;
using Godot;

namespace DoveDraft.Text;

[Tool, GlobalClass, Icon("../Icons/text_speaker.png")]
public partial class TextSpeaker : Node
{
    //
    //  Exports
    //
    
    /// <summary>
    /// The origin of our audio, and also where audio players will be created. If this is null, a child node without specialization will be made.
    /// </summary>
    [Export] public Node AudioOriginNode { get; set; }
    
    /// <summary>
    /// The sounds to use when writing text, by default.
    /// </summary>
    [Export] public TextSounds DefaultSounds { get; set; }
    
    //
    //  Public Variables
    //
    
    /// <summary>
    /// Is this speaker muted?
    /// </summary>
    public bool IsMuted { get; set; }
    
    //
    //  Private Variables
    //

    private TextSounds Sounds
    {
        get => _sounds;
        set
        {
            // If we're already using these sounds, EXIT EARLY
            if (_sounds == value) return;
            
            // Use defaults if we're given no sounds
            TextSounds newSounds = value ?? DefaultSounds;
            
            // Remove all existing sound players
            foreach (AudioStreamPlayer player in _audioPlayers.Values)
            {
                player.QueueFree();
            }
            _audioPlayers.Clear();
            
            // Save our new sounds, and if we truly have none then exit
            _sounds = newSounds;
            if (newSounds == null) return;
            
            // OTHERWISE, let's spawn players for each type
            foreach (TextSoundType type in Enum.GetValues<TextSoundType>())
            {
                var stream = newSounds.GetStream(type);
                if(stream == null) continue;
                
                // Spawn a player for this stream
                var newPlayer = CreateSoundPlayer(type, stream);
                if(newPlayer == null) continue;
                
                // Store the new player by type
                _audioPlayers[type] = newPlayer;
            }
        }
    }
    private TextSounds _sounds;
    
    /// <summary>
    /// How many characters since last playing a sound.
    /// </summary>
    private int _charCounter = 0;

    /// <summary>
    /// The last character processed.
    /// </summary>
    private string _previousChar = "";

    /// <summary>
    /// A cache of audio players that exist on a per-TextSoundType basis.
    /// </summary>
    private Dictionary<TextSoundType, Node> _audioPlayers = new();
    
    //
    //  Public Methods
    //

    public void ResetSpeakState()
    {
        _charCounter = 0;
        _previousChar = "";
    }

    public void HandleCharacter(string character, bool forcePlay = false)
    {
        _charCounter++;
        TextSoundType speakType = CharToSpeakType(character);
        _previousChar = character;
        
        // If we should force playing a sound, mark the speak type appropriately
        if (forcePlay && speakType == TextSoundType.None) speakType = TextSoundType.Default;
        
        // If we shouldn't play a sound, EXIT EARLY
        if (speakType == TextSoundType.None) return;
        
        // Play a sound for this character
        PlaySoundType(speakType);
        _charCounter = 0;
    }

    /// <summary>
    /// When text starts, force a noise
    /// </summary>
    /// <param name="rawText"></param>
    /// <param name="strippedText"></param>
    /// <param name="settings"></param>
    public void OnTextReaderReadingSTarted(string rawText, string strippedText, TextReaderSettings settings)
    {
        ResetSpeakState();
        Sounds = settings.Sounds;
        HandleCharacter(" ", true);
    }

    /// <summary>
    /// When a new character is shown, eat it and maybe play a noise
    /// </summary>
    /// <param name="visibleCount"></param>
    /// <param name="character"></param>
    public void OnTextReaderVisibleCharsChanged(int visibleCount, string character)
    {
        HandleCharacter(character);
    }
    
    //
    //  Godot Methods
    //

    public TextSpeaker()
    {
        // Populate the default sounds variable if not provided
        DefaultSounds ??= GD.Load<TextSounds>("addons/DoveDraft/Text/Sounds/Console/console.tres");
    }
    
    //
    //  Private Methods
    //

    private Node CreateSoundPlayer(TextSoundType type, AudioStream stream)
    {
        // If we're not given an audio origin, then just make our own and assume no spatialization
        if (AudioOriginNode == null)
        {
            var newOrigin = new Node();
            AddChild(newOrigin);
            AudioOriginNode = newOrigin;
        }
        
        // Create the audio stream player depending on the type that the audio origin is. This lets us have spatialization
        Node newPlayer = AudioOriginNode switch
        {
            Node3D => new AudioStreamPlayer3D(),
            Node2D => new AudioStreamPlayer2D(),
            _ => new AudioStreamPlayer()
        };
        newPlayer.Set("stream", stream);
        
        AudioOriginNode.AddChild(newPlayer);
        return newPlayer;
    }

    private void PlaySoundType(TextSoundType type)
    {
        if (IsMuted) return;
        if (type == TextSoundType.None) return;
        
        // If we don't have this type or a default type, EXIT EARLY
        TextSoundType realType = _audioPlayers.ContainsKey(type) ? type : TextSoundType.Default;
        if (!_audioPlayers.TryGetValue(realType, out Node playerForType)) return;
        
        // OTHERWISE - we have a real sound type, so play it and stop other sound types!
        foreach (Node player in _audioPlayers.Values)
        {
            player.Call(player == playerForType ? "play" : "stop");
        }
    }
    
    /// <summary>
    /// Depending on the state of this, get the speak type for the given char.
    /// </summary>
    /// <param name="character">The character to get the next sound for.</param>
    /// <returns>The text sound for this character.</returns>
    private TextSoundType CharToSpeakType(string character) => character switch
    {
        "\n" or " " => TextSoundType.None,
        "." => _previousChar == "." ? TextSoundType.None : TextSoundType.Period,
        "," or "-" or ":" or ";" => TextSoundType.Comma,
        "?" => TextSoundType.Question,
        "!" => TextSoundType.Exclam,
        _ => _charCounter > 3 ? TextSoundType.Default : TextSoundType.None
    };
}