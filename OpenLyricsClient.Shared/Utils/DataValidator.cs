using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Shared.Utils
{
    public class DataValidator
    {
        public static bool ValidateData(string value)
        {
            return value != null && value != string.Empty ;
        }

        public static bool ValidateData(object value)
        {
            return value != null;
        }

        public static bool ValidateData(params object[] values)
        {
            if (values == null)
                return false;

            if (values.Length == 0)
                return false;

            if (values[0] == null)
                return false;

            for (int i = 0; i < values.Length; i++)
                if (values[i] == null)
                    return false;

            return true;
        }
    }
}
