using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.Shared.Utils;

public class AvaloniaUtils
{
    public static bool IsInPreviewerMode()
    {
        try
        {
            using (PlatformManager.DesignerMode())
            {
                AvaloniaXamlLoader.IRuntimeXamlLoader loader = AvaloniaLocator.Current.GetService<AvaloniaXamlLoader.IRuntimeXamlLoader>();
                return loader != null;
            }
        }
        catch (Exception e)
        {
            return true;
        }
    }
}