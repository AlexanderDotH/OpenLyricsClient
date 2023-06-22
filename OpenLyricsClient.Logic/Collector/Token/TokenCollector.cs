using DevBase.Async.Task;
using DevBase.Generics;
using OpenLyricsClient.Logic.Collector.Token.Provider.Musixmatch;
using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Logic.Collector.Token
{
    public class TokenCollector
    {
        private TaskSuspensionToken _collectTokenSuspensionToken;
        private AList<ITokenCollector> _tokenCollector;
        private bool _disposed;

        public TokenCollector()
        {
            this._tokenCollector = new AList<ITokenCollector>();
            this._tokenCollector.Add(new MusixmatchTokenCollector());

            Core.INSTANCE.TaskRegister.Register(
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

                await Task.Delay(20000);
            }
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.COLLECT_TOKENS);
        }
    }
}
