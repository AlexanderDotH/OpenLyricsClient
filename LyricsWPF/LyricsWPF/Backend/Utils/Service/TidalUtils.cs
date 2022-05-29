using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AIGS.Helper;
using DevBase.Enums;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.RequestData.Data;
using DevBase.Web.ResponseData;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Json;
using TidalLib;

namespace LyricsWPF.Backend.Utils.Service
{
    public class TidalUtils
    {
        public static async Task<JsonTidalAuthDevice> RegisterDevice()
        {
            string clientID = "zU4XHVVkc2tDPo4t";
            string clientSecret = "VJKhDFqJPqvsPVNBV6ukXTJmwlvbttP7wlMlrc72se4=";

            GenericList<FormKeypair> formData = new GenericList<FormKeypair>();
            formData.Add(new FormKeypair("client_id", clientID));
            formData.Add(new FormKeypair("scope", "r_usr+w_usr+w_sub"));

            RequestData requestData = new RequestData(new Uri("https://auth.tidal.com/v1/oauth2/device_authorization"),
                EnumRequestMethod.POST,
                new EnumContentType[] { EnumContentType.FORM }, 
                new EnumEncodingType[] { EnumEncodingType.UTF8 }, 
                formData);

            string authToken = Convert.ToBase64String(Encoding.Default.GetBytes(clientID + ":" + clientSecret));
            requestData.AddAuthMethod(new Auth(authToken, EnumAuthType.BASIC));

            Request request = new Request(requestData);

            ResponseData response = await request.GetResponseAsync();

            return new JsonDeserializer<JsonTidalAuthDevice>().Deserialize(response.GetContentAsString());
        }

        public static async Task<JsonTidalAccountAccess> GetTokenFrom(JsonTidalAuthDevice authDevice)
        {
            string clientID = "zU4XHVVkc2tDPo4t";

            GenericList<FormKeypair> formData = new GenericList<FormKeypair>();
            formData.Add(new FormKeypair("client_id", clientID));
            formData.Add(new FormKeypair("device_code", authDevice.DeviceCode));
            formData.Add(new FormKeypair("grant_type", "urn:ietf:params:oauth:grant-type:device_code"));
            formData.Add(new FormKeypair("scope", "r_usr+w_usr+w_sub"));

            RequestData requestData = new RequestData(new Uri("https://auth.tidal.com/v1/oauth2/token"),
                EnumRequestMethod.POST,
                new EnumContentType[] { EnumContentType.FORM },
                new EnumEncodingType[] { EnumEncodingType.UTF8 },
                formData);

            try
            {
                Request request = new Request(requestData);
                ResponseData response = await request.GetResponseAsync();

                Console.WriteLine("dawd");

                if (response.StatusCode == HttpStatusCode.NoContent)
                    return null;

                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                if (response.GetContentAsString().Contains("authorization_pending"))
                    return null;

                JsonTidalAccountAccess accountAccess =
                    new JsonDeserializer<JsonTidalAccountAccess>().Deserialize(response.GetContentAsString());

                if (accountAccess == null)
                    return null;

                return accountAccess;
            }
            catch (Exception e) { }

            return null;
        }

        public static async Task<JsonTidalAccountRefreshAccess> RefreshToken(TidalAccess tidalAccess)
        {
            string clientID = "zU4XHVVkc2tDPo4t";
            string clientSecret = "VJKhDFqJPqvsPVNBV6ukXTJmwlvbttP7wlMlrc72se4=";

            GenericList<FormKeypair> formData = new GenericList<FormKeypair>();
            formData.Add(new FormKeypair("client_id", clientID));
            formData.Add(new FormKeypair("device_code", tidalAccess.RefreshToken));
            formData.Add(new FormKeypair("grant_type", "refresh_token"));
            formData.Add(new FormKeypair("scope", "r_usr+w_usr+w_sub"));

            RequestData requestData = new RequestData(new Uri("https://auth.tidal.com/v1/oauth2/token"),
                EnumRequestMethod.POST,
                new EnumContentType[] { EnumContentType.FORM },
                new EnumEncodingType[] { EnumEncodingType.UTF8 },
                formData);

            string authToken = Convert.ToBase64String(Encoding.Default.GetBytes(clientID + ":" + clientSecret));
            requestData.AddAuthMethod(new Auth(authToken, EnumAuthType.BASIC));

            ResponseData response = await new Request(requestData).GetResponseAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            if (response.GetContentAsString().Contains("authorization_pending"))
                return null;

            JsonTidalAccountRefreshAccess accountAccess =
                new JsonDeserializer<JsonTidalAccountRefreshAccess>().Deserialize(response.GetContentAsString());

            if (accountAccess == null)
                return null;

            return accountAccess;
        }

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

                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    return p;
                }
            }

            return null;
        }

        //zU4XHVVkc2tDPo4t id
        //VJKhDFqJPqvsPVNBV6ukXTJmwlvbttP7wlMlrc72se4= secret
        
    }
}
