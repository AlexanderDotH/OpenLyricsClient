using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Utils
{
    class DateConverter
    {
        private DateTime _startDate;

        public DateConverter(DateTime startDate)
        {
            this._startDate = startDate;
        }

        public long ConvertToLongMS(DateTime dateTime)
        {
            return (long)dateTime.Subtract(this._startDate).TotalMilliseconds;
        }
    }
}
