using System.Net;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Listener
{
    class Listener
    {
        private string _prefix;
        private string _suffix;
        private string _response;
        private string _lookFor;

        private bool _running;

        public Listener(string prefix, string suffix, string lookFor)
        {
            this._prefix = prefix;
            this._lookFor = lookFor;
            this._suffix = suffix;

            Task task = new Task(async () => await StartListener(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None);
            task.Start();
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
                if (request.RawUrl.StartsWith(this._suffix))
                {
                    var options = c.Request.QueryString;
                    this._response = options.Get(this._lookFor);
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

        public string Response
        {
            get
            {
                return this._response;
            }
        }
    }
}
