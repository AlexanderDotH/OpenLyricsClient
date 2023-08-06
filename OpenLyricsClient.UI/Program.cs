using System;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using OpenLyricsClient.Logic;
using OpenLyricsClient.UI.Handler;

namespace OpenLyricsClient.UI
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(LogEventLevel.Fatal)
                .AfterSetup(t =>
                {
                    new Core();
                })
                .UseReactiveUI();
    }
}
