using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using DevBase.Async.Task;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.Models.Custom;

public class LyricsScrollerViewModel : INotifyPropertyChanged
{
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsPercentageSuspensionToken;

    public ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private double _percentage;

    private RomanizationHelper _romanizationHelper;

    public LyricsScrollerViewModel()
    {
        this._romanizationHelper = new RomanizationHelper();
        
        if (!AvaloniaUtils.IsInPreviewerMode())
        {
            
            this._lyricParts = new ObservableCollection<LyricPart>();
            this.CurrentLyricParts =  new ObservableCollection<LyricPart>();

            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
            
            Core.INSTANCE.TickHandler += OnLyricsLoadTickHandler;
            Core.INSTANCE.TickHandler += OnLyricsSyncTickHandler;
            Core.INSTANCE.TickHandler += OnLyricsSyncPercentageTickHandler;

            Core.INSTANCE.SettingManager.SettingsChanged += (sender, args) =>
            {
                this.CurrentLyricParts = null;
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
            
        for (var i = 0; i < this._lyricParts.Count; i++)
        {
            if (this._lyricParts[i] == currentSong.CurrentLyricPart)
            {
                if (i + 1 < this._lyricParts.Count)
                {
                    LyricPart nextPart = this._lyricParts[i + 1];
                        
                    long time = nextPart.Time - currentSong.CurrentLyricPart.Time;
                    long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                    double change = Math.Round((double)(100 * currentTime) / time);
                        
                    Percentage = change;
                }
                else
                {
                    long time = currentSong.SongMetadata.MaxTime - currentSong.CurrentLyricPart.Time;
                    long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                    double change = Math.Round((double)(100 * currentTime) / time);
                        
                    Percentage = change;
                }
            }
        }
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
            return;
        
        if (currentSong.Lyrics.LyricParts == null)
            return;

        if (!DataValidator.ValidateData(this.CurrentLyricParts))
        {
            this.CurrentLyricParts = new ObservableCollection<LyricPart>();
            return;
        }
        
        if (!(AreListsEqual(this.CurrentLyricParts, currentSong.Lyrics.LyricParts)))
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                this.CurrentLyricParts =  new ObservableCollection<LyricPart>(currentSong.Lyrics.LyricParts);
            });
        }
    }
    
    private bool AreListsEqual(ObservableCollection<LyricPart> lyricPartsList1, LyricPart[] lyricPartsList2)
    {
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
                if (!currentPart1.Part.Equals(currentPart2.Part))
                    return false;
            }
        }

        return true;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.POST)
            return;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricPart"));
        }
    }
    
    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
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