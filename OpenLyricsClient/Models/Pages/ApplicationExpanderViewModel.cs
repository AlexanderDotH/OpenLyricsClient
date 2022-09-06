using System;
using System.Collections.ObjectModel;
using OpenLyricsClient.Models.Views;

namespace OpenLyricsClient.Models.Pages;

public class ApplicationExpanderViewModel : ViewModelBase
{
    private ObservableCollection<string> _lyricsSelectionMode;

    public ApplicationExpanderViewModel()
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