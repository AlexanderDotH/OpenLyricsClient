using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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