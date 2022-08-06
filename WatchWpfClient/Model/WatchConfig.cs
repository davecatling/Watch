using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchWpfClient.Model
{
    internal class WatchConfig
    {
        public string URL { get; set; }
        public string LoginCode { get; set; }
        public string NewUserCode { get; set; }
        public string ReadCode { get; set; }
        public string WriteCode { get; set; }
    }
}
