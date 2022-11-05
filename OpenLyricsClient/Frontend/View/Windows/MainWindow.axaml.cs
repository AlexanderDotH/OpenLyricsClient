using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CefNet.Avalonia;
using CefNet.WinApi;

namespace OpenLyricsClient.Frontend.View.Windows
{
    public partial class MainWindow : Window
    {
        private static MainWindow INSTANCE;

        private bool _windowDragable;
        
        //Pages
        private Carousel _pageSelector;

        //Buttons
        private Button _lyricsButton;
        private Button _fulltextButton;
        private Button _settingsButton;

        public MainWindow()
        {
            InitializeComponent();

            INSTANCE = this;
            this._windowDragable = true;

            this._pageSelector = this.Get<Carousel>(nameof(PageSelection));
            
            this._lyricsButton = this.Get<Button>(nameof(BTN_LyricsButton));
            this._fulltextButton = this.Get<Button>(nameof(BTN_FullTextButton));
            this._settingsButton = this.Get<Button>(nameof(BTN_SettingsButton));
        }

        private void LyricsPage_Click(object? sender, RoutedEventArgs e)
        {
            SelectPage(0);
        }

        private void FullTextPage_Click(object? sender, RoutedEventArgs e)
        {
            SelectPage(1);
        }
        
        private void SettingsPage_Click(object? sender, RoutedEventArgs e)
        {
            SelectPage(2);
        }

        private void SelectPage(int pageID)
        {
            if (this._pageSelector.ItemCount == 0)
                return;
            
            if (pageID > this._pageSelector.ItemCount)
                this._pageSelector.SelectedIndex = 0;

            switch (pageID)
            {
                case 0:
                {
                    UnselectAll();
                    SelectButton(this.BTN_LyricsButton);
                    this._windowDragable = true;
                    break;
                }
                case 1:
                {
                    UnselectAll();
                    SelectButton(this.BTN_FullTextButton);
                    this._windowDragable = true;
                    break;
                }
                case 2:
                {
                    UnselectAll();
                    SelectButton(this.BTN_SettingsButton);
                    this._windowDragable = false;
                    break;
                }
            }
            
            this._pageSelector.SelectedIndex = pageID;
        }

        private void SelectButton(Button button)
        {
            button.Foreground = App.Current.FindResource("PrimaryFontColorBrush") as SolidColorBrush;
        }
        
        private void UnselectAll()
        {
            this.BTN_LyricsButton.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
            this.BTN_FullTextButton.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
            this.BTN_SettingsButton.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
        }

        public static MainWindow Instance
        {
            get => INSTANCE;
        }
        
        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (this._windowDragable)
                this.BeginMoveDrag(e);
        }

        private void WebView_OnBrowserCreated(object? sender, EventArgs e)
        {
            this.Get<WebView>(nameof(WebView)).Navigate("http://google.de");
        }
    }
}
