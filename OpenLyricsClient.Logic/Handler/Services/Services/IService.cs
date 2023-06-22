namespace OpenLyricsClient.Logic.Handler.Services.Services
{
    public interface IService
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
