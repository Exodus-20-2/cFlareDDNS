using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace cFlareDDNS.Data
{
    internal class Globals
    {
        public static string globalIpAddress = new WebClient().DownloadString("https://api.ipify.org");

        public static int updateInterval = 1800 * 1000;

        public static List<string> domainRecords = new List<string>();

    }
}
