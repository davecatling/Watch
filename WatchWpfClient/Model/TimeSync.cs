using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchWpfClient.Model
{
    public class TimeSync : IWatchItem
    {
        public string Id { get; private set; }
        public string Text { get; private set; }
        public DateTime Time { get; private set; }
        public int Server { get; private set; }
        private readonly int _illegalServer = 0;

        public TimeSync()
        {
            Id = Guid.NewGuid().ToString();
            Time = DateTime.Now;
            while (Server == _illegalServer)
                Server = new Random().Next(1, 12);
            Text = $"Synced with world server #{Server} @ {Time} - difference: {new Random().NextDouble()} ({Id})";            
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TimeSync(TimeSync prevTimeSync) : this()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _illegalServer = prevTimeSync.Server;
        }
    }

}
