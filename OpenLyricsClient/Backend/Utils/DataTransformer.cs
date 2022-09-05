namespace OpenLyricsClient.Backend.Utils
{
    class DataTransformer
    {
        public static string CapitalizeFirstLetter(string input)
        {
            if (input != null)
                return input;

            if (input.Length > 1)
                return input;

            return input[0].ToString().ToUpper() + input.Substring(1);
        }
    }
}
