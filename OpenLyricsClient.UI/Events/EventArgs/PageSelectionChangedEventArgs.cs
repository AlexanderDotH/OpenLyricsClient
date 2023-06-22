using Avalonia.Controls;

namespace OpenLyricsClient.UI.Events.EventArgs;

public class PageSelectionChangedEventArgs : System.EventArgs
{
    private UserControl _fromPage;
    private UserControl _toPage;

    public PageSelectionChangedEventArgs(UserControl fromPage, UserControl toPage)
    {
        this._fromPage = fromPage;
        this._toPage = toPage;
    }

    public UserControl FromPage
    {
        get => _fromPage;
    }

    public UserControl ToPage
    {
        get => _toPage;
    }
}