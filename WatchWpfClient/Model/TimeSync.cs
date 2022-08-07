using System;

namespace WatchWpfClient.Model
{
    public class TimeSync : IWatchItem
    {
        public string Id { get; private set; }
        public string Text { get; private set; }
        public DateTime Time { get; private set; }
        public int Server { get; private set; }

        public TimeSync()
        {
            Id = Guid.NewGuid().ToString();
            Time = DateTime.Now;
            Server = new Random().Next(1, 12);
            Text = $"Synced with world server #{Server} @ {Time} - difference: {new Random().NextDouble()} ({Id})";            
        }
    }

}
