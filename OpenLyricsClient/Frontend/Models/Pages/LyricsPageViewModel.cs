using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Frontend.Models.Pages;

public class LyricsPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private TaskSuspensionToken _songInfoSuspensionToken;

    private string _currentSongName;
    private string _currentArtists;
    private string _currentAlbumName;
    private double _currentPercentage;
    private string _currentTime;
    private string _currentMaxTime;

    private string _artwork;
    
    public LyricsPageViewModel()
    {
        Core.INSTANCE.TickHandler += OnTickHandler;
    }

    private void OnTickHandler(object sender)
    {
        Song song = Core.INSTANCE.SongHandler.CurrentSong;
        
        if (!DataValidator.ValidateData(song))
            return;

        SongName = song.SongMetadata.Name;
        Artists = song.SongMetadata.FullArtists;
        AlbumName = song.SongMetadata.Album;
        Percentage = song.GetPercentage();
        CurrentTime = song.ProgressString;
        CurrentMaxTime = song.MaxProgressString;

        if (DataValidator.ValidateData(song.Artwork))
        {
            Artwork artwork = song.Artwork;

            if (DataValidator.ValidateData(artwork))
            {
                if (!DataValidator.ValidateData(this._artwork) || 
                      DataValidator.ValidateData(this._artwork) && !this._artwork.Equals(artwork))
                    Artwork = artwork.FilePath;
            }
        }
    }

    public string SongName
    {
        get => this._currentSongName;
        set
        {
            this._currentSongName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongName"));
        }
    }
    
    public string Artists
    {
        get => this._currentArtists;
        set
        {
            this._currentArtists = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artists"));
        }
    }
    
    public string AlbumName
    {
        get => this._currentAlbumName;
        set
        {
            this._currentAlbumName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumName"));
        }
    }
    
    public double Percentage
    {
        get => this._currentPercentage;
        set
        {
            this._currentPercentage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
        }
    }

    public string CurrentTime
    {
        get => this._currentTime;
        set
        {
            this._currentTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentTime"));
        }
    }
    
    public string CurrentMaxTime
    {
        get => this._currentMaxTime;
        set
        {
            this._currentMaxTime = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentMaxTime"));
        }
    }
    
    public string Artwork
    {
        get => this._artwork;
        set
        {
            this._artwork = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Artwork"));
        }
    }

    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}