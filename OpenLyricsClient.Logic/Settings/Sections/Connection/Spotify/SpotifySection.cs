using DevBase.Generics;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Other;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Logic.Settings.Sections.Connection.Spotify;

public class SpotifySection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public SpotifySection(string filePath)
    {
        this._file = new FileInfo(filePath);
    }
    
    public async Task WriteToDisk()
    {
        string encoded = Core.INSTANCE.Sealing.SimpleEncrypt(this._data.ToString());
        await File.WriteAllTextAsync(this._file.FullName, encoded);
    }

    public async Task ReadFromDisk()
    {
        if (!this._file.Exists)
        {
            this._data = Defaults();
            await WriteToDisk();
            return;
        }

        await using FileStream stream = this._file.OpenRead();
        using StreamReader reader = new StreamReader(stream);

        string decoded = Core.INSTANCE.Sealing.SimpleDecrypt(reader.ReadToEnd());
        
        this._data = JObject.Parse(decoded);
        
        await stream.FlushAsync();
        
        stream.Close();
        reader.Close();
    }

    public T GetValue<T>(string field)
    {
        return (T)this._data[field].ToObject<T>();
    }

    public async Task SetValue<T>(string field, T value)
    {
        this._data[field] = JToken.FromObject(value);
        await WriteToDisk();
    }

    public JObject Defaults()
    {
        PrivateUser privateUser = new PrivateUser
        {
            Country = string.Empty,
            Email = string.Empty,
            Followers = null,
            Href = string.Empty,
            Id = string.Empty,
            Images = null,
            Product = string.Empty,
            Type = "",
            Uri = "",
            DisplayName = ""
        };

        SpotifyStatistics statistics = new SpotifyStatistics
        {
            TopTracks = null,
            TopArtists = null
        };
        
        SpotifyAccess spotifyAccess = new SpotifyAccess
        {
            AccessToken = "null",
            IsSpotifyConnected = false,
            RefreshToken = string.Empty,
            SpotifyExpireTime = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            UserData = privateUser,
            Statistics = statistics
        };
        
        return new JsonDeserializer().Serialize(spotifyAccess);
    }
    
    public string[] GetFields()
    {
        AList<string> fields = new AList<string>();

        foreach (var pair in this._data)
            fields.Add(pair.Key);

        return fields.GetAsArray();
    }
}