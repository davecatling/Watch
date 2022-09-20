using Microsoft.WindowsAzure.Storage.Table;

namespace WatchFunctions.Model
{
    public class MessageEntity : TableEntity
    {
        public string Sender { get; set; }
        public string To { get; set; }
        public byte[] TextBytes { get; set; }
        public MessageEntity() { }        
    }
}
