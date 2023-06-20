using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenLyricsClient.Shared.Communication;

namespace OpenLyricsClient.Auth.Communication.Client;

public class InterProcessService
{
    private IIpcClient<IInterProcessService> _client;
    
    public InterProcessService()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddNamedPipeIpcClient<IInterProcessService>("AuthWindow", pipeName: Program.WebViewConfiguration.Pipe)
            .BuildServiceProvider();

        IIpcClientFactory<IInterProcessService> clientFactory = serviceProvider
            .GetRequiredService<IIpcClientFactory<IInterProcessService>>();

        this._client = clientFactory.CreateClient("AuthWindow");
    }

    public void Invoke(string flowID, object access)
    {
        this._client.InvokeAsync(o => o.Authentication(flowID, access));
    }
}