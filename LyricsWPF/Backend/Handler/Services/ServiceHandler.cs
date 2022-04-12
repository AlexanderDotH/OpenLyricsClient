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
        private Thread _thread;
        private bool _disposed;

        public ServiceHandler()
        {
            this._debugger = new Debugger<ServiceHandler>(this);

            this._services = new List<IService>();
            this._services.Add(new SpotifyService());

            this._thread = new Thread(ManageRefresh);
            this._thread.Start();

            this._disposed = false;
        }

        private void ManageRefresh()
        {
            while (!this._disposed)
            {
                Thread.Sleep(5000);

                if (Core.INSTANCE.Settings.IsSpotifyConnected)
                {
                    if (Core.INSTANCE.Settings.BearerAccess != null)
                    {
                        DateTime? expire = Core.INSTANCE.Settings.SpotifyExpireTime;
                        DateTime expiresTime = expire.Value.AddMinutes(Core.INSTANCE.Settings.BearerAccess.ExpiresIn / 60);

                        if (DateTime.Now > expiresTime)
                        {
                            GetServiceByName("Spotify").RefreshToken();
                            this._debugger.Write("Refreshed Spotify", DebugType.DEBUG);
                        }
                    }
                }
            }
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
                this._thread.Abort();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }

            this._disposed = true;
        }
    }
}
