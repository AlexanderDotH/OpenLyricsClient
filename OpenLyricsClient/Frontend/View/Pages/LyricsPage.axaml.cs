using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DevBase.Async.Task;
using Material.Styles;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Models.Pages;
using OpenLyricsClient.Frontend.View.Windows;
using Squalr.Engine.Utils.Extensions;
using LyricsScroller = OpenLyricsClient.Frontend.View.Custom.LyricsScroller;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class LyricsPage : UserControl
{
    private TextBlock _txtTimeFrom;
    private TextBlock _txtTimeTo;
    private LyricsScroller _cstmLyricsDisplay;
    private Grid _presenterGrid;
    private Card _cardBar;

    private Border _artworkBorder;
    
    private Image _artworkImage;
    private string _oldImagePath;
    
    private LyricsPageViewModel _lyricsPageViewModel;
    
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;

    public LyricsPage()
    {
        InitializeComponent();

        this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));
        this._cstmLyricsDisplay = this.Get<LyricsScroller>(nameof(LRC_Display));
        this._presenterGrid = this.Get<Grid>(nameof(GRD_Content));
        this._cardBar = this.Get<Card>(nameof(CRD_Bar));
        
        if (this.DataContext is LyricsPageViewModel)
        {
            LyricsPageViewModel dataContext = (LyricsPageViewModel)this.DataContext;
            this._lyricsPageViewModel = dataContext;
        }
        
        Image image = new Image();
        image.Width = 320;
        image.Height = 320;
        image.VerticalAlignment = VerticalAlignment.Center;
        image.HorizontalAlignment = HorizontalAlignment.Center;
        image.Margin = new Thickness(0, 0, 0, 60);

        SolidColorBrush primaryBackColor = App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        
        if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
            primaryBackColor = App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
        
        Border border = new Border();
        border.Width = 328;
        border.Height = 328;
        border.VerticalAlignment = VerticalAlignment.Center;
        border.HorizontalAlignment = HorizontalAlignment.Center;
        border.Margin = new Thickness(0, 0, 0, 60);
        border.BorderThickness = new Thickness(5);
        border.BorderBrush = primaryBackColor;
        border.CornerRadius = new CornerRadius(8);

        this._artworkBorder = border;
        
        this._artworkImage = image;
        this._oldImagePath = string.Empty;
        
        this._presenterGrid.Children.Add(image);
        this._presenterGrid.Children.Add(border);
            
        if (!DataValidator.ValidateData(this._lyricsPageViewModel))
            return;

        this._lyricsPageViewModel.PropertyChanged += DataContextOnPropertyChanged;
    }

    private void DataContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName.IsNullOrEmpty())
            return;

        if (e.PropertyName.Equals("Artwork") && !this._oldImagePath.Equals(this._lyricsPageViewModel.Artwork))
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AsyncImageLoader.ImageLoader.SetSource(this._artworkImage, this._lyricsPageViewModel.Artwork);
                } 
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    this._artworkImage.Source = new Bitmap(this._lyricsPageViewModel.Artwork);
                }
                
                this._oldImagePath = this._lyricsPageViewModel.Artwork;
            });
        }

        if (e.PropertyName.Equals("UiBackground"))
        {
            SolidColorBrush primaryBackColor = App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.ArtworkBackground)
                primaryBackColor = App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;

            this._artworkBorder.BorderBrush = primaryBackColor;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 1;
        this._txtTimeTo.Opacity = 1;
        this._cardBar.Height = 60 * App.INSTANCE.ScalingManager.CurrentScaling;
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 0;
        this._txtTimeTo.Opacity = 0;
        this._cardBar.Height = 40 * App.INSTANCE.ScalingManager.CurrentScaling;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (this._cstmLyricsDisplay != null)
        {
            /*this._cstmLyricsDisplay.Reset();
            this._cstmLyricsDisplay.Reload();
            this._cstmLyricsDisplay.ResyncOffset();*/
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this._cstmLyricsDisplay != null)
        {
            this._cstmLyricsDisplay.ResyncOffset();
        }
    }

    public string SongName
    {
        get { return "Never gonna give you up"; }
    }
    
    public string Artists
    {
        get { return "Rick Astley"; }
    }
    
    public string AlbumName
    {
        get { return "Example Album"; }
    }

    public string CurrentTime
    {
        get { return "0:00"; }
    }

    public string CurrentMaxTime
    {
        get { return "10:00"; }
    }
    
    public double Percentage
    {
        get { return 0; }
    }
}