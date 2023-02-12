using DevBase.Generics;
using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Utils
{
    public class JsonDeserializer<T>
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

        public T Deserialize(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, this._serializerSettings);
        }

        public AList<string> ErrorList
        {
            get => _errorList;
            set => _errorList = value;
        }
    }
}
