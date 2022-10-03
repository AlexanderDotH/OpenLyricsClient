using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Models.Views;

namespace OpenLyricsClient.Frontend.View.Custom.Model;

public class LyricsScrollerViewModel : INotifyPropertyChanged
{
    public ObservableCollection<LyricPart> Lyrics { get; set; }

    public LyricsScrollerViewModel()
    {
        Lyrics = new ObservableCollection<LyricPart>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}