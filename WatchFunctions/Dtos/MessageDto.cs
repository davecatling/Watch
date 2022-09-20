namespace WatchFunctions.Dtos
{
    public class MessageDto
    {
        public string To { get; set; }
        public string ChannelNumber { get; set; }
        public byte[] TextBytes { get; set; }
    }
}
