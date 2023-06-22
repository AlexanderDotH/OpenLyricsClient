using OpenLyricsClient.Logic.Events.EventArgs;

namespace OpenLyricsClient.Logic.Events.EventHandler;

public delegate void LyricChangedEventHandler(Object sender, LyricChangedEventArgs lyricChangedEventArgs);
