using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchFunctions.Model
{
    public class MessageEntity : TableEntity
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public MessageEntity() { }
    }
}
