using System;
using OpenLyricsClient.Backend.Events.EventArgs;

namespace OpenLyricsClient.Backend.Events.EventHandler;

public delegate void LyricChangedEventHandler(Object sender, LyricChangedEventArgs lyricChangedEventArgs);
