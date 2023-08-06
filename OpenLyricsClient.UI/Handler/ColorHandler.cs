using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Avalonia.Color.Extensions;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Events.EventHandler;

namespace OpenLyricsClient.UI.Handler;

public class ColorHandler
{
    // The dynamic ones
    private SolidColorBrush _primaryThemeColorBrush;
    private SolidColorBrush _primaryThemeFontColorBrush;

    private SolidColorBrush _secondaryThemeColorBrush;
    private SolidColorBrush _secondaryThemeFontColorBrush;
    
    private SolidColorBrush _lightThemeFontColorBrush;
    private SolidColorBrush _selectedLineFontColorBrush;
    private SolidColorBrush _unselectedLineFontColorBrush;

    public event ColorResourceUpdatedEventHandler ColorResourceUpdated;
    
    public ColorHandler()
    {
        this._primaryThemeColorBrush = FromResource("PrimaryThemeColorBrush");
        this._primaryThemeFontColorBrush = FromResource("PrimaryThemeFontColorBrush");

        this._secondaryThemeColorBrush = FromResource("SecondaryThemeColorBrush");
        this._secondaryThemeFontColorBrush = FromResource("SecondaryThemeFontColorBrush");
        
        this._lightThemeFontColorBrush = FromResource("LightThemeFontColorBrush");
        this._selectedLineFontColorBrush = FromResource("SelectedLineFontColorBrush");
        this._unselectedLineFontColorBrush = FromResource("UnSelectedLineFontColorBrush");
        
        Core.INSTANCE.ColorHandler.ColorPaletteChangedEvent += ColorHandlerOnColorPaletteChangedEvent;
    }

    private void ColorHandlerOnColorPaletteChangedEvent(object sender, ColorPaletteChangedEventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateColor(this._primaryThemeColorBrush, args.ColorPalette.PrimaryColor.Dark);
            UpdateColor(this._secondaryThemeColorBrush, args.ColorPalette.PrimaryColor.Dark);

            if (args.ColorPalette.PrimaryColor.Dark.BrightnessPercentage() > 30)
            {
                UpdateColor(this._primaryThemeFontColorBrush, args.ColorPalette.PrimaryForegroundColor.Light);
                UpdateColor(this._secondaryThemeFontColorBrush, args.ColorPalette.SecondaryForegroundColor.Light);

                UpdateColor(this._selectedLineFontColorBrush, args.ColorPalette.SelectedLineColor.Light);
                UpdateColor(this._unselectedLineFontColorBrush, args.ColorPalette.UnSelectedLineColor.Light);
            }
            else
            {
                UpdateColor(this._primaryThemeFontColorBrush, args.ColorPalette.PrimaryForegroundColor.Dark);
                UpdateColor(this._secondaryThemeFontColorBrush, args.ColorPalette.SecondaryForegroundColor.Dark);

                UpdateColor(this._selectedLineFontColorBrush, args.ColorPalette.SelectedLineColor.Dark);
                UpdateColor(this._unselectedLineFontColorBrush, args.ColorPalette.UnSelectedLineColor.Dark);
            }

            UpdateColor(this._lightThemeFontColorBrush, args.ColorPalette.PrimaryColor.Light);
            
            OnColorResourceUpdated();
        });
    }
    
    private SolidColorBrush FromResource(string resourceName)
    {
        return App.Current.FindResource(resourceName) as SolidColorBrush;
    }
    
    private void UpdateColor(SolidColorBrush colorBrush, Avalonia.Media.Color color)
    {
        colorBrush.Color = color;
    }

    private void OnColorResourceUpdated()
    {
        this.ColorResourceUpdated.Invoke(this);
    }
}