using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Services.Services
{
    interface IService
    {
        string ServiceName();
        Task StartAuthorization();
        Task RefreshToken();

        string GetAccessToken();
        bool IsConnected();
    }
}
