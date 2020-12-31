using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public static class Constants
    {
        public const string GrainStorage = "PgStore";
        public const string ClusterId = "dev";
        public const string ServiceId = "Conduitorleans";
        //public const string ConnStr = "Host=localhost;Port=5432;Database=conduitorleans;Username=postgres;Password=mainmain;Application Name=conduitorleans";
        public const string ConnStr = "Host=172.17.28.225;Port=5432;Database=conduitorleans;Username=postgres;Password=mainmain;Application Name=conduitorleans";
    }
}
