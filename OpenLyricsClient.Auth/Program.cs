using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.ReactiveUI;
using CommandLine;
using OpenLyricsClient.Auth.Configuration;
using OpenLyricsClient.Auth.Worker;

namespace OpenLyricsClient.Auth;

class Program
{
    public static WebViewConfiguration WebViewConfiguration;

    [STAThread]
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<WebViewConfiguration>(args)
            .WithParsed<WebViewConfiguration>(c =>
            {
                WebViewConfiguration = c;
            }).WithNotParsed((e) =>
            {
                Debug.WriteLine($"Could not parse required arguments");

                foreach (Error error in e.ToArray())
                {
                    if (error is MissingRequiredOptionError)
                    {
                        MissingRequiredOptionError err = error as MissingRequiredOptionError;
                        Debug.WriteLine($"You forgot to set \"{err?.NameInfo.LongName}\"");
                    }
                }

                throw new Exception("Missing requirements");
            });
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()
            {
                //UseWindowsUIComposition = false
            })
            .AfterSetup((a) =>
            {
                new BackgroundWorker();
            })
            .LogToTrace()
            .UseReactiveUI();
}