using Microsoft.WindowsAzure.Storage.Table;

namespace WatchFunctions.Model
{
    public class AccessEntity : TableEntity
    {
        public string Handle { get; set; }
        public string Grantor { get; set; }
        public AccessEntity() { }
    }
}
