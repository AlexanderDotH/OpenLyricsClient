using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using OpenLyricsClient.Backend.Collector.Token.Provider.Musixmatch;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Collector.Token
{
    public class TokenCollector
    {

        private TaskSuspensionToken _collectTokenSuspensionToken;
        private GenericList<ITokenCollector> _tokenCollector;
        private bool _disposed;

        public TokenCollector()
        {
            this._tokenCollector = new GenericList<ITokenCollector>();
            this._tokenCollector.Add(new MusixmatchTokenCollector());

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _collectTokenSuspensionToken,
                new Task(async () => await this.TokenCollectionTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.COLLECT_TOKENS);
        }

        public async Task TokenCollectionTask()
        {
            while (!this._disposed)
            {
                await this._collectTokenSuspensionToken.WaitForRelease();

                for (int i = 0; i < this._tokenCollector.Length; i++)
                {
                    ITokenCollector collector = this._tokenCollector.Get(i);

                    await collector.CollectToken();
                }

                await Task.Delay(15000);
            }
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.COLLECT_TOKENS);
        }
    }
}
