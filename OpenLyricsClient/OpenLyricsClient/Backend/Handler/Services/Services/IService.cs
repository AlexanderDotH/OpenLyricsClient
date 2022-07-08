using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Services.Services
{
    interface IService
    {
        string ServiceName();
        Task StartAuthorization();

        string GetAccessToken();
        bool IsConnected();

        void Dispose();

        string ProcessName();
    }
}
