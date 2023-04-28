using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Enum;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using ReactiveUI;
using TextCopy;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings;

public class SettingsUserPageViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ReactiveCommand<Unit, Unit> CopyIDCommand { get; }
    public ReactiveCommand<Unit, Unit> CopySecretCommand { get; }

    public SettingsUserPageViewModel()
    {
        CopyIDCommand = ReactiveCommand.CreateFromTask(CopyID);
        CopySecretCommand = ReactiveCommand.CreateFromTask(CopySecret);
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;

    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        if (settingschangedeventargs.Section != typeof(AccountSection))
            return;
        
        OnPropertyChanged("IsStandardMember");
        OnPropertyChanged("IsPlusMember");
        OnPropertyChanged("IsMasterMember");
    }

    private async Task CopyID()
    {
        await ClipboardService.SetTextAsync(UserID);
    }
    
    private async Task CopySecret()
    {
        await ClipboardService.SetTextAsync(UserSecret);
    }
    
    public string UserID
    {
        get => Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserID");
    }

    public string UserSecret
    {
        get => Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserSecret");
    }
    
    public string UserSecretCensored
    {
        get
        {
            string secret = UserSecret;

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < secret.Length; i++)
            {
                if (i > 5 && i < secret.Length - 5)
                {
                    stringBuilder.Append("*");
                }
                else
                {
                    stringBuilder.Append(secret[i]);
                }
            }

            return stringBuilder.ToString();
        }
    }

    public bool IsStandardMember
    {
        get => Core.INSTANCE.LicenseHandler.License.Model == EnumSubscriptions.OPENLYRICSCLIENT_STANDARD;
    }
    
    public bool IsPlusMember
    {
        get => Core.INSTANCE.LicenseHandler.License.Model == EnumSubscriptions.OPENLYRICSCLIENT_PLUS;
    }
    
    public bool IsMasterMember
    {
        get => Core.INSTANCE.LicenseHandler.License.Model == EnumSubscriptions.OPENLYRICSCLIENT_MASTER;
    }
    
    public EnumSubscriptions Subscription => Core.INSTANCE!.LicenseHandler!.License!.Model!;
    
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