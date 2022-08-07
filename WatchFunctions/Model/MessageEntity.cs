using Microsoft.WindowsAzure.Storage.Table;

namespace WatchFunctions.Model
{
    public class MessageEntity : TableEntity
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public MessageEntity() { }        
    }
}
