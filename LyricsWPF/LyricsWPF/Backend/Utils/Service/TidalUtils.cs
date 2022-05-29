using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Helper;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Json;
using TidalLib;

namespace LyricsWPF.Backend.Utils.Service
{
    public class TidalUtils
    {
        public static async Task<bool> TestTidalConnection(TidalAccess tidalAccess)
        {
            (string s, LoginKey lg) = await Client.Login(tidalAccess.AccessToken);
            return lg != null;
        }
        public static bool IsTidalRunning()
        {
            Process[] processes = Process.GetProcessesByName("TIDAL");

            for (int i = 0; i < processes.Length; i++)
            {
                Process p = processes[i];

                if (!string.IsNullOrWhiteSpace(p.MainWindowTitle))
                {
                    return true;
                }
            }

            return false;
        }

        public static Process FindTidalProcess()
        {
            Process[] processes = Process.GetProcessesByName("TIDAL");

            for (int i = 0; i < processes.Length; i++)
            {
                Process p = processes[i];

                if (!string.IsNullOrWhiteSpace(p.MainWindowTitle))
                {
                    return p;
                }
            }

            return null;
        }

        //zU4XHVVkc2tDPo4t id
        //VJKhDFqJPqvsPVNBV6ukXTJmwlvbttP7wlMlrc72se4= secret

        public static async Task<(string, TidalAccess)> RefreshAccessToken(TidalAccess tidalAccess, HttpHelper.ProxyInfo oProxy = null)
        {
            string clientID = "zU4XHVVkc2tDPo4t";
            string clientSecret = "VJKhDFqJPqvsPVNBV6ukXTJmwlvbttP7wlMlrc72se4=";

            string authorization = clientID + ":" + clientSecret;
            string base64 = System.Convert.ToBase64String(Encoding.Default.GetBytes(authorization));
            string header = $"Authorization: Basic {base64}";

            HttpHelper.Result result = await HttpHelper.GetOrPostAsync("https://auth.tidal.com/v1/oauth2/token", new Dictionary<string, string>(){
                {"client_id", clientID},
                {"refresh_token", tidalAccess.RefreshToken},
                {"grant_type","refresh_token"},
                {"scope","r_usr+w_usr+w_sub"}}, Proxy: oProxy, Header: header);
            if (result.Success == false)
            {
                if (result.Errresponse == null)
                    return (result.Errmsg, null);

                JsonTidalResponse respon = JsonHelper.ConverStringToObject<JsonTidalResponse>(result.Errresponse);
                string msg = respon.UserMessage + "! ";
                if (respon.Status != "200")
                    msg += "Refresh failed. Please log in again.";
                return (msg, null);
            }

            LoginKey key = JsonHelper.ConverStringToObject<LoginKey>(result.sData);

            TidalAccess newAccess = new TidalAccess();

            newAccess.RefreshToken = tidalAccess.RefreshToken;
            newAccess.AccessToken = JsonHelper.GetValue(result.sData, "access_token");
            newAccess.ApiToken = tidalAccess.ApiToken;
            newAccess.UniqueKey = tidalAccess.UniqueKey;
            newAccess.UserID = Convert.ToInt32(JsonHelper.GetValue(result.sData, "user", "userId"));

            DateTimeOffset expirationDate = DateTimeOffset.FromUnixTimeMilliseconds(tidalAccess.ExpirationDate);

            int expiresIn = Convert.ToInt32(JsonHelper.GetValue(result.sData, "expires_in"));
            expirationDate = expirationDate.Add(TimeSpan.FromSeconds(expiresIn));

            newAccess.ExpirationDate = expirationDate.ToUnixTimeMilliseconds();

            newAccess.IsTidalConnected = await TestTidalConnection(tidalAccess);

            return (null, newAccess);
        }
    }
}
