namespace OpenLyricsClient.UI.Structure;

public class Token
{
    private string _accessToken;
    private string _refreshToken;

    public Token(string accessToken, string refreshToken)
    {
        this._accessToken = accessToken;
        this._refreshToken = refreshToken;
    }

    public string AccessToken
    {
        get => this._accessToken;
    }

    public string RefreshToken
    {
        get => this._refreshToken;
    }
}