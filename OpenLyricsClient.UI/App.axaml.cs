using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Handler;
using OpenLyricsClient.UI.Models.Windows;
using OpenLyricsClient.UI.View.Windows;
using ScalingManager = OpenLyricsClient.UI.Scaling.ScalingManager;
using ScalingProvider = OpenLyricsClient.UI.Scaling.ScalingProvider;

namespace OpenLyricsClient.UI
{
    public partial class App : Application
    {
        public static event EventHandler FrameworkInitialized;
        public static event EventHandler FrameworkShutdown;

        private ScalingManager _scalingManager;
        private ColorHandler _colorHandler;

        public static App INSTANCE;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            INSTANCE = this;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            this._colorHandler = new ColorHandler();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindowViewModel windowViewModel = new MainWindowViewModel();

                MainWindow mainWindow = new MainWindow
                {
                    DataContext = windowViewModel,
                };

                ScalingManager manager = ScalingProvider.Register(mainWindow, windowViewModel);
                
                this._scalingManager = manager;

                desktop.MainWindow = mainWindow;
                
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

        public ColorHandler ColorHandler
        {
            get => _colorHandler;
        }
    }
}
