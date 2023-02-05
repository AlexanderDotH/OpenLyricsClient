using System;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DevBase.Avalonia.Scaling;
using ReactiveUI;

namespace OpenLyricsClient.Frontend.Models.Windows
{
    public class MainWindowViewModel : ViewModelBase, IViewModel
    {
        public ReactiveCommand<Unit, Unit> CloseButtonActionCommand { get; }
        public ReactiveCommand<Unit, Unit> ExpandButtonActionCommand { get; }
        public ReactiveCommand<Unit, Unit> HideButtonActionCommand { get; }
        public ReactiveCommand<Unit, Unit> FullButtonActionCommand { get; }

        private bool _isMaximized;
        private bool _isFullScreen;
        private double _prevWidth;
        private double _prevHeight;
        
        public MainWindowViewModel()
        {
            CloseButtonActionCommand = ReactiveCommand.Create(CloseButtonAction);
            ExpandButtonActionCommand = ReactiveCommand.Create(ExpandButtonAction);
            HideButtonActionCommand = ReactiveCommand.Create(HideButtonAction);
            FullButtonActionCommand = ReactiveCommand.Create(FullButtonAction);

            this._isFullScreen = false;
            this._isMaximized = false;
            this._prevHeight = 0;
            this._prevWidth = 0;
        }

        private void CloseButtonAction()
        {
            Environment.Exit(0);
        }
        
        private void ExpandButtonAction()
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!this._isMaximized)
                {
                    desktop.MainWindow.WindowState = WindowState.Maximized;
                    this._isMaximized = true;
                }
                else
                {
                    desktop.MainWindow.WindowState = WindowState.Normal;
                    this._isMaximized = false;
                }
            }
        }
        
        private void HideButtonAction()
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!this._isFullScreen)
                {
                    desktop.MainWindow.WindowState = WindowState.Minimized;
                }
            }
        }
        
        private void FullButtonAction()
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!this._isFullScreen)
                {
                    this._prevWidth = desktop.MainWindow.Width;
                    this._prevHeight = desktop.MainWindow.Height;
                    desktop.MainWindow.WindowState = WindowState.FullScreen;
                    this._isFullScreen = true;
                }
                else
                {
                    desktop.MainWindow.WindowState = WindowState.Normal;
                    desktop.MainWindow.Width = this._prevWidth;
                    desktop.MainWindow.Height = this._prevHeight;
                    this._isFullScreen = false;
                }
            }
        }

        public bool IsResizing { get; set; }
    }
}
