using OpenLyricsClient.Shared.Communication;

namespace OpenLyricsClient.Logic.Communication.Services;

public class AuthenticationService : IInterProcessService
{
    public void Authentication(string flowID, object access)
    {
        Core.INSTANCE.AuthenticationPipe.AddAuthflowResult(flowID, access);
    }
}