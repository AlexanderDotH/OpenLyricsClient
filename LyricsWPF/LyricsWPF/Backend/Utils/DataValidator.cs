using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Handler.Song;

namespace LyricsWPF.Backend.Utils
{
    class DataValidator
    {
        public static bool ValidateData(string value)
        {
            return value != null && value != string.Empty ;
        }

        public static bool ValidateData(string value, string value1)
        {
            return value != null && value1 != null && value != string.Empty && value1 != string.Empty;
        }

        public static bool ValidateData(string value, string[] value1)
        {
            return value != null && value1 != null && value != string.Empty && value1.Length > 0;
        }

        //public static bool ValidateData(object value, object value1, object value2)
        //{
        //    return value != null && value1 != null && value2 != null;
        //}

        //public static bool ValidateData(object value, object value1)
        //{
        //    return value != null && value1 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value2, object value3)
        //{
        //    return value != null && value1 != null && value2 != null && value3 != null;
        //}


        //public static bool ValidateData(object baseObject, params object[] values)
        //{
        //    if (baseObject != null)
        //    {
        //        return ValidateData(values);
        //    }

        //    return false;
        //}

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

        public static bool ValidateData(object value)
        {
            return value != null;
        }

        public static bool ValidateSong(Song value)
        {
            return value != null && value.Lyrics != null && value.CurrentLyricPart != null;
        }
    }
}
