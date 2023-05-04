using DevBase.Generics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenLyricsClient.Shared.Utils
{
    public class JsonDeserializer
    {
        private JsonSerializerSettings _serializerSettings;
        private AList<string> _errorList;

        public JsonDeserializer()
        {
            this._errorList = new AList<string>();

            this._serializerSettings = new JsonSerializerSettings();

            this._serializerSettings.NullValueHandling = NullValueHandling.Ignore;

            this._serializerSettings.Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                this._errorList.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, this._serializerSettings);
        }

        public JObject Serialize(object structure)
        {
            string serialized = JsonConvert.SerializeObject(structure, this._serializerSettings);
            return JObject.Parse(serialized);
        }
        
        public JObject ToJObject<T>(T data) => JObject.FromObject(data);
        
        public AList<string> ErrorList
        {
            get => _errorList;
            set => _errorList = value;
        }
    }
}
