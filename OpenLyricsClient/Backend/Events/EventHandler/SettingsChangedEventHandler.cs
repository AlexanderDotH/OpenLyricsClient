using System;
using OpenLyricsClient.Backend.Events.EventArgs;

namespace OpenLyricsClient.Backend.Events.EventHandler;

public delegate void SettingsChangedEventHandler(Object sender, SettingsChangedEventArgs settingsChangedEventArgs);