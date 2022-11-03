using System.Collections.ObjectModel;

namespace OpenLyricsClient.Frontend.Models.Pages;

public class SettingsPageViewModel : ViewModelBase
{
    private ObservableCollection<string> _lyricsSelectionMode;

    public SettingsPageViewModel()
    {
    }

    public ObservableCollection<string> LyricsSelectionMode
    {
        get
        {
            this._lyricsSelectionMode = new ObservableCollection<string>();
            this._lyricsSelectionMode.Add("Quality");
            this._lyricsSelectionMode.Add("Performance");
            return this._lyricsSelectionMode;
        }
    }
}