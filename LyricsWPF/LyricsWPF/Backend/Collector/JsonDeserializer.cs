using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector
{
    public class JsonDeserializer<T>
    {
        private JsonSerializerSettings _serializerSettings;
        private GenericList<string> _errorList;

        public JsonDeserializer()
        {
            this._errorList = new GenericList<string>();

            this._serializerSettings = new JsonSerializerSettings();
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

        public GenericList<string> ErrorList
        {
            get => _errorList;
            set => _errorList = value;
        }
    }
}
