using System.Threading.Tasks;
using DevBase.Async.Task;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenLyricsClient.Backend.Authentication;
using OpenLyricsClient.Backend.Communication.Services;
using OpenLyricsClient.Shared.Communication;
using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Backend.Communication;

public class InterProcessService
{
    private string _pipeName;

    private TaskSuspensionToken _hostTokenSuspensionToken;
    
    public InterProcessService(string pipeName, string[] args)
    {
        this._pipeName = pipeName;
        
        Core.INSTANCE.TaskRegister.Register(
            out _hostTokenSuspensionToken,
            new Task(async () => await IpcHost(pipeName, args), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
            EnumRegisterTypes.SERVICE_TASK);
    }

    private async Task IpcHost(string pipeName, string[] args)
    {
        IHost service = CreateHostBuilder(pipeName, args).Build();
        await service.RunAsync();
    }

    private IHostBuilder CreateHostBuilder(string pipename, string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddScoped<IInterProcessService, AuthenticationService>();
            })
            .ConfigureIpcHost(builder =>
            {
                builder.AddNamedPipeEndpoint<IInterProcessService>(pipeName: pipename);
            })
            .ConfigureLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            });

    public string PipeName
    {
        get => _pipeName;
    }
}