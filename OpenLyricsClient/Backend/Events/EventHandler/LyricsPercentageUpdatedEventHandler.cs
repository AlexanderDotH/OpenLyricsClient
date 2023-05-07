using System;
using OpenLyricsClient.Backend.Events.EventArgs;

namespace OpenLyricsClient.Backend.Events.EventHandler;

public delegate void LyricsPercentageUpdatedEventHandler(Object sender, LyricsPercentageUpdatedEventArgs args);
