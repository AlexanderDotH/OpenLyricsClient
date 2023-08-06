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
using DevBase.Avalonia.Color.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Handler.Services.Services.Spotify;
using OpenLyricsClient.Logic.Handler.Song.SongProvider;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Palette;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Extensions;
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

    private ColorPalette _colorPalette;
    
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
        
        App.INSTANCE.ColorHandler.ColorResourceUpdated += ColorHandlerOnColorResourceUpdated;
    }

    private void ColorHandlerOnColorResourceUpdated(object sender)
    {
        OnPropertyChanged("UiFontForeground");
        OnPropertyChanged("AiBadgeStartColor");
        OnPropertyChanged("AiBadgeEndColor");
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
        
        OnPropertyChanged("UiFontForeground");
        OnPropertyChanged("AiBadgeStartColor");
        OnPropertyChanged("AiBadgeEndColor");
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
        OnPropertyChanged("LyricFindBadge");
        OnPropertyChanged("MusixMatchBadge");
        OnPropertyChanged("TextylBadge");
        OnPropertyChanged("NetEaseBadge");
        OnPropertyChanged("IsInCache");
    }

    private void ArtworkHandlerOnArtworkAppliedHandler(object sender, ArtworkAppliedEventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            OnPropertyChanged("Artwork");
            OnPropertyChanged("UiBackground");
            OnPropertyChanged("UiLightBackground");
            OnPropertyChanged("UiForeground");
            OnPropertyChanged("UiFontForeground");
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
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            OnPropertyChanged("SongName");
            OnPropertyChanged("Artists");
            OnPropertyChanged("IsInCache");
        });
        //OnPropertyChanged("Album");
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

    public ColorPalette ColorPalette
    {
        get => _colorPalette;
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
               IsInCache;
    }
    
    public bool LyricFindBadge
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("Deezer") == true &&
               IsInCache;
    }
    
    public bool NetEaseBadge
    {
        get => (Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("NetEase") == true || 
                Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("NetEaseV2") == true) &&
               IsInCache;
    }
    
    public bool TextylBadge
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("Textyl") == true &&
               IsInCache;
    }

    public bool MusixMatchBadge
    {
        get => Core.INSTANCE?.SongHandler?.CurrentSong?.Lyrics?.LyricProvider?.SequenceEqual("MusixMatch") == true &&
               IsInCache;
    }

    public bool IsInCache =>
        Core.INSTANCE.CacheManager.IsLyricsInCache(
            SongRequestObject.FromSong(Core.INSTANCE?.SongHandler?.CurrentSong!));
    
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

    public SolidColorBrush UiLightBackground => UiBackground.AdjustBrightness(150);
    
    public SolidColorBrush UiForeground
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return (App.Current.FindResource("SecondaryBackgroundBrush") as SolidColorBrush).AdjustBrightness(50);
            
            return (App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush);
        }
    }

    public Color AiBadgeStartColor
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return (App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush).Color;

            return (App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush).Color;
        }
    }
    
    public Color AiBadgeEndColor
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return (App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush).Color;
            
            return (App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush).AdjustBrightness(150).Color;
        }
    }
    
    public SolidColorBrush UiFontForeground
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("PrimaryFontColorBrush") as SolidColorBrush;
            
            SolidColorBrush brush = App.Current.FindResource("PrimaryFontColorBrush") as SolidColorBrush;

            if (AiBadgeStartColor.BrightnessPercentage() > 80)
                return brush.AdjustBrightness(70);

            return brush;
        }
    }
}