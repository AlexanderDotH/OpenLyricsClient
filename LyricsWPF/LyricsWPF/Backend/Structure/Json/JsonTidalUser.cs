using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure.Json
{
    public class JsonTidalUser
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("fullName")]
        public object FullName { get; set; }

        [JsonProperty("firstName")]
        public object FirstName { get; set; }

        [JsonProperty("lastName")]
        public object LastName { get; set; }

        [JsonProperty("nickname")]
        public object Nickname { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("city")]
        public object City { get; set; }

        [JsonProperty("postalcode")]
        public object Postalcode { get; set; }

        [JsonProperty("usState")]
        public object UsState { get; set; }

        [JsonProperty("phoneNumber")]
        public object PhoneNumber { get; set; }

        [JsonProperty("birthday")]
        public object Birthday { get; set; }

        [JsonProperty("gender")]
        public object Gender { get; set; }

        [JsonProperty("imageId")]
        public object ImageId { get; set; }

        [JsonProperty("channelId")]
        public int ChannelId { get; set; }

        [JsonProperty("parentId")]
        public int ParentId { get; set; }

        [JsonProperty("acceptedEULA")]
        public bool AcceptedEULA { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("facebookUid")]
        public int FacebookUid { get; set; }

        [JsonProperty("appleUid")]
        public object AppleUid { get; set; }

        [JsonProperty("googleUid")]
        public object GoogleUid { get; set; }

        [JsonProperty("newUser")]
        public bool NewUser { get; set; }
    }
}
