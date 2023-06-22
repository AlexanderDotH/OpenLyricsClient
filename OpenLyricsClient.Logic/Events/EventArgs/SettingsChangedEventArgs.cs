namespace OpenLyricsClient.Logic.Events.EventArgs;

public class SettingsChangedEventArgs : System.EventArgs
{
    private Object _section;
    private string _field;

    public Object Section
    {
        get => _section;
        set => _section = value;
    }

    public string Field
    {
        get => _field;
        set => _field = value;
    }
}