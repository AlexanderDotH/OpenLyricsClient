using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Services.Services
{
    interface IService
    {
        string ServiceName();
        Task StartAuthorization();

        string GetAccessToken();
        bool IsConnected();
        Task<bool> TestConnection();

        void Dispose();

        string ProcessName();
    }
}
