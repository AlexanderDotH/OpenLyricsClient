using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CefNet.CApi;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Frontend.View.Custom;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class LyricsPage : UserControl
{
    private TextBlock _txtTimeFrom;
    private TextBlock _txtTimeTo;
    
    public LyricsPage()
    {
        InitializeComponent();

        this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));

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

        this.Get<LyricsDisplay>(nameof(LRC_Display)).LyricData = lyrics;

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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Get<LyricsDisplay>(nameof(LRC_Display)).SelectedLine++;
    }
}