using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Frontend.Scaling;
using OpenLyricsClient.Frontend.Structure;
using Xilium.CefGlue.Avalonia;

namespace OpenLyricsClient.Frontend.View.Windows.Auth;

public partial class CefAuthWindow : ScalableWindow
{
    private string _authURL;
    private string _authCompleteIdentifier;
    
    private bool _isComplete;
    private string _accessToken;
    private string _refreshToken;

    private AvaloniaCefBrowser _authWebView;

    public CefAuthWindow()
    {
        InitializeComponent();
        
        Decorator webViewContainer = this.Get<Decorator>(nameof(WebViewContainer));
        
        AvaloniaCefBrowser browser = new AvaloniaCefBrowser();
        browser.Initialized += BrowserOnInitialized;
        browser.AddressChanged += BrowserOnAddressChanged;

        webViewContainer.Child = browser;

        this._authWebView = browser;
    }
    
    public CefAuthWindow(string authUrl, string authCompleteIdentifier) : this()
    {
        this._authURL = authUrl;
        this._authCompleteIdentifier = authCompleteIdentifier;
        
        this._isComplete = false;
        this._accessToken = string.Empty;
        this._refreshToken = string.Empty;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void BrowserOnInitialized(object? sender, EventArgs e)
    {
        this._authWebView.Address = this._authURL;
    }

    private void BrowserOnAddressChanged(object sender, string address)
    {
        if (address.Contains(this._authCompleteIdentifier))
        {
            Regex regex = new Regex(@"(refresh_token=([\w\W]+)((access_token=([\w\W]+))))");
            if (regex.IsMatch(address))
            {
                Match match = regex.Match(address);

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

    public override event EventHandler<PointerPressedEventArgs> BeginResize;
    public override event EventHandler<PointerEventArgs> Resize;
    public override event EventHandler<PointerReleasedEventArgs> EndResize;
}