using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WatchFunctions.Dtos;

namespace WatchWpfClient.Model
{
    public class WatchApp
    {
        public delegate void ItemAddedOrRemovedEventHandler(object sender, ItemAddedOrRemovedEventArgs args);
        public event ItemAddedOrRemovedEventHandler? ItemAddedOrRemoved;
        public List<TimeSync>? TimeSyncs { get; private set; }

        public string? ChannelNumber
        {
            get
            { return _channelInputMode ? null : _channelNumber; }
        }

        private readonly Timer _syncTimer;
        private bool _channelInputMode;
        private string? _channelNumber;
        private string? _sessionToken;
        private FunctionProxy? _functionProxy;

        public WatchApp()
        {
            _functionProxy = new FunctionProxy();
            TimeSyncs = new List<TimeSync>();
            _syncTimer = new Timer(new Random().NextInt64(2000, 3000));
            _syncTimer.Elapsed += SyncTimer_Elapsed;
            _syncTimer.Start();
        }

        public async Task<bool> NewUser(string handle, string? email, string passWord)
        {
            var dto = new NewUserDto()
            {
                Handle = handle,
                Email = email,
                Password = passWord
            };
            return await _functionProxy!.NewUser(dto);
        }

        public async Task<bool> Login(string username, string password)
        {
            var session = await _functionProxy!.Login(username, password);
            if (session != null && session != String.Empty)
            {
                _sessionToken = session;
                return true;
            }
            return false;
        }

        private void SyncTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            AddNewSync();
            _syncTimer.Interval = new Random().NextInt64(TimeSyncs!.Count < 3 ? 2000 : 10000, TimeSyncs.Count < 3 ? 10000 : 60000);
        }

        private void AddNewSync()
        {
            TimeSync newTimeSync = new TimeSync();
            TimeSyncs!.Add(newTimeSync);
            ItemAddedOrRemoved?.Invoke(this, new ItemAddedOrRemovedEventArgs(newTimeSync, ChangeType.Added));
            CullTimeSyncs();
        }

        private void CullTimeSyncs()
        {
            while (TimeSyncs!.Count > 20)
            {
                var oldestSync = TimeSyncs.MinBy(ts => ts.Time);
                if (oldestSync != null)
                {
                    TimeSyncs.Remove(oldestSync);
                    ItemAddedOrRemoved?.Invoke(this, new ItemAddedOrRemovedEventArgs(oldestSync, ChangeType.Removed));
                }
            }
        }

        public bool ToggleChannelInput()
        {
            _channelInputMode = !_channelInputMode;
            if (_channelInputMode)
                _channelNumber = null;
            return _channelInputMode;
        }

        public void AddChannelPart(string channelPart)
        {
            if (!_channelInputMode)
                throw new InvalidOperationException("Channel input inactive");
            if (_channelNumber == null)
                _channelNumber = channelPart;
            else
                _channelNumber += channelPart;
        }

        public class ItemAddedOrRemovedEventArgs : EventArgs
        {
            public IWatchItem Item { get; private set; }
            public ChangeType ChangeType { get; private set; }
            public ItemAddedOrRemovedEventArgs(IWatchItem watchItem, ChangeType changeType)
            {
                Item = watchItem;
                ChangeType = changeType;
            }
        }

        public enum ChangeType
        {
            Added,
            Removed
        }
    }
   
}
