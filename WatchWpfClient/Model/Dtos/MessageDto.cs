namespace WatchWpfClient.Model.Dtos
{
    public class MessageDto
    {
        public string ChannelNumber { get; set; }
        public string Sender { get; set; }
        public string To { get; set; }
        public byte[] TextBytes { get; set; }
        public string TimeStamp { get; set; }
        public string RowKey { get; set; }
    }
}
