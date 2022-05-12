using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Utils
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
