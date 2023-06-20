using System;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using OpenLyricsClient.Auth.Communication.Client;
using OpenLyricsClient.Auth.Holder;
using OpenLyricsClient.Shared.Structure.Access;
using Xilium.CefGlue.Avalonia;

namespace OpenLyricsClient.Auth.Views;

public partial class MainWindow : Window
{
    private AvaloniaCefBrowser _webView;

    private InterProcessService _service;
    
    public MainWindow()
    {
        InitializeComponent();

        this.Width = Program.WebViewConfiguration.Width;
        this.Height = Program.WebViewConfiguration.Height;
        
        this._webView = BuildWebView();

        Decorator container = this.Get<Decorator>(nameof(Auth.Views.MainWindow.WebViewContainer));
        container.Child = this._webView;

        this._service = new InterProcessService();
    }

    private AvaloniaCefBrowser BuildWebView()
    {
        AvaloniaCefBrowser view = new AvaloniaCefBrowser();

        view.Initialized += (sender, args) =>
        {
            view.Address = Program.WebViewConfiguration.AuthEndpoint;
        };

        view.AddressChanged += (sender, address) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Title = address;
            });
            
            if (address.Contains(Program.WebViewConfiguration.CompleteIdentifier))
            {
                Regex regex = new Regex(RegexHolder.AuthProviderToRegex(Program.WebViewConfiguration.Provider));
                if (regex.IsMatch(address))
                {
                    Match match = regex.Match(address);

                    if (match.Groups.Count >= 5)
                    {
                        AccessToken token = new AccessToken()
                        {
                            Access = match.Groups[5].Value,
                            Refresh = match.Groups[2].Value
                        };
                        
                        this._service.Invoke(Program.WebViewConfiguration.Flow, token);
                    }
                }
            }
        };

        return view;
    }
}