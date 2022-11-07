using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenLyricsClient.Backend;
using ReactiveUI;

namespace OpenLyricsClient.Frontend.Models.Pages;

public class SettingsPageViewModel : INotifyPropertyChanged
{
    private ObservableCollection<string> _lyricsSelectionMode;

    public ReactiveCommand<Unit, Unit> ConnectToSpotify { get; }
    public ReactiveCommand<Unit, Unit> DisconnectFromSpotify { get; }

    private bool _spotifyConnected;
    
    public SettingsPageViewModel()
    {
        DisconnectFromSpotify = ReactiveCommand.Create(DisconnectSpotify);
        ConnectToSpotify = ReactiveCommand.CreateFromTask(StartSpotifyAuthFlow);
        
        /*Task.Factory.StartNew(async () =>
        {
            while (!Core.IsDisposed())
            {
                await Task.Delay(500);
                SpotifyConnected = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected;
            }
        });*/
    }

    public ObservableCollection<string> LyricsSelectionMode
    {
        get
        {
            this._lyricsSelectionMode = new ObservableCollection<string>();
            this._lyricsSelectionMode.Add("Quality");
            this._lyricsSelectionMode.Add("Performance");
            return this._lyricsSelectionMode;
        }
    }

    public void DisconnectSpotify()
    {
        Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = false;
        Core.INSTANCE.SettingManager.WriteSettings();
    }
    
    public async Task StartSpotifyAuthFlow()
    {
        Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");

        /*long untilTime = DateTimeOffset.Now.AddMinutes(5).ToUnixTimeMilliseconds();
        
        while (!Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected && 
               DateTimeOffset.Now.ToUnixTimeMilliseconds() < untilTime)
        {
            await Task.Delay(500);
            SpotifyConnected = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected;
        }*/
    }
    

    public bool SpotifyConnected
    {
        get { return _spotifyConnected; }
        set
        {
            this._spotifyConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpotifyConnected"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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