using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        public List<Message>? Messages { get; }

        public string? ChannelNumber
        {
            get
            { return _channelInputMode ? null : _channelNumber; }
        }

        private readonly Timer _syncTimer;
        private bool _channelInputMode;
        private string? _channelNumber;
        private FunctionProxy? _functionProxy;
        private string _handle;
        private string _password;

        public WatchApp()
        {
            _functionProxy = new FunctionProxy();
            TimeSyncs = new List<TimeSync>();
            Messages = new List<Message>();
            _syncTimer = new Timer(new Random().NextInt64(2000, 3000));
            _syncTimer.Elapsed += SyncTimer_Elapsed;
            _syncTimer.Start();
        }

        public async Task<bool> NewUser(string handle, string? email, string password)
        {
            var newUserDto = new Dtos.NewUserDto()
            {
                Handle = handle,
                Email = email,
                Password = password
            };
            var watchRsa = new WatchRsa(_functionProxy!);
            var newUserWithKeysDto = (watchRsa.GenerateKeys(newUserDto));
            _handle = handle;
            _password = password;
            return await _functionProxy!.NewUser(newUserWithKeysDto);
        }

        public async Task<bool> Login(string username, string password)
        {
            var result = await _functionProxy!.Login(username, password);
            _handle = username;
            _password = password;
            if (Messages != null)
                Messages.Clear();
            return result;
        }

        public async Task<bool> Read()
        {
            var latestMessages = await _functionProxy!.Read(_channelNumber!);
            var deletedMessages = Messages!.Where(msg => !latestMessages.Any(lm => lm.RowKey == msg.Id)).ToList();
            deletedMessages.ForEach(dm => RemoveMessage(dm));
            var addedMessages = latestMessages.Where(lm => !Messages!.Any(msg => msg.Id == lm.RowKey)).ToList();
            addedMessages.ForEach(am => AddMessage(am));
            return true;
        }

        public async Task<List<string>> ChannelHandles()
        {
            var handles = await _functionProxy!.ChannelHandles(_channelNumber!);
            return handles;
        }

        public async Task<string> Write(string message, string to)
        {
            byte[] textBytes = null;
            if (to != "ALL")
            {
                var watchRsa = new WatchRsa(_functionProxy!);
                textBytes = await watchRsa.Encrypt(message, to);
            }
            if (textBytes == null)
                textBytes = new UnicodeEncoding().GetBytes(message);
            return await _functionProxy!.Write(new Dtos.MessageDto()
            {
                ChannelNumber = _channelNumber!, 
                TextBytes = textBytes,
                To = to
            });
        }

        public async Task<string> GrantAccess(string handle)
        {
            return await _functionProxy!.GrantAccess(_channelNumber!, handle);
        }

        private void SyncTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            AddNewSync();
            _syncTimer.Interval = new Random().NextInt64(TimeSyncs!.Count < 3 ? 2000 : 10000, TimeSyncs.Count < 3 ? 10000 : 60000);
        }

        private void RemoveMessage(Message msg)
        {
            Messages!.Remove(msg);
            ItemAddedOrRemoved?.Invoke(this, new ItemAddedOrRemovedEventArgs(msg, ChangeType.Removed));
        }

        private void AddMessage(Dtos.MessageDto msgDto)
        {
            var message = new Message(msgDto);
            if (msgDto.To == _handle)
            {
                var watchRsa = new WatchRsa(_functionProxy!);
                var plainText = watchRsa.Decrypt(msgDto.TextBytes, msgDto.To);
                message.Text = plainText;
            }
            Messages!.Add(message);
            ItemAddedOrRemoved?.Invoke(this, new ItemAddedOrRemovedEventArgs(message, ChangeType.Added));
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
            if (_channelInputMode)
            {
                if (_channelNumber == null)
                    _channelNumber = channelPart;
                else
                    _channelNumber += channelPart;
            }
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
