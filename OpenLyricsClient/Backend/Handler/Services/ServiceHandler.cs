using System;
using System.Threading.Tasks;
using DevBase.Generics;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Handler.Services.Services.Spotify;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Handler.Services
{
    class ServiceHandler : IHandler
    {
        private AList<IService> _services;
        private Debugger<ServiceHandler> _debugger;

        public ServiceHandler()
        {
            this._debugger = new Debugger<ServiceHandler>(this);

            this._services = new AList<IService>();

            this._services.Add(new SpotifyService());
            //this._services.Add(new TidalService());
        }

        public bool IsConnected(string serviceName)
        {
            return GetServiceByName(serviceName).Connected;
        }

        public string GetAccessToken(IService service)
        {
            IService s = this._services.FindEntry(service);

            if (!DataValidator.ValidateData(s))
                return null;

            return s.AccessToken;
        }

        public void MarkServiceAsActive(IService service)
        {
            for (int i = 0; i < this._services.Length; i++)
            {
                IService s = this._services.Get(i);

                s.Active = false;
                
                if (s.Equals(service))
                    s.Active = true;
            }
        }

        public IService GetActiveService()
        {
            for (int i = 0; i < this._services.Length; i++)
            {
                IService s = this._services.Get(i);

                if (s.Active)
                    return s;
            }

            return null;
        }
        
        public string GetAccessToken(string serviceName)
        {
            return GetAccessToken(GetServiceByName(serviceName));
        }

        public async Task AuthorizeService(string serviceName)
        {
            await GetServiceByName(serviceName).StartAuthorization();
        }

        public IService GetServiceByName(string serviceName)
        {
            for (int i = 0; i < this._services.Length; i++)
            {
                IService s = this._services.Get(i);
                if (s.Name.Equals(serviceName))
                {
                    return s;
                }
            }
            
            return null;
        }

        public bool IsAnyConnected()
        {
            for (int i = 0; i < this._services.Length; i++)
            {
                if (this._services.Get(i).Connected)
                    return true;
            }

            return false;
        }
        
        public void Dispose()
        {
            try
            {
                for (int i = 0; i < this._services.Length; i++)
                {
                    this._services.Get(i).Dispose();
                }
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }
    }
}
