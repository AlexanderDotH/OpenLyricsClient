using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DevBase.Avalonia.Scaling;
using OpenLyricsClient.Frontend.Models.Windows;
using OpenLyricsClient.Frontend.View.Windows;

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
                
                manager.SetScaling(0.75d);
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
