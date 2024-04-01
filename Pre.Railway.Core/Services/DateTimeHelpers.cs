using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public static class DateTimeHelpers
    {

        public static string GetTime(this long seconds)
        {
            TimeSpan converted = TimeSpan.FromSeconds(seconds);

            return converted.ToString(@"hh\:mm");
        }

        public static long GetUTCGap(this DateTime local)
        {
            DateTime utc = DateTime.UtcNow;
            var difference = local - utc;

            var result = (long)difference.TotalSeconds;
            return result;
        }

    }
}
