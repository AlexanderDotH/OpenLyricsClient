using System;
using System.Collections.Generic;
using System.Threading;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Services.Services;
using LyricsWPF.Backend.Handler.Services.Services.Spotify;

namespace LyricsWPF.Backend.Handler.Services
{
    class ServiceHandler : IHandler
    {
        private List<IService> _services;
        private Debugger<ServiceHandler> _debugger;

        public ServiceHandler()
        {
            this._debugger = new Debugger<ServiceHandler>(this);

            this._services = new List<IService>();
            this._services.Add(new SpotifyService());
        }
        
        public bool IsConnected(string serviceName)
        {
            return GetServiceByName(serviceName).IsConnected();
        }

        public string GetAccessToken(IService service)
        {
            foreach (IService s in _services)
            {
                if (service.Equals(s))
                { 
                    return s.GetAccessToken();
                }
            }

            return null;
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
            foreach (IService s in _services)
            {
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
                for (int i = 0; i < this._services.Count; i++)
                {
                    this._services[i].Dispose();
                }
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }
    }
}
