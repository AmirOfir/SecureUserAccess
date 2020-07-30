using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurePassword
{
    class DALHelper
    {
        public static DateTime ParseTime(string currentDate, string currentTime)
        {
            return DateTime.Parse(currentDate + " " + currentTime);
        }
    }
}
