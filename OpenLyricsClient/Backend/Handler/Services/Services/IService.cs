using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Services.Services
{
    interface IService
    {
        Task StartAuthorization();
        Task<bool> TestConnection();
        bool CanSeek();
        Task<bool> Seek(long position);
        void Dispose();
        string Name { get; }
        string ProcessName { get; }
        string AccessToken { get; }
        bool Connected { get; }
        bool Active { get; set; }
    }
}
