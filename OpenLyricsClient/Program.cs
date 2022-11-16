using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System;
using CefNet;
using OpenLyricsClient.Backend;
using OpenLyricsClient.External.CefNet.Utils;

namespace OpenLyricsClient
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // CefSetup cefSetup = new CefSetup();
            // cefSetup.SetupCef();
            //
            // App.FrameworkShutdown += (sender, args) =>
            // {
            //     cefSetup.CefNetApplication.Shutdown();
            // };
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .AfterSetup(t =>
                {
                    new Core();
                })
                .UseReactiveUI();
    }
}
