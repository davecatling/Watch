using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchWpfClient.Model
{
    public class Message : IWatchItem
    {
        public string Id { get; private set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public string TimeStamp { get; set; }

        public Message(Dtos.MessageDto dto)
        {
            Id = dto.RowKey;
            Sender = dto.Sender;
            Text = dto.Text;
            TimeStamp = dto.TimeStamp;
        }
    }
}
