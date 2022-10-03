using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CefNet.CApi;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.View.Custom.View;
using OpenLyricsClient.Frontend.View.Views;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class LyricsPage : UserControl
{
    private TextBlock _txtTimeFrom;
    private TextBlock _txtTimeTo;
    private LyricsScroller _cstmLyricsDisplay;
    
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;

    public LyricsPage()
    {
        InitializeComponent();

        this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));
        this._cstmLyricsDisplay = this.Get<LyricsScroller>(nameof(LRC_Display));

        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        
        Core.INSTANCE.TaskRegister.RegisterTask(
            out _displayLyricsSuspensionToken, 
            new Task(async () => await DisplayLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.SHOW_LYRICS);
        
        Core.INSTANCE.TaskRegister.RegisterTask(
            out _syncLyricsSuspensionToken, 
            new Task(async () => await SyncLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.SYNC_LYRICS);
    }

    private async Task DisplayLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._displayLyricsSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;
            
            if (!DataValidator.ValidateData(currentSong)) 
                continue;
            
            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this._cstmLyricsDisplay.LyricData = currentSong.Lyrics;
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

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this._cstmLyricsDisplay.CurrentLyricPart = currentSong.CurrentLyricPart;
            });
        }
    }
    
    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.POST)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._cstmLyricsDisplay.Reset();
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 1;
        this._txtTimeTo.Opacity = 1;
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 0;
        this._txtTimeTo.Opacity = 0;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }
}