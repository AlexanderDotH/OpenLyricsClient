using OpenLyricsClient.Logic.Events.EventArgs;

namespace OpenLyricsClient.Logic.Events.EventHandler;

public delegate void LyricTimeChangedEventHandler(Object sender, LyricChangedEventArgs lyricChangedEventArgs);