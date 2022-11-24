using DevBase.Async.Task;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusixmatchClientLib.Auth;
using OpenLyricsClient.Backend.Structure;

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
            bool settingsChanged = false;

            try
            {
                if (!DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.MusixMatchToken))
                    return;

                if (Core.INSTANCE.SettingManager.Settings.MusixMatchToken.Count > this._tokenLimit)
                    return;

                string token = await new MusixmatchToken("").IssueNewTokenAsync();
                long expiresIn = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeMilliseconds();

                MusixMatchToken mxmToken = new MusixMatchToken();
                mxmToken.Token = token;
                mxmToken.ExpirationDate = expiresIn;
                mxmToken.Usage = 5;

                Core.INSTANCE.SettingManager.Settings.MusixMatchToken.Add(mxmToken);

                _debugger.Write("Requested new musixmatch token", DebugType.INFO);

                settingsChanged = true;
            }
            catch (Exception e)
            {
                _debugger.Write(e);
            }

            //Check expiration date
            for (int i = 0; i < Core.INSTANCE.SettingManager.Settings.MusixMatchToken.Count; i++)
            {
                MusixMatchToken token = Core.INSTANCE.SettingManager.Settings.MusixMatchToken[i];

                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() >
                    token.ExpirationDate)
                {
                    Core.INSTANCE.SettingManager.Settings.MusixMatchToken.Remove(token);
                    settingsChanged = true;
                }
            }
            
            if (settingsChanged)
                Core.INSTANCE.SettingManager.WriteSettings(false);
        }
        
        public MusixMatchToken GetToken()
        {
            List<MusixMatchToken> tokens = Core.INSTANCE.SettingManager.Settings.MusixMatchToken;

            if (tokens.Count == 0)
                return null;
            
            MusixMatchToken token = tokens[new Random().Next(0, tokens.Count)];
            token.Usage--;

            if (token.Usage <= 0)
            {
                Core.INSTANCE.SettingManager.Settings.MusixMatchToken.Remove(token);
            }
            
            return token;
        }
    }
}
