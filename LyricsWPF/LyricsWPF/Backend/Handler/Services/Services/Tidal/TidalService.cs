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
using LyricsWPF.Backend.Utils.Service;
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
                await Task.Delay(2000);

                if (Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected)
                {
                    if (DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.TidalAccess))
                    {
                        TidalAccess newestTidal = await GetNewestTidalAccess();

                        long savedTime = Core.INSTANCE.SettingManager.Settings.TidalAccess.ExpirationDate;
                        DateTimeOffset savedTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(savedTime);
                        DateTimeOffset currentTime = DateTimeOffset.Now;

                        if (currentTime > savedTimeOffset)
                        {
                            (string error, TidalAccess access) = await TidalUtils.RefreshAccessToken(newestTidal);

                            if (!DataValidator.ValidateData(access))
                            {
                                this._debugger.Write(error, DebugType.ERROR);
                                continue;
                            }

                            Core.INSTANCE.SettingManager.Settings.TidalAccess = access;
                            Core.INSTANCE.SettingManager.WriteSettings();

                            this._debugger.Write("Refreshed Tidal!", DebugType.INFO);
                        }
                    }

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

            if (await TidalUtils.TestTidalConnection(tidalAccess))
                tidalAccess.IsTidalConnected = true;

            return tidalAccess;
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
