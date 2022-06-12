using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Services.Services
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
