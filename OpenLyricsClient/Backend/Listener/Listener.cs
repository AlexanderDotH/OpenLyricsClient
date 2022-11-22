using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.External.CefNet.Structure;

namespace OpenLyricsClient.Backend.Listener
{
    class Listener
    {
        private string _prefix;
        private string _suffix;
        private Token _response;
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

                    this._response = new Token(accessToken, refreshToken);
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

        public Token Response
        {
            get
            {
                return this._response;
            }
        }
    }
}
