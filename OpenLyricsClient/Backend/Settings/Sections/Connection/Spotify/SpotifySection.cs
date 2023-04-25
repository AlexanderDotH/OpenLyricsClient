using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Other;
using OpenLyricsClient.Backend.Utils;
using SpotifyAPI.Web;
using SimpleArtist = SpotifyAPI.Web.SimpleArtist;
using SimpleTrack = SpotifyAPI.Web.SimpleTrack;

namespace OpenLyricsClient.Backend.Settings.Sections.Connection.Spotify;

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
        await File.WriteAllTextAsync(this._file.FullName, this._data.ToString());
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

        this._data = JObject.Parse(reader.ReadToEnd());
        
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
}