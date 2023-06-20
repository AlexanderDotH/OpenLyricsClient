using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Auth.Holder;

public class RegexHolder
{
    public static string AuthProviderToRegex(EnumAuthProvider provider)
    {
        switch (provider)
        {
            case EnumAuthProvider.SPOTIFY:
            {
                return @"(refresh_token=([\w\W]+)((access_token=([\w\W]+))))";
                break;
            }
        }

        return string.Empty;
    }
}