using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DevBase.Web;
using Microsoft.Extensions.Primitives;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Frontend.Structure;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings.Providers;

public class SettingsSpotifyViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _userGreeting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsSpotifyViewModel()
    {
        Core.INSTANCE.SettingManager.SettingsChanged += SettingManagerOnSettingsChanged;
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UserGreeting"));
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