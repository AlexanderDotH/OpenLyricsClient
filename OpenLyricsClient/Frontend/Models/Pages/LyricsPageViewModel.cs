using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        this._currentSongName = string.Empty;
        this._currentArtists = string.Empty;
        this._currentAlbumName = string.Empty;
        this._currentPercentage = 0;
        this._currentTime = string.Empty;
        this._currentMaxTime = string.Empty;

        this._time = 0;
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
        this._time = 0;
    }

    // RECODE NEEDED My eyes are bleeding
    private void OnTickHandler(object sender)
    {
        Song song = Core.INSTANCE.SongHandler.CurrentSong;
        
        if (!DataValidator.ValidateData(song))
            return;

        if (!this._currentSongName.Equals(song.SongMetadata.Name))
            SongName = song.SongMetadata.Name;

        if (!this._currentArtists.Equals(song.SongMetadata.FullArtists))
            Artists = song.SongMetadata.FullArtists;
        
        if (!this._currentAlbumName.Equals(song.SongMetadata.Album))
            AlbumName = song.SongMetadata.Album;

        if (!this._time.Equals(song.Time))
        {
            Percentage = song.GetPercentage();
            this._time = song.Time;
        }
        
        if (!this._currentTime.Equals(song.ProgressString))
            CurrentTime = song.ProgressString;
        
        if (!this._currentMaxTime.Equals(song.MaxProgressString))
            CurrentMaxTime = song.MaxProgressString;

        if (DataValidator.ValidateData(song.Artwork))
        {
            Artwork artwork = song.Artwork;

            if (DataValidator.ValidateData(artwork))
            {
                if (!DataValidator.ValidateData(this._artwork) || 
                      DataValidator.ValidateData(this._artwork) && !this._artwork.Equals(artwork))
                    Artwork = artwork.FilePath;
            }
        }
    }

    public string SongName
    {
        get => this._currentSongName;
        set
        {
            this._currentSongName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongName"));
        }
    }
    
    public string Artists
    {
        get => this._currentArtists;
        set
        {
            this._currentArtists = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artists"));
        }
    }
    
    public string AlbumName
    {
        get => this._currentAlbumName;
        set
        {
            this._currentAlbumName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumName"));
        }
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
        get => this._artwork;
        set
        {
            this._artwork = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artwork"));
        }
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