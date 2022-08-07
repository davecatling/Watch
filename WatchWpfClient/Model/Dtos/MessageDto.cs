namespace WatchWpfClient.Model.Dtos
{
    public class MessageDto
    {
        public string Sender { get; set; } 
        public string Text { get; set; }
        public string TimeStamp { get; set; }
        public string RowKey { get; set; }
    }
}
