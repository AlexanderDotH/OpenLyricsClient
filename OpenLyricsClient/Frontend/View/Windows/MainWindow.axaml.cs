using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ScalableWindow = OpenLyricsClient.Frontend.Scaling.ScalableWindow;

namespace OpenLyricsClient.Frontend.View.Windows
{
    public partial class MainWindow : ScalableWindow
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual; // center doesn't work on osx
                this.Position = new PixelPoint(0, 0); // initialize window in the top left corner
                this.Padding = new Thickness(0, 28, 0, 0); // pad the buttons from top
                this.CRD_WindowDecoration.IsVisible = false; // hide original buttons
                this.CRD_WindowDecoration_FullScreen.IsVisible = false; // hide fullscreen button
            }

            INSTANCE = this; 
            this._windowDragable = true;

            this._pageSelector = this.Get<Carousel>(nameof(PageSelection));
            
            this._lyricsButton = this.Get<Button>(nameof(BTN_LyricsButton));
            //this._fulltextButton = this.Get<Button>(nameof(BTN_FullTextButton));
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

        public void SelectPage(int pageID)
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
                /*case 1:
                {
                    UnselectAll();
                    SelectButton(this.BTN_FullTextButton);
                    this._windowDragable = true;
                    break;
                }*/
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
            //this.BTN_FullTextButton.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
            this.BTN_SettingsButton.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
        }

        public static MainWindow Instance
        {
            get => INSTANCE;
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            DragWindow(e);
        }

        public void DragWindow(PointerPressedEventArgs e)
        {
            if (!this._windowDragable)
                return;
            
            this.BeginMoveDrag(e);
        }

        public override event EventHandler<PointerPressedEventArgs> BeginResize;
        public override event EventHandler<PointerEventArgs> Resize;
        public override event EventHandler<PointerReleasedEventArgs> EndResize;
        
        public bool WindowDragable
        {
            get => _windowDragable;
            set => _windowDragable = value;
        }
    }
}
