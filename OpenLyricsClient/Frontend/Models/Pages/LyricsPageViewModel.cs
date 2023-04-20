using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Frontend.Models.Pages;

public class LyricsPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private TaskSuspensionToken _songInfoSuspensionToken;

    private string _currentSongName;
    private string _currentArtists;
    private string _currentAlbumName;
    private double _currentPercentage;
    private long _time;
    private string _currentTime;
    private string _currentMaxTime;

    private string _artwork;
    
    public LyricsPageViewModel()
    {
        Core.INSTANCE.TickHandler += OnTickHandler;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        Core.INSTANCE.SettingManager.SettingsChanged += SettingManagerOnSettingsChanged;
        Core.INSTANCE.ArtworkHandler.ArtworkAppliedHandler += ArtworkHandlerOnArtworkAppliedHandler;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        
        this._currentSongName = string.Empty;
        this._currentArtists = string.Empty;
        this._currentAlbumName = string.Empty;
        this._currentPercentage = 0;
        this._currentTime = string.Empty;
        this._currentMaxTime = string.Empty;

        this._time = 0;
    }

    private void LyricHandlerOnLyricsFound(object sender)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AiBadge"));
    }

    private void ArtworkHandlerOnArtworkAppliedHandler(object sender, ArtworkAppliedEventArgs args)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artwork"));
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UiBackground"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UiForeground"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedColor"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UnSelectedColor"));
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongName"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artists"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Album"));

        this._time = 0;
    }

    // RECODE NEEDED My eyes are bleeding
    private void OnTickHandler(object sender)
    {
        Song song = Core.INSTANCE.SongHandler.CurrentSong;
        
        if (!DataValidator.ValidateData(song))
            return;

        if (!this._time.Equals(song.Time))
        {
            Percentage = song.GetPercentage();
            this._time = song.Time;
        }
        
        if (!this._currentTime.Equals(song.ProgressString))
            CurrentTime = song.ProgressString;
         
        if (!this._currentMaxTime.Equals(song.MaxProgressString))
            CurrentMaxTime = song.MaxProgressString;
    }

    public string? SongName
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.SongMetadata?.Name!;
    }

    
    
    /*public string? SongName
    {
        get => "Never gonna give up";
    }
    
    public string Artists
    {
        get => "Rick Astley";
    }*/

    public bool AiBadge
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider.SequenceEqual("OpenLyricsClient") == true;
    }
    
    public string Artists
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.SongMetadata?.FullArtists!;
    }

    public double Percentage
    {
        get => this._currentPercentage;
        set
        {
            this._currentPercentage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
        }
    }

    public string CurrentTime
    {
        get => this._currentTime;
        set
        {
            this._currentTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentTime"));
        }
    }
    
    public string CurrentMaxTime
    {
        get => this._currentMaxTime;
        set
        {
            this._currentMaxTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentMaxTime"));
        }
    }
    
    public string Artwork
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Artwork?.FilePath!;
    }
    
    public SolidColorBrush SelectedColor
    {
        get
        {
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
                return App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
        }
    }
    
    public SolidColorBrush UnSelectedColor
    {
        get
        {
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
                return App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
        }
    }

    public SolidColorBrush UiBackground
    {
        get
        {
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
                return App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        }
    }
    
    public SolidColorBrush UiForeground
    {
        get
        {
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
                return App.Current.FindResource("PrimaryFontColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryFontColorBrush") as SolidColorBrush;
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