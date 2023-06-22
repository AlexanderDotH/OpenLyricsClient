using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.UI.View.Windows;

namespace OpenLyricsClient.UI.View.Pages.Settings;

public partial class SettingsRomanization : UserControl
{
    public SettingsRomanization()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    #region Japanese Section

    private void ExpandJapaneseSection(object? sender, PointerPressedEventArgs e)
    {
        Expander expander = this.Get<Expander>(nameof(PART_ExpanderJapanese));
        expander.IsExpanded = !expander.IsExpanded;
    }

    private void ExpandJapaneseSectionEnter(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = false;
    }

    private void ExpandJapaneseSectionLeave(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = true;
    }

    #endregion
    
    #region Korean Section

    private void ExpandKoreanSection(object? sender, PointerPressedEventArgs e)
    {
        Expander expander = this.Get<Expander>(nameof(PART_ExpanderKorean));
        expander.IsExpanded = !expander.IsExpanded;
    }

    private void ExpandKoreanSectionEnter(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = false;
    }

    private void ExpandKoreanSectionLeave(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = true;
    }

    #endregion
    
    #region Russian Section

    private void ExpandRussianSection(object? sender, PointerPressedEventArgs e)
    {
        Expander expander = this.Get<Expander>(nameof(PART_ExpanderRussian));
        expander.IsExpanded = !expander.IsExpanded;
    }

    private void ExpandRussianSectionEnter(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = false;
    }

    private void ExpandRussianSectionLeave(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = true;
    }

    #endregion
    
}