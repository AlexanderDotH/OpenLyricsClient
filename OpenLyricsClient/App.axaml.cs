using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Models.Windows;
using OpenLyricsClient.Frontend.View.Windows;
using ScalingManager = OpenLyricsClient.Frontend.Scaling.ScalingManager;
using ScalingProvider = OpenLyricsClient.Frontend.Scaling.ScalingProvider;

namespace OpenLyricsClient
{
    public partial class App : Application
    {
        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;

        private ScalingManager _scalingManager;

        public static App INSTANCE;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            INSTANCE = this;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindowViewModel windowViewModel = new MainWindowViewModel();

                MainWindow mainWindow = new MainWindow
                {
                    DataContext = windowViewModel,
                };

                desktop.MainWindow = mainWindow;
                
                ScalingManager manager = ScalingProvider.Register(mainWindow, windowViewModel);
                
                this._scalingManager = manager;

                desktop.Startup += Startup;
                desktop.Exit += Exit;
                
                manager.OnlyScaleOnStartup = true;
                manager.SetScaling(WindowUtils.GetScalingFactor());
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

        public ScalingManager ScalingManager
        {
            get => _scalingManager;
        }
    }
}
