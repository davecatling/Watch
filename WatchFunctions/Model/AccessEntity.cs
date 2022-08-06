using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchFunctions.Model
{
    public class AccessEntity : TableEntity
    {
        public string Handle { get; set; }
        public string Grantor { get; set; }
        public AccessEntity() { }
    }
}
