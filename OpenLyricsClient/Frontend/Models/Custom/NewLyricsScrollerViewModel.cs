﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Models.Pages.Settings;
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
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        if (!settingschangedeventargs.Field.Equals("Artwork Background"))
            return;
        
        OnPropertyChanged("UiBackground");
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
    
    public SolidColorBrush UiBackground
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<bool>("Artwork Background") == true)
                return App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        }
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