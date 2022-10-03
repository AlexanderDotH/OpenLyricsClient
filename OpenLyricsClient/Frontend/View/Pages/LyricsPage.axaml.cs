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
    
    public LyricsPage()
    {
        InitializeComponent();

        this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));
        this._cstmLyricsDisplay = this.Get<LyricsScroller>(nameof(LRC_Display));

        LyricData lyrics = new LyricData(LyricReturnCode.SUCCESS);
        lyrics.LyricParts = new LyricPart[]
        {
            new LyricPart(0, "Ich never nick nick up"),
            new LyricPart(1, "Nickgers in der nacht"),
            new LyricPart(2, "Schau, was hab ich mit den nicks gemacht"),
            new LyricPart(3, "Denn nicks sind immer nackt"),
            new LyricPart(4, "Yeah, Yeah, Yeah"),
            new LyricPart(5, "Yeah, Yeah, Yeah"),
            new LyricPart(6, "..."),
            new LyricPart(7, "Ich habe nicks im Keller"),
            new LyricPart(8, "Der eine ist baumfäller"),
            new LyricPart(9, "Der andere ist fallensteller"),
            new LyricPart(10, "Der andere ist ein teller"),
            new LyricPart(11, "..."),
            new LyricPart(12, "Yeah, Yeah, Yeah"),
            new LyricPart(13, "Yeah, Yeah, Yeah"),
            new LyricPart(14, "..."),
            new LyricPart(15, "Turn up"),
            new LyricPart(16, "Nick"),
        };


        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        
        Core.INSTANCE.TaskRegister.RegisterTask(
            out _displayLyricsSuspensionToken, 
            new Task(async () => await DisplayLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.SHOW_LYRICS);
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
    int currentLine = 0;

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Get<LyricsScroller>(nameof(LRC_Display)).SelectedLine = currentLine;
        currentLine++;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }
}