namespace OpenLyricsClient.Backend.Utils
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

        public static bool ValidateData(object value)
        {
            return value != null;
        }

        //public static bool ValidateData(object value, object value1)
        //{
        //    return value != null && value1 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value2)
        //{
        //    return value != null && value1 != null && value2 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value3, object value4)
        //{
        //    return value != null && value1 != null && value3 != null && value4 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value3, object value4, object value5)
        //{
        //    return value != null && value1 != null && value3 != null && value4 != null && value5 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value3, object value4, object value5, object value6)
        //{
        //    return value != null && value1 != null && value3 != null && value4 != null && value5 != null && value6 != null;
        //}

        //public static bool ValidateData(object value, object value1, object value3, object value4, object value5, object value6, object value7)
        //{
        //    return value != null && value1 != null && value3 != null && value4 != null && value5 != null && value6 != null && value7 != null;
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

    }
}
