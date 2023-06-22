using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.UI.Events.EventArgs;
using OpenLyricsClient.UI.Events.EventHandler;

namespace OpenLyricsClient.UI.View.Windows
{
    public partial class MainWindow : Scaling.ScalableWindow
    {
        private static MainWindow INSTANCE;

        private bool _windowDragable;
        
        //Pages
        private Carousel _pageSelector;

        //Buttons
        private Button _lyricsButton;
        private Button _fulltextButton;
        private Button _settingsButton;

        private ATupleList<int, UserControl> _pageList;

        private TimeSpan _animationSpan;

        public event PageSelectionChangedEventHandler PageSelectionChanged;
        public event PageSelectionChangedFinishedEventHandler PageSelectionChangedFinished;
        
        public MainWindow()
        {
            INSTANCE = this;
            InitializeComponent();

            this._pageList = new ATupleList<int, UserControl>();
            this._pageList.Add(0, this.Get<UserControl>(nameof(LyricsPage)));
            this._pageList.Add(1, this.Get<UserControl>(nameof(FullLyricsPage)));
            this._pageList.Add(2, this.Get<UserControl>(nameof(SettingsPage)));
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual; // center doesn't work on osx
                this.Position = new PixelPoint(0, 0); // initialize window in the top left corner
                this.Padding = new Thickness(0, 28, 0, 0); // pad the buttons from top
                this.CRD_WindowDecoration.IsVisible = false; // hide original buttons
                this.CRD_WindowDecoration_FullScreen.IsVisible = false; // hide fullscreen button
            }

            this._windowDragable = true;

            this._pageSelector = this.Get<Carousel>(nameof(PageSelection));
            this._pageSelector.SelectedIndex = 0;
            
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

            UserControl from = this._pageList.FindEntry(this._pageSelector.SelectedIndex);
            UserControl to = this._pageList.FindEntry(pageID);
            
            if (pageID > this._pageSelector.ItemCount)
                this._pageSelector.SelectedIndex = 0;

            switch (pageID)
            {
                case 0:
                {
                    UnselectAll();
                    SelectButton(this.BTN_LyricsButton);
                    this._windowDragable = true;
                    PageChanged(from, to);
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
                    //this._windowDragable = false;
                    PageChanged(from, to);
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
        
        protected virtual void PageChanged(UserControl from, UserControl to)
        {
            PageSelectionChangedEventHandler handler = PageSelectionChanged;
            handler.Invoke(this, new PageSelectionChangedEventArgs(from, to));
            
            this._animationSpan = ((PageSlide)PageSelection.PageTransition).Duration;

            DateTimeOffset until = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)this._animationSpan.TotalMilliseconds);

            Task.Factory.StartNew(async () =>
            {
                while (until > DateTimeOffset.Now) { await Task.Delay(100); }

                PageChangedFinished(from, to);
            });

        }

        protected virtual void PageChangedFinished(UserControl from, UserControl to)
        {
            PageSelectionChangedFinishedEventHandler handler = PageSelectionChangedFinished;
            handler.Invoke(this, new PageSelectionChangedEventArgs(from, to));
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
