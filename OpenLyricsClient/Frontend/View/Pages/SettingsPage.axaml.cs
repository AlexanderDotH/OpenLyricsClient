using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Generics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Frontend.View.Windows;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class SettingsPage : UserControl
{
    private Button _btnRomanization;
    private Button _btnLyrics;
    private Button _btnSpotify;
    private Button _btnCache;
    private Button _btnProfile;
    private Button _btnCredits;

    private Carousel _pageSelector;

    private ATupleList<Button, int> _buttonList;
    
    private bool _loaded;

    private int _currentIndex;
    private int _oldIndex;
    
    public SettingsPage()
    {
        InitializeComponent();

        this._loaded = false;

        this._buttonList = new ATupleList<Button, int>();

        this._btnRomanization = this.Get<Button>(nameof(BTN_Romanization));
        this._btnLyrics = this.Get<Button>(nameof(BTN_Lyrics));
        this._btnSpotify = this.Get<Button>(nameof(BTN_Spotify));
        this._btnCache = this.Get<Button>(nameof(BTN_Cache));
        this._btnProfile = this.Get<Button>(nameof(BTN_Profile));
        this._btnCredits = this.Get<Button>(nameof(BTN_Credits));
        
        this._buttonList.AddRange(
            new Tuple<Button, int>(this._btnRomanization, 0), 
            new Tuple<Button, int>(this._btnLyrics, 1), 
            new Tuple<Button, int>(this._btnSpotify, 2), 
            new Tuple<Button, int>(this._btnCache, 3),
            new Tuple<Button, int>(this._btnProfile, 4),
            new Tuple<Button, int>(this._btnCredits, 5));
        
        this._pageSelector = this.Get<Carousel>(nameof(PageSelection));
        
        this._oldIndex = -1;
        
        SelectPage(0);

        UiThreadRenderTimer uiThreadRenderTimer = new UiThreadRenderTimer(60);
        uiThreadRenderTimer.Tick += span =>
        {
            if (this._currentIndex != this._oldIndex || this._oldIndex < 0)
            {
                Button element = this._buttonList.FindEntry(_currentIndex);
        
                element.Foreground = App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
                element.Background = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
                this._oldIndex = this._currentIndex;
            }
        };

        this._loaded = true;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.DragWindow(e);
    }

    private void UnselectAll()
    {
        this._buttonList.ForEach(b =>
        {
            b.Item1.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
            b.Item1.Background = App.Current.FindResource("SecondaryBackgroundBrush") as SolidColorBrush;
        });
    }

    private void SelectPage(int index)
    {
        this._oldIndex = -1;
        this._currentIndex = index;
        
        UnselectAll();
        this._pageSelector.SelectedIndex = index;
    }

    private void BTN_Romanization_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(0);
    }

    private void BTN_Lyrics_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(1);
    }

    private void BTN_Spotify_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(2);
    }

    private void BTN_Cache_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(3);
    }

    private void BTN_Profile_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(4);
    }

    private void BTN_Credits_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(5);
    }
}