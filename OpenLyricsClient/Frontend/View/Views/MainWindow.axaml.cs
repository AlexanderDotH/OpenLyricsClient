using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace OpenLyricsClient.Frontend.View.Views
{
    public partial class MainWindow : Window
    {

        //Pages
        private Carousel _pageSelector;

        //Buttons
        private Button _lyricsButton;
        private Button _fulltextButton;
        private Button _settingsButton;

        public MainWindow()
        {
            InitializeComponent();

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
                    break;
                }
                case 1:
                {
                    UnselectAll();
                    SelectButton(this.BTN_FullTextButton);
                    break;
                }
                case 2:
                {
                    UnselectAll();
                    SelectButton(this.BTN_SettingsButton);
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
    }
}
