using System;
using System.Threading.Tasks;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Enum;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Handler.License;

public class LicenseHandler : IHandler
{
    private TaskSuspensionToken _refreshSuspensionToken;

    private bool _disposed;

    private JsonOpenLyricsClientSubscriptionModel _license;
    private DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient _openLyricsClientApi;
    
    public LicenseHandler()
    {
        this._disposed = false;

        this._openLyricsClientApi = new DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient(Core.INSTANCE.Sealing.ServerPublicKey);

        JsonOpenLyricsClientSubscriptionModel model = new JsonOpenLyricsClientSubscriptionModel
        {
            Model = EnumSubscriptions.OPENLYRICSCLIENT_STANDARD
        };
        this._license = model;
        
        Core.INSTANCE.TaskRegister.Register(
            out _refreshSuspensionToken, 
            new Task(async () => await RefreshLicense(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.REFRESH_LICENSE);
    }

    private async Task RefreshLicense()
    {
        while (!this._disposed)
        {
            string userID = Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserID");
            string userSecret = Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserSecret");

            JsonOpenLyricsClientSubscription subscription = new JsonOpenLyricsClientSubscription
            {
                UserID = userID,
                UserSecret = userSecret
            };

            JsonOpenLyricsClientSubscriptionModel model = await this._openLyricsClientApi.CheckSubscription(subscription);
            this._license = model;
            
            await Task.Delay((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
        }
    }

    public void Dispose()
    {
        this._disposed = true;
        Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.REFRESH_LICENSE);
    }

    public JsonOpenLyricsClientSubscriptionModel License
    {
        get => this._license;
    }
}