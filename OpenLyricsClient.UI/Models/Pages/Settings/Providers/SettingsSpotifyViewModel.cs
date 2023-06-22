using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Settings.Sections.Connection.Spotify;
using ReactiveUI;
using SpotifyAPI.Web;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.UI.Models.Pages.Settings.Providers;

public class SettingsSpotifyViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ReactiveCommand<Unit, Unit> ConnectToSpotify { get; }
    public ReactiveCommand<Unit, Unit> DisconnectFromSpotify { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsSpotifyViewModel()
    {
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingManagerOnSettingsChanged;
        
        DisconnectFromSpotify = ReactiveCommand.CreateFromTask(DisconnectSpotify);
        ConnectToSpotify = ReactiveCommand.Create(StartSpotifyAuthFlow);
    }
    
    private async Task DisconnectSpotify()
    {
        Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().SetValue("IsSpotifyConnected", false);
        await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "IsSpotifyConnected");
    }
    
    private void StartSpotifyAuthFlow()
    {
        /*if (Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected == false && 
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess?.Statistics != null &&
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess?.UserData?.DisplayName != string.Empty)
        {
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess!.IsSpotifyConnected = true;
            Core.INSTANCE.SettingManager.WriteSettings();
        }
        else
        {
            await Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");
        }*/
        Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        if (settingschangedeventargs.Section != typeof(SpotifySection))
            return;
        
        OnPropertyChanged("UserGreeting");
        OnPropertyChanged("UserFollower");
        OnPropertyChanged("UserPlan");
        OnPropertyChanged("IsConnected");
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

            stringBuilder.Append(string.Format(" {0}!", Core.INSTANCE?.SettingsHandler?.Settings<SpotifySection>()?.GetValue<PrivateUser>("UserData")?.DisplayName));

            return stringBuilder.ToString();
        }
    }

    public string UserFollower
    {
        get
        {
            int? follower = Core.INSTANCE?.SettingsHandler?.Settings<SpotifySection>()?.GetValue<PrivateUser>("UserData")?.Followers?.Total;
            return string.Format("{0} follower", follower);
        }
    }
    
    public string UserPlan
    {
        get
        {
            string? product = Core.INSTANCE?.SettingsHandler?.Settings<SpotifySection>()?.GetValue<PrivateUser>("UserData")?.Product;

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