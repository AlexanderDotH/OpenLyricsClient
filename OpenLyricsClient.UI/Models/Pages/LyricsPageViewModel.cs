using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Handler.Services.Services.Spotify;
using OpenLyricsClient.Logic.Handler.Song.SongProvider;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.View.Windows;
using ReactiveUI;

namespace OpenLyricsClient.UI.Models.Pages;

public class LyricsPageViewModel : ModelBase
{
    private TaskSuspensionToken _songInfoSuspensionToken;

    public ReactiveCommand<Unit, Unit> UpdatePlaybackCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousSongCommand { get; }
    public ReactiveCommand<Unit, Unit> NextSongCommand { get; }
    
    public ReactiveCommand<Unit, Unit> SwitchToSettingsCommand { get; }
    
    public LyricsPageViewModel()
    {
        UpdatePlaybackCommand = ReactiveCommand.CreateFromTask(UpdatePlayback);
        PreviousSongCommand = ReactiveCommand.CreateFromTask(()=>SkipSong(EnumPlayback.PREVOUS_TRACK));
        NextSongCommand = ReactiveCommand.CreateFromTask(()=>SkipSong(EnumPlayback.NEXT_TRACK));

        SwitchToSettingsCommand = ReactiveCommand.Create(SwitchToSettings);

        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingManagerOnSettingsChanged;
        Core.INSTANCE.ArtworkHandler.ArtworkAppliedHandler += ArtworkHandlerOnArtworkAppliedHandler;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SongHandler.SongUpdated += SongHandlerOnSongUpdated;
    }

    private void SongHandlerOnSongUpdated(object sender)
    {
        OnPropertyChanged("SongName");
        OnPropertyChanged("Artists");
        /*OnPropertyChanged("Album");*/
        OnPropertyChanged("IsSongPlaying");
        OnPropertyChanged("CurrentTime");
        OnPropertyChanged("Percentage");
        OnPropertyChanged("CurrentTime");
        OnPropertyChanged("CurrentMaxTime");
        OnPropertyChanged("IsPlayerAvailable");
        OnPropertyChanged("IsSongAvailable");
        OnPropertyChanged("IsEmpty");
    }

    public void SwitchToSettings()
    {
        MainWindow.Instance.SelectPage(2);
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

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        OnPropertyChanged("AiBadge");
    }

    private void ArtworkHandlerOnArtworkAppliedHandler(object sender, ArtworkAppliedEventArgs args)
    {
        OnPropertyChanged("Artwork");

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            OnPropertyChanged("UiBackground");
            OnPropertyChanged("UiForeground");
        });
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        OnPropertyChanged("UiBackground");
        OnPropertyChanged("UiForeground");
        OnPropertyChanged("SelectedColor");
        OnPropertyChanged("UnSelectedColor");
        
        OnPropertyChanged("IsAnyServiceConnected");
        OnPropertyChanged("IsSongNotPlayingAndAnyServiceConnected");
        OnPropertyChanged("IsSongAvailable");
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        OnPropertyChanged("SongName");
        OnPropertyChanged("Artists");
        OnPropertyChanged("Album");
    }

    public string? SongName
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.SongMetadata?.Name!;
    }

    public bool IsSongPlaying
    {
        get => Core.INSTANCE.SongHandler?.CurrentSong?.Paused! == false;
    }

    public bool IsSongAvailable
    {
        get => Core.INSTANCE.SongHandler?.CurrentSong! != null && IsAnyServiceConnected;
    }
    
    public bool IsEmpty
    {
        get => Core.INSTANCE.SongHandler?.CurrentSong! == null && IsAnyServiceConnected;
    }
    
    public bool IsPlayerAvailable
    {
        get => Core.INSTANCE.SongHandler?.SongProvider! == EnumSongProvider.SPOTIFY;
    }

    public bool IsAnyServiceConnected
    {
        get => Core.INSTANCE.ServiceHandler.IsAnyConnected();
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
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("OpenLyricsClient") == true && 
               Core.INSTANCE.CacheManager.IsLyricsInCache(SongRequestObject.FromSong(Core.INSTANCE?.SongHandler?.CurrentSong!));
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
        get => Core.INSTANCE.SongHandler?.CurrentSong?.ProgressString!;
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
}