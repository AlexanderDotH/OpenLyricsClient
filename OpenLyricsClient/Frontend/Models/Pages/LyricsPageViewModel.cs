using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
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
using OpenLyricsClient.Backend.Handler.Services.Services.Spotify;
using OpenLyricsClient.Backend.Handler.Song.SongProvider;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using ReactiveUI;

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
    
    public ReactiveCommand<Unit, Unit> UpdatePlaybackCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousSongCommand { get; }
    public ReactiveCommand<Unit, Unit> NextSongCommand { get; }
    
    public LyricsPageViewModel()
    {
        UpdatePlaybackCommand = ReactiveCommand.CreateFromTask(UpdatePlayback);
        PreviousSongCommand = ReactiveCommand.CreateFromTask(()=>SkipSong(EnumPlayback.PREVOUS_TRACK));
        NextSongCommand = ReactiveCommand.CreateFromTask(()=>SkipSong(EnumPlayback.NEXT_TRACK));

        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingManagerOnSettingsChanged;
        Core.INSTANCE.ArtworkHandler.ArtworkAppliedHandler += ArtworkHandlerOnArtworkAppliedHandler;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SongHandler.SongUpdated += SongHandlerOnSongUpdated;
        
        this._currentSongName = string.Empty;
        this._currentArtists = string.Empty;
        this._currentAlbumName = string.Empty;
        this._currentPercentage = 0;
        this._currentTime = string.Empty;
        this._currentMaxTime = string.Empty;

        this._time = 0;
    }

    private void SongHandlerOnSongUpdated(object sender)
    {
        OnPropertyChanged("SongName");
        OnPropertyChanged("Artists");
        /*OnPropertyChanged("Album");*/
        OnPropertyChanged("IsSongPlaying");
        OnPropertyChanged("CurrentTime");
        OnPropertyChanged("Percentage");
        OnPropertyChanged("CurrentMaxTime");
        OnPropertyChanged("IsPlayerAvailable");
    }

    public async Task UpdatePlayback()
    {
        try
        {
            Song song = Core.INSTANCE.SongHandler?.CurrentSong!;
            SpotifyService service = (SpotifyService)Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");
        
            if (song.Paused)
            {
                await service.UpdatePlayback(EnumPlayback.RESUME);
            }
            else
            {
                await service.UpdatePlayback(EnumPlayback.PAUSE);
            }

            await Dispatcher.UIThread.InvokeAsync(() => OnPropertyChanged("IsSongPlaying"));
        }
        catch (Exception e) { }
    }

    public async Task SkipSong(EnumPlayback playback)
    {
        try
        {
            SpotifyService service = (SpotifyService)Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");
            await service.UpdatePlayback(playback);
        }
        catch (Exception e) { }
    }

    private void LyricHandlerOnLyricsFound(object sender)
    {
        OnPropertyChanged("AiBadge");
    }

    private void ArtworkHandlerOnArtworkAppliedHandler(object sender, ArtworkAppliedEventArgs args)
    {
        OnPropertyChanged("Artwork");
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        OnPropertyChanged("UiBackground");
        OnPropertyChanged("UiForeground");
        OnPropertyChanged("SelectedColor");
        OnPropertyChanged("UnSelectedColor");
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        OnPropertyChanged("SongName");
        OnPropertyChanged("Artists");
        OnPropertyChanged("Album");

        this._time = 0;
    }

    public string? SongName
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.SongMetadata?.Name!;
    }

    public bool IsSongPlaying
    {
        get => Core.INSTANCE.SongHandler?.CurrentSong?.Paused! == false;
    }

    public bool IsPlayerAvailable
    {
        get => Core.INSTANCE.SongHandler?.SongProvider! == EnumSongProvider.SPOTIFY;
    }
    
    public bool IsSongPaused
    {
        get => Core.INSTANCE.SongHandler?.CurrentSong?.Paused! == true;
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
        get
        {
            Song song = Core.INSTANCE.SongHandler.CurrentSong;

            if (DataValidator.ValidateData(song))
            {
                return song.GetPercentage();
            }

            return 0;
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
        get => Core.INSTANCE.SongHandler?.CurrentSong?.MaxProgressString!;
    }
    
    public string Artwork
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Artwork?.FilePath!;
    }
    
    public SolidColorBrush SelectedColor
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
        }
    }
    
    public SolidColorBrush UnSelectedColor
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
        }
    }

    public SolidColorBrush UiBackground
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        }
    }
    
    public SolidColorBrush UiForeground
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
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