using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DevBase.Web;
using Microsoft.Extensions.Primitives;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Frontend.Structure;
using ReactiveUI;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings.Providers;

public class SettingsSpotifyViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ReactiveCommand<Unit, Unit> ConnectToSpotify { get; }
    public ReactiveCommand<Unit, Unit> DisconnectFromSpotify { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsSpotifyViewModel()
    {
        Core.INSTANCE.SettingManager.SettingsChanged += SettingManagerOnSettingsChanged;
        
        DisconnectFromSpotify = ReactiveCommand.Create(DisconnectSpotify);
        ConnectToSpotify = ReactiveCommand.CreateFromTask(StartSpotifyAuthFlow);
    }
    
    public void DisconnectSpotify()
    {
        Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = false;
        Core.INSTANCE.SettingManager.WriteSettings();
    }
    
    public async Task StartSpotifyAuthFlow()
    {
        Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UserGreeting"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UserFollower"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UserPlan"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConnected"));
    }

    public string UserGreeting
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Good ");
            
            switch (DateTime.Now.Hour)
            {
                case < 12:
                    stringBuilder.Append("morning");
                    break;

                case < 19:
                    stringBuilder.Append("midday");
                    break;

                case < 24:
                    stringBuilder.Append("evening");
                    break;
            }

            stringBuilder.Append(string.Format(" {0}!", Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.UserData?.DisplayName!));

            return stringBuilder.ToString();
        }
    }

    public string UserFollower
    {
        get
        {
            int? follower = Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.UserData?.Followers?.Total!;
            return string.Format("{0} follower", follower);
        }
    }
    
    public string UserPlan
    {
        get
        {
            string? product = Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.UserData?.Product;

            if (product.IsNullOrEmpty())
                return string.Empty;
            
            string formated = product?.Substring(0, 1).ToUpper() + product?.Substring(1, product.Length - 1);
            
            return string.Format("{0} Plan",  formated);
        }
    }

    public bool IsConnected
    {
        get => Core.INSTANCE.ServiceHandler.IsConnected("Spotify");
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