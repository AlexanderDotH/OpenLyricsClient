using System;

namespace OpenLyricsClient.Backend.Events.EventArgs;

public class SettingsChangedEventArgs : System.EventArgs
{
    private Settings.Settings _settings;

    public SettingsChangedEventArgs(Settings.Settings settings)
    {
        _settings = settings;
    }

    public Settings.Settings Settings
    {
        get => _settings;
        set => _settings = value;
    }
}