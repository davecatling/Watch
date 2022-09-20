using System.Text;

namespace WatchWpfClient.Model
{
    public class Message : IWatchItem
    {
        public string Id { get; private set; }
        public string Sender { get; set; }
        public string To {  get; set; }
        public string Text { get; set; }
        public string TimeStamp { get; set; }

        public Message(Dtos.MessageDto dto)
        {
            Id = dto.RowKey;
            Sender = dto.Sender;
            To = dto.To;
            Text = new UnicodeEncoding().GetString(dto.TextBytes);
            TimeStamp = dto.TimeStamp;
        }
    }
}
