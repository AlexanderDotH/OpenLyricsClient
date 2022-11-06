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

namespace OpenLyricsClient.External.CefNet.View;

public partial class CefAuthWindow : Window
{
    private string _authURL;
    private string _authCompleteIDentifier;
    
    private bool _isComplete;
    private string _returnValue;

    private WebView _authWebView;

    public CefAuthWindow()
    {
        this._authURL = "https://google.de";
        this._authCompleteIDentifier = "/callback";
        
        this._isComplete = false;
        this._returnValue = string.Empty;
        
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
        this._returnValue = string.Empty;

        this._authWebView = this.FindControl<WebView>(nameof(AuthWebView));
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
        Debug.WriteLine(e.Url);
        if (e.Url.Contains(this._authCompleteIDentifier))
        {
            Regex regex = new Regex(@"(refresh_token=([\w\W]+)((access_token=([\w\W]+))))");
            if (regex.IsMatch(e.Url))
            {
                Match match = regex.Match(e.Url);

                if (match.Groups.Count >= 5)
                {
                    string refresh_token = match.Groups[2].Value;
                    string access_token = match.Groups[5].Value;
                    
                    this._isComplete = true;
                    this.Close();
                }
            }
        }
    }

    public Task<string> GetAuthCode()
    {
        TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
        
        Task.Factory.StartNew(async () =>
        {
            while (!this._isComplete)
            {
                await Task.Delay(500);
                completionSource.SetResult(this._returnValue);
            }
        });

        return completionSource.Task;
    }
}