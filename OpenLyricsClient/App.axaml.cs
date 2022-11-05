using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Frontend.Models.Windows;
using OpenLyricsClient.Frontend.View.Windows;

namespace OpenLyricsClient
{
    public partial class App : Application
    {
        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                desktop.Startup += Startup;
                desktop.Exit += Exit;
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private void Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            FrameworkInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            FrameworkShutdown?.Invoke(this, EventArgs.Empty);
        }
    }
}
