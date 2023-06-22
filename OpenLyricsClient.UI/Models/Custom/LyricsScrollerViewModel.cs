using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Helper;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Models.Pages.Settings;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.UI.Models.Custom;

public class LyricsScrollerViewModel : INotifyPropertyChanged
{
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsPercentageSuspensionToken;

    public ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private double _percentage;

    private RomanizationHelper _romanizationHelper;

    private double _oldPercentage;
    private LyricPart _oldLyricPart;

    public LyricsScrollerViewModel()
    {
        this._romanizationHelper = new RomanizationHelper();
        
        if (!AvaloniaUtils.IsInPreviewerMode())
        {
            this._oldPercentage = 0;
            //this._oldLyricPart = new LyricPart(-1, "Never gonna give you up");
            
            this._lyricParts = new ObservableCollection<LyricPart>();
            this.CurrentLyricParts =  new ObservableCollection<LyricPart>();

            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
            
            Core.INSTANCE.TickHandler += OnLyricsLoadTickHandler;
            Core.INSTANCE.TickHandler += OnLyricsSyncTickHandler;
            Core.INSTANCE.TickHandler += OnLyricsSyncPercentageTickHandler;

            Core.INSTANCE.SettingsHandler.SettingsChanged += (sender, args) =>
            {
                if (!args.Section.Equals(typeof(SettingsLyricsViewModel)))
                    return;
                
                this.CurrentLyricParts = null;
                OnPropertyChanged("UiBackground");
            };
        }
    }

    private void OnLyricsSyncPercentageTickHandler(object sender)
    {
        Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

        if (!DataValidator.ValidateData(currentSong))
            return;

        if (!DataValidator.ValidateData(currentSong.Lyrics))
            return;

        if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
            return;

        if (!DataValidator.ValidateData(this._lyricParts))
            return;

        if (!this._oldPercentage.Equals(currentSong.CurrentLyricPart.Percentage))
            this.Percentage = currentSong.CurrentLyricPart.Percentage;
    }

    private void OnLyricsSyncTickHandler(object sender)
    {
        Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

        if (!DataValidator.ValidateData(currentSong))
            return;

        if (!DataValidator.ValidateData(currentSong.Lyrics))
            return;

        if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
            return;

        this.CurrentLyricPart = currentSong.CurrentLyricPart;

    }

    private void OnLyricsLoadTickHandler(object sender)
    {
        Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

        if (!DataValidator.ValidateData(currentSong))
            return;

        if (!DataValidator.ValidateData(currentSong.Lyrics))
        {
            this.CurrentLyricParts = null;
            return;
        }
        
        if (currentSong!.Lyrics!.LyricParts!.IsNullOrEmpty())
            return;

        if (!DataValidator.ValidateData(this.CurrentLyricParts))
        {
            this.CurrentLyricParts = new ObservableCollection<LyricPart>();
            return;
        }

        if (!(AreListsEqual(this.CurrentLyricParts, currentSong?.Lyrics?.LyricParts!)))
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                this.CurrentLyricParts =  new ObservableCollection<LyricPart>(currentSong?.Lyrics?.LyricParts!);
            });
        }
    }
    
    private bool AreListsEqual(ObservableCollection<LyricPart> lyricPartsList1, LyricPart[] lyricPartsList2)
    {
        if (lyricPartsList2.IsNullOrEmpty())
            return false;
        
        if (!DataValidator.ValidateData(lyricPartsList1) || !DataValidator.ValidateData(lyricPartsList2))
            return false;
        
        if (lyricPartsList2.Length != lyricPartsList1.Count)
            return false;
        
        for (int i = 0; i < lyricPartsList1.Count; i++)
        {
            LyricPart currentPart1 = lyricPartsList1[i];

            for (int j = 0; j < lyricPartsList2.Length; j++)
            {
                LyricPart currentPart2 = lyricPartsList2[i];
                if (!currentPart1.Equals(currentPart2))
                    return false;
            }
        }

        return true;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType == EventType.POST)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (this._lyricParts.IsNullOrEmpty())
                return;
            
            this._lyricParts.Clear();
            this._lyricPart = null;
        });
    }

    public ObservableCollection<LyricPart> CurrentLyricParts
    {
        get => _lyricParts;
        set
        {
            _lyricParts = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricParts"));
        }
    }
    
    public LyricPart CurrentLyricPart
    {
        get => _lyricPart;
        set
        {
            _lyricPart = value;
            _oldLyricPart = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricPart"));
        }
    }
    
    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            _oldPercentage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
        }
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