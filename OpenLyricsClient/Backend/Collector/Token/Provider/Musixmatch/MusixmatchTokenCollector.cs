using DevBase.Async.Task;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusixmatchClientLib.Auth;
using OpenLyricsClient.Backend.Settings.Sections.Tokens;
using OpenLyricsClient.Shared.Structure;

namespace OpenLyricsClient.Backend.Collector.Token.Provider.Musixmatch
{
    public class MusixmatchTokenCollector : ITokenCollector
    {
        private Debugger<MusixmatchTokenCollector> _debugger;
        private readonly int _tokenLimit;

        public static MusixmatchTokenCollector Instance; 

        public MusixmatchTokenCollector()
        {
            this._tokenLimit = 8;
            this._debugger = new Debugger<MusixmatchTokenCollector>(this);

            Instance = this;
        }

        public async Task CollectToken()
        {
            List<MusixMatchToken> tokens = Core.INSTANCE.SettingsHandler.Settings<TokenSection>()
                .GetValue<List<MusixMatchToken>>("Tokens");
            
            try
            {
                if (!DataValidator.ValidateData(tokens))
                    return;

                if (tokens.Count > this._tokenLimit)
                    return;

                string token = await new MusixmatchToken("").IssueNewTokenAsync();
                long expiresIn = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeMilliseconds();

                MusixMatchToken mxmToken = new MusixMatchToken();
                mxmToken.Token = token;
                mxmToken.ExpirationDate = expiresIn;
                mxmToken.Usage = 5;

                await Core.INSTANCE.SettingsHandler.Settings<TokenSection>()!.AddToken(mxmToken);

                this._debugger.Write("Requested new musixmatch token", DebugType.INFO);
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }

            //Check expiration date
            for (int i = 0; i < tokens.Count; i++)
            {
                MusixMatchToken token = tokens[i];

                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() >
                    token.ExpirationDate)
                {
                    await Core.INSTANCE.SettingsHandler.Settings<TokenSection>()!.RemoveToken(token);
                }
            }
        }
        
        public async Task<MusixMatchToken> GetToken()
        {
            List<MusixMatchToken> tokens = Core.INSTANCE.SettingsHandler.Settings<TokenSection>()
                .GetValue<List<MusixMatchToken>>("Tokens");

            if (tokens.Count == 0)
                return null;
            
            MusixMatchToken token = tokens[new Random().Next(0, tokens.Count)];
            Core.INSTANCE.SettingsHandler.Settings<TokenSection>()?.RemoveUsage(token);

            return token;
        }
    }
}
