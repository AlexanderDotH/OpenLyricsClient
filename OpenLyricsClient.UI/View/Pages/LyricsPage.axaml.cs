using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DevBase.Api.Apis.Deezer.Structure.Json;
using DevBase.Async.Task;
using Material.Styles;
using Material.Styles.Controls;
using Microsoft.Extensions.Logging.EventSource;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Palette;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Events.EventArgs;
using OpenLyricsClient.UI.Extensions;
using OpenLyricsClient.UI.Models.Pages;
using OpenLyricsClient.UI.View.Custom;
using OpenLyricsClient.UI.View.Custom.Badges.Lyrics.Providers;
using OpenLyricsClient.UI.View.Windows;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.UI.View.Pages;

public partial class LyricsPage : UserControl
{
    //private TextBlock _txtTimeFrom;
    //private TextBlock _txtTimeTo;
    private LyricsScroller _cstmLyricsDisplay;
    //private Grid _presenterGrid;
    //private Card _cardBar;

    private Border _artworkBorder;

    private Border _percentagePanel;
    private Grid _informationBorder;
    private Viewbox _artworkViewBox;
    
    private Image _artworkImage;
    private string _oldImagePath = string.Empty;

    private AiLyricsBadge _aiLyricsBadge;
    
    private LyricsPageViewModel _lyricsPageViewModel;
    
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;

    private SolidColorBrush _primaryColor;
    private SolidColorBrush _foreColor;
    
    public LyricsPage()
    {
        InitializeComponent();

        //this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        //this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));
        this._cstmLyricsDisplay = this.Get<LyricsScroller>(nameof(LRC_Display));
        //this._presenterGrid = this.Get<Grid>(nameof(GRD_Content));
        //this._cardBar = this.Get<Card>(nameof(CRD_Bar));

        this._percentagePanel = this.Get<Border>(nameof(CTRL_PercentagePanel));
        this._informationBorder = this.Get<Grid>(nameof(CTRL_InformationBorder));
        this._artworkViewBox = this.Get<Viewbox>(nameof(CTRL_ViewBoxCover));

        this._aiLyricsBadge = this.Get<AiLyricsBadge>(nameof(CTRL_AiBadge));
        
        if (this.DataContext is LyricsPageViewModel model)
            this._lyricsPageViewModel = model;
        
        Image image = new Image();
        image.Width = 320;
        image.Height = 320;
        image.VerticalAlignment = VerticalAlignment.Center;
        image.HorizontalAlignment = HorizontalAlignment.Center;
        image.Margin = new Thickness(0, 0, 0, 0);

        SolidColorBrush primaryBackColor = App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        
        if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<bool>("Artwork Background") == true)
            primaryBackColor = App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
        
        Border border = new Border();
        border.Width = 328;
        border.Height = 328;
        border.VerticalAlignment = VerticalAlignment.Center;
        border.HorizontalAlignment = HorizontalAlignment.Center;
        border.Margin = new Thickness(0, 0, 0, 0);
        border.BorderThickness = new Thickness(5);
        border.BorderBrush = primaryBackColor;
        border.CornerRadius = new CornerRadius(8);

        this._artworkBorder = border;
        
        this._artworkImage = image;
        
        this._artworkViewBox.Child = image;
        
        /*this._presenterGrid.Children.Add(image);
        this._presenterGrid.Children.Add(border);*/
            
        if (!DataValidator.ValidateData(this._lyricsPageViewModel))
            return;

        MainWindow.Instance.PageSelectionChanged += InstanceOnPageSelectionChanged;
        
        this._lyricsPageViewModel.PropertyChanged += DataContextOnPropertyChanged;
    }

    private void InstanceOnPageSelectionChanged(object sender, PageSelectionChangedEventArgs pageselectionchanged)
    {
        if (pageselectionchanged.FromPage.GetType() == typeof(LyricsPage))
        {
            this._cstmLyricsDisplay.IsVisible = false;
        }
        else
        {
            this._cstmLyricsDisplay.IsVisible = true;
            
        }
    }

    private void DataContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!DataValidator.ValidateData(e))
            return;
        
        if (e.PropertyName.IsNullOrEmpty())
            return;

        /*if (e.PropertyName?.Equals("ColorPalette") == true)
        {
            ColorPalette palette = this._lyricsPageViewModel.ColorPalette;
            
            this._aiLyricsBadge.ForegroundColorBrush = new SolidColorBrush(palette.SecondaryForegroundColor.Dark);

            
            this._aiLyricsBadge.StartColor = this._lyricsPageViewModel.UiForeground.Color;
            this._aiLyricsBadge.EndColor = this._lyricsPageViewModel.UiForeground.AdjustBrightness(150).Color;

        }*/
        
        if (e.PropertyName?.Equals("Percentage") == true)
        {
            double scaled = this._informationBorder.Bounds.Width * 0.01;
            double upScaled = scaled * this._lyricsPageViewModel.Percentage;
            
            this._percentagePanel.Width = upScaled;
            this._percentagePanel.MaxWidth = this._informationBorder.Bounds.Width;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        /*this._txtTimeFrom.Opacity = 1;
        this._txtTimeTo.Opacity = 1;
        this._cardBar.Height = 60 * App.INSTANCE.ScalingManager.CurrentScaling;*/
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        /*this._txtTimeFrom.Opacity = 0;
        this._txtTimeTo.Opacity = 0;
        this._cardBar.Height = 40 * App.INSTANCE.ScalingManager.CurrentScaling;*/
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.DragWindow(e);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        /*if (this._cstmLyricsDisplay != null)
        {
            this._cstmLyricsDisplay.Resync();
        }*/
    }
}