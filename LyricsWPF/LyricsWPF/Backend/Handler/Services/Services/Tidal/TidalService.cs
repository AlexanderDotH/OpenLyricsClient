using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.IO;
using DevBase.Typography;
using DevBase.Utilities;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Json;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;
using TidalLib;

namespace LyricsWPF.Backend.Handler.Services.Services.Tidal
{
    internal class TidalService : IService
    {

        private bool _disposed;

        private Debugger<TidalService> _debugger;

        private Task _refeshTokenTask;

        public TidalService()
        {
            this._debugger = new Debugger<TidalService>(this);

            this._disposed = false;

            this._refeshTokenTask = new Task(async () => await RefreshToken(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None);
            this._refeshTokenTask.Start();
        }

        private async Task RefreshToken()
        {
            while (!this._disposed)
            {
                if (Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected)
                {
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.TidalAccess))
                    {
                        long savedTime = Core.INSTANCE.SettingManager.Settings.TidalAccess.ExpirationDate;

                        if (currentTime > savedTime)
                        {
                            TidalAccess tidalAccess = await GetNewestTidalAccess();

                            if (!DataValidator.ValidateData(tidalAccess))
                                continue;

                            Core.INSTANCE.SettingManager.Settings.TidalAccess = tidalAccess;
                            Core.INSTANCE.SettingManager.WriteSettings();
                        }
                    }

                    await Task.Delay(2000);
                }
            }
        }

        private async Task<TidalAccess> GetNewestTidalAccess()
        {
            AFileObject file = new AFileObject(
                FileUtils.SafeFileReadAccess(
                    new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TIDAL\\Logs\\app.log")));

            if (!file.FileInfo.Exists)
                return null;

            GenericList<string> fileContent = AFile.ReadFile(file.FileInfo).ToList();

            GenericList<JsonTidalAccess> tidalAccesses = new GenericList<JsonTidalAccess>();

            for (int i = 0; i < fileContent.Length; i++)
            {
                string line = fileContent[i];

                if (line.Contains("Session was changed "))
                {
                    if (i + 21 < fileContent.Length)
                    {
                        string content = StringUtils.StringArrayToString(fileContent.GetRangeAsList(i, i + 21).ToArray());

                        string regex =
                            "(\\(\\d{0,3}.\\/\\d{0,3}\\/\\d{0,3}.\\d{0,3}:\\d{0,3}:\\d{0,3}.\\d{0,3}\\).\\[[a-zA-Z]*\\].- Session was changed )";

                        if (Regex.IsMatch(content,regex))
                        {
                            content = content.Replace(Regex.Match(content, regex).Value, string.Empty);
                            content += "}";
                        }

                        JsonTidalAccess json = new JsonDeserializer<JsonTidalAccess>().Deserialize(content);

                        if (DataValidator.ValidateData(json))
                            tidalAccesses.Add(new JsonDeserializer<JsonTidalAccess>().Deserialize(content));
                    }
                }
            }

            if (tidalAccesses.Length <= 0)
                return null;

            JsonTidalAccess lastTidalAccess = tidalAccesses.Get(tidalAccesses.Length - 1);

            if (lastTidalAccess == null)
                return null;

            TidalAccess tidalAccess = new TidalAccess();
            tidalAccess.UserID = lastTidalAccess.UserId;
            tidalAccess.ApiToken = lastTidalAccess.ApiToken;
            tidalAccess.UniqueKey = lastTidalAccess.ClientUniqueKey;
            tidalAccess.AccessToken = lastTidalAccess.OAuthAccessToken;
            tidalAccess.RefreshToken = lastTidalAccess.OAuthRefreshToken;
            tidalAccess.ExpirationDate = lastTidalAccess.OAuthExpirationDate;

            if (await TestTidalConnection(tidalAccess))
                tidalAccess.IsTidalConnected = true;

            return tidalAccess;
        }

        private async Task<bool> TestTidalConnection(TidalAccess tidalAccess)
        {
            (string s, LoginKey lg) = await Client.Login(tidalAccess.AccessToken);
            return lg != null;
        }

        public string ServiceName()
        {
            return "Tidal";
        }

        public async Task StartAuthorization()
        {
            Core.INSTANCE.SettingManager.Settings.TidalAccess = await GetNewestTidalAccess();
            Core.INSTANCE.SettingManager.WriteSettings();
        }

        public string GetAccessToken()
        {
            return Core.INSTANCE.SettingManager.Settings.TidalAccess.AccessToken;
        }

        public bool IsConnected()
        {
            return Core.INSTANCE.SettingManager.Settings.TidalAccess != null && Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected;
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
