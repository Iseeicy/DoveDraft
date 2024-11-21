namespace DoveDraft.Text;

public enum TextReaderState
{
    /// <summary>
    /// There is no text being show.
    /// </summary>
    Empty = 0,
    
    /// <summary>
    /// Text is actively being read.
    /// </summary>
    Reading = 1,
    
    /// <summary>
    /// Text was being read, but is now paused.
    /// </summary>
    Paused = 2,
    
    /// <summary>
    /// Text has finished being read and is on screen.
    /// </summary>
    HasRead = 3
}