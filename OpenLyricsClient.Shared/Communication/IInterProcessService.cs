using OpenLyricsClient.Shared.Structure.Json;

namespace OpenLyricsClient.Shared.Communication;

public interface IInterProcessService
{
    void Authentication(string flowID, object access);
}