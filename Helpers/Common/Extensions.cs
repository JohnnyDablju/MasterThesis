using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    static class Extensions
    {
        public static DateTime TimestampToDateTime(this long timestamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddMilliseconds(Convert.ToInt64(timestamp));
        }
    }
}
