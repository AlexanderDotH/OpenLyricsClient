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

            Core.INSTANCE.TaskRegister.Register(
                out _displayLyricsSuspensionToken,
                new Task(async () => await DisplayLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SHOW_LYRICS);

            Core.INSTANCE.TaskRegister.Register(
                out _syncLyricsSuspensionToken,
                new Task(async () => await SyncLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SYNC_LYRICS);
            
            Core.INSTANCE.TaskRegister.Register(
                out _syncLyricsPercentageSuspensionToken,
                new Task(async () => await SyncLyricsPercentageTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SYNC_LYRICS_PERCENTAGE);

            Core.INSTANCE.SettingManager.SettingsChanged += (sender, args) =>
            {
                this.CurrentLyricParts = null;
            };
        }
    }

    private async Task DisplayLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._displayLyricsSuspensionToken.WaitForRelease();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(currentSong))
                    return;

                if (!DataValidator.ValidateData(currentSong.Lyrics))
                    return;

                if (!DataValidator.ValidateData(currentSong.Lyrics.LyricParts))
                    return;

                if (!DataValidator.ValidateData(this.CurrentLyricParts))
                {
                    this.CurrentLyricParts = new ObservableCollection<LyricPart>();
                    return;
                }
                
                if (this.CurrentLyricParts == null)
                    return;
                    
                if (!(AreListsEqual(this.CurrentLyricParts, currentSong.Lyrics.LyricParts)))
                {
                    this.CurrentLyricParts =  new ObservableCollection<LyricPart>(currentSong.Lyrics.LyricParts);
                }
            });
        }
    }

    private async Task SyncLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._syncLyricsSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

            if (!DataValidator.ValidateData(currentSong))
                continue;

            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
                continue;

            if (!IsLyricPartEqual(this.CurrentLyricPart, currentSong.CurrentLyricPart))
            {
                this.CurrentLyricPart = currentSong.CurrentLyricPart;
            }
        }
    }

    private bool IsLyricPartEqual(LyricPart part1, LyricPart part2)
    {
        if (!DataValidator.ValidateData(part1) || !DataValidator.ValidateData(part2))
            return false;

        if (!DataValidator.ValidateData(part1.Part) || !DataValidator.ValidateData(part2.Part))
            return false;
        
        if (!DataValidator.ValidateData(part1.Time) || !DataValidator.ValidateData(part2.Time))
            return false;
        
        return part1.Time.Equals(part2.Time) && part1.Part.Equals(part2.Part);
    }
    
    private async Task SyncLyricsPercentageTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._syncLyricsPercentageSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

            if (!DataValidator.ValidateData(currentSong))
                continue;

            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
                continue;

            if (!DataValidator.ValidateData(this._lyricParts))
                continue;
            
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