using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.Models.Custom;

public class NewLyricsScrollerViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private LyricPart? _lyric;
    
    public NewLyricsScrollerViewModel()
    {
        Core.INSTANCE.LyricHandler.LyricChanged += LyricHandlerOnLyricChanged;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        Lyric = null;
        OnPropertyChanged("Lyrics");
    }

    private void LyricHandlerOnLyricsFound(object sender)
    {
        OnPropertyChanged("Lyrics");
    }

    private void LyricHandlerOnLyricChanged(object sender, LyricChangedEventArgs lyricchangedeventargs)
    {
        Lyric = lyricchangedeventargs.LyricPart;
    }

    public LyricPart[]? Lyrics
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricParts!;
    }

    public LyricPart? Lyric
    {
        get => this._lyric;
        set => SetField(ref this._lyric, value);
    }
    
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