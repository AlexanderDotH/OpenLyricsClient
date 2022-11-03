using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Frontend.View.Windows;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class SettingsPage : UserControl
{
    private ComboBox _comboboxMode;
    
    public SettingsPage()
    {
        InitializeComponent();

        this._comboboxMode = this.Get<ComboBox>(nameof(CMBX_LyricsSelection));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!this._comboboxMode.IsDropDownOpen)
            MainWindow.Instance.BeginMoveDrag(e);
    }
}