using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet;
using CefNet.Avalonia;
using OpenLyricsClient.Backend;
using OpenLyricsClient.External.CefNet.Structure;

namespace OpenLyricsClient.External.CefNet.View;

public partial class CefAuthWindow : Window
{
    private string _authURL;
    private string _authCompleteIDentifier;
    
    private bool _isComplete;
    private string _accessToken;
    private string _refreshToken;

    private WebView _authWebView;

    public CefAuthWindow()
    {
        this._authURL = "https://google.de";
        this._authCompleteIDentifier = "/callback";
        
        this._isComplete = false;
        this._accessToken = string.Empty;
        this._refreshToken = string.Empty;
        
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    public CefAuthWindow(string authUrl, string authCompleteIDentifier) : this()
    {
        this._authURL = authUrl;
        this._authCompleteIDentifier = authCompleteIDentifier;
        
        this._isComplete = false;
        this._accessToken = string.Empty;
        this._refreshToken = string.Empty;
        
        this._authWebView = this.Get<WebView>(nameof(AuthWebView));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void WebView_OnBrowserCreated(object? sender, EventArgs e)
    {
        this._authWebView.Navigate(this._authURL);
    }

    private void AuthWebView_OnAddressChange(object? sender, AddressChangeEventArgs e)
    {
        if (e.Url.Contains(this._authCompleteIDentifier))
        {
            Regex regex = new Regex(@"(refresh_token=([\w\W]+)((access_token=([\w\W]+))))");
            if (regex.IsMatch(e.Url))
            {
                Match match = regex.Match(e.Url);

                if (match.Groups.Count >= 5)
                {
                    this._refreshToken = match.Groups[2].Value;
                    this._accessToken = match.Groups[5].Value;
                    this._isComplete = true;
                }
            }
        }
    }

    public async Task<Token> GetAuthCode()
    {
        while (!this._isComplete)
        {
            await Task.Delay(500);

            if (this._accessToken != string.Empty && 
                this._refreshToken != string.Empty)
            {
                await Task.Delay(2000);
                return new Token(this._accessToken, this._refreshToken);
            }
        }

        return null;
    }
}