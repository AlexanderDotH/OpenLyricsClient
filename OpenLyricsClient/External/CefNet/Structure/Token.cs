using System;

namespace OpenLyricsClient.External.CefNet.Structure;

public class Token
{
    private string _accessToken;
    private string _refreshToken;

    public Token(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
    }

    public string AccessToken
    {
        get => _accessToken;
    }

    public string RefreshToken
    {
        get => _refreshToken;
    }
}