using System.Net;
using System.Text.RegularExpressions;
using DevBase.Async.Task;
using OpenLyricsClient.Shared.Structure.Access;

namespace OpenLyricsClient.Logic.Authentication
{
    class Listener
    {
        private string _prefix;
        private string _suffix;
        private AccessToken _response;
        private string _refreshPrefix;
        private string _accessPrefix;

        private bool _running;

        private TaskSuspensionToken _suspensionToken;
        
        public Listener(string prefix, string suffix, string refreshPrefix, string accessPrefix)
        {
            this._prefix = prefix;
            this._suffix = suffix;

            this._refreshPrefix = refreshPrefix;
            this._accessPrefix = accessPrefix;
        }

        public async Task StartListener()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(_prefix);
            httpListener.Start();
            _running = true;

            while (httpListener.IsListening)
            {
                var c = await httpListener.GetContextAsync();

                HttpListenerRequest request = c.Request;
                if (request.RawUrl.Contains(this._suffix))
                {
                    string refreshToken = string.Empty;
                    string accessToken = string.Empty;
                    
                    string refreshRegex = "(refresh_token.)([\\w\\W]*(access_token))";
                    string accessRegex = "(access_token.)([\\w\\W]*)";
                    
                    if (Regex.IsMatch(request.RawUrl, refreshRegex))
                    {
                        refreshToken = Regex.Match(request.RawUrl, refreshRegex).Groups[2].Value;
                    }
                    
                    if (Regex.IsMatch(request.RawUrl, accessRegex))
                    {
                        accessToken = Regex.Match(request.RawUrl, accessRegex).Groups[2].Value;
                    }

                    AccessToken token = new AccessToken()
                    {
                        Access = accessToken,
                        Refresh = refreshToken
                    };
                    
                    this._response = token;
                    httpListener.Stop();
                }

                if (!_running) httpListener.Stop();
            }
        }

        public void StopListener()
        {
            this._running = false;
        }

        public bool Finished
        {
            get { return !this._running && this._response != null; }
        }

        public AccessToken Response
        {
            get
            {
                return this._response;
            }
        }
    }
}
