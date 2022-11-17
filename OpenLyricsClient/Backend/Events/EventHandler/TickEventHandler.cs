using System;
using OpenLyricsClient.Backend.Events.EventArgs;

namespace OpenLyricsClient.Backend.Events.EventHandler;

public delegate void TickEventHandler(Object sender);
public delegate void SlowTickEventHandler(Object sender);