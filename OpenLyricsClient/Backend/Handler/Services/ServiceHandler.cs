using System;
using DevBase.Generic;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Handler.Services.Services.Spotify;
using OpenLyricsClient.Backend.Handler.Services.Services.Tidal;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Services
{
    class ServiceHandler : IHandler
    {
        private GenericList<IService> _services;
        private Debugger<ServiceHandler> _debugger;

        public ServiceHandler()
        {
            this._debugger = new Debugger<ServiceHandler>(this);

            this._services = new GenericList<IService>();

            this._services.Add(new SpotifyService());
            this._services.Add(new TidalService());
        }

        public bool IsConnected(string serviceName)
        {
            return GetServiceByName(serviceName).IsConnected();
        }

        public string GetAccessToken(IService service)
        {
            IService s = this._services.FindEntry(service);

            if (!DataValidator.ValidateData(s))
                return null;

            return s.GetAccessToken();
        }

        public string GetAccessToken(string serviceName)
        {
            return GetAccessToken(GetServiceByName(serviceName));
        }

        public void AuthorizeService(string serviceName)
        {
            GetServiceByName(serviceName).StartAuthorization();
        }

        public IService GetServiceByName(string serviceName)
        {
            for (int i = 0; i < this._services.Length; i++)
            {
                IService s = this._services.Get(i);
                if (s.ServiceName().Equals(serviceName))
                {
                    return s;
                }
            }
            
            return null;
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
