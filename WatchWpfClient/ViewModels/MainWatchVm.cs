using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using WatchWpfClient.Model;

namespace WatchWpfClient.ViewModels
{
    public class MainWatchVm : INotifyPropertyChanged
    {
        private readonly Clock _clock;
        private readonly WatchApp _watchApp;
        private WatchVmState _state;
        private object _timeSyncLock;
        private ObservableCollection<TimeSync>? _timeSyncs;
        private object _messageLock;
        private ObservableCollection<Message>? _messages;
        private string _newUserHandle;
        private string _newUserPassword;
        private string _newUserEmail;
        private string _loginHandle;
        private string _loginPassword;
        private string _newMessage;
        private string _status;
        private ICommand? _toggleChannelInputCommand;
        private ICommand? _addChannelPartCommand;
        private ICommand? _newUserCommand;
        private ICommand? _loginCommand;
        private ICommand? _writeCommand;

        public ObservableCollection<TimeSync>? TimeSyncs
        {
            get => _timeSyncs;
            private set
            {
                _timeSyncs = value;
                OnPropertyChanged(nameof(TimeSyncs));   
            }
        }

        public ObservableCollection<Message>? Messages
        {
            get => _messages;
            private set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string NewUserHandle
        {
            get => _newUserHandle;
            set
            {
                _newUserHandle = value;
                OnPropertyChanged(nameof(NewUserHandle));
            }
        }

        public string NewUserPassword
        {
            get => _newUserPassword;
            set
            {
                _newUserPassword = value;
                OnPropertyChanged(nameof(NewUserPassword));
            }
        }

        public string NewUserEmail
        {
            get => _newUserEmail;
            set
            {
                _newUserEmail = value;
                OnPropertyChanged(nameof(NewUserEmail));
            }
        }

        public string LoginHandle
        {
            get => _loginHandle;
            set
            {
                _loginHandle = value;
                OnPropertyChanged(nameof(LoginHandle));
            }
        }

        public string LoginPassword
        {
            get => _loginPassword;
            set
            {
                _loginPassword = value;
                OnPropertyChanged(nameof(LoginPassword));
            }
        }

        public string NewMessage
        {
            get => _newMessage;
            set
            {
                _newMessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public Clock? Clock { get => _clock; }
        public WatchVmState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public MainWatchVm()
        {
            _clock = new Clock();
            _watchApp = new WatchApp();
            _watchApp.ItemAddedOrRemoved += WatchApp_ItemAddedOrRemoved;
            State = WatchVmState.Normal;
            _timeSyncLock = new object();
            _messageLock = new object();
            _newMessage = string.Empty;
            TimeSyncs = new ObservableCollection<TimeSync>();
            Messages = new ObservableCollection<Message>();
            BindingOperations.EnableCollectionSynchronization(TimeSyncs, _timeSyncLock);
            BindingOperations.EnableCollectionSynchronization(Messages, _messageLock);
        }

        public ICommand ToggleChannelInputCommand
        {
            get
            {
                if (_toggleChannelInputCommand == null)
                    _toggleChannelInputCommand = new RelayCommand((exec) => ToggleChannelInput());
                return _toggleChannelInputCommand;
            }
        }

        public ICommand AddChannelPartCommand
        {
            get
            {
                if (_addChannelPartCommand == null)
                    _addChannelPartCommand = new RelayCommand(AddChannelPart);
                return _addChannelPartCommand;
            }
        }

        public ICommand NewUserCommand
        {
            get
            {
                if (_newUserCommand == null)
                    _newUserCommand = new RelayCommand((exec) => NewUser(), (canExec) =>NewUserOK());
                return _newUserCommand;
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                    _loginCommand = new RelayCommand((exec) => Login(), (canExec) => LoginOK());
                return _loginCommand;
            }
        }

        public ICommand WriteCommand
        {
            get
            {
                if (_writeCommand == null)
                    _writeCommand = new RelayCommand((exec) => Write(), (canExec) => WriteOK());
                return _writeCommand;
            }
        }

        private bool NewUserOK()
        {
            if (_newUserHandle.Length >= 8 && _newUserPassword.Length >= 12)
                return true;
            return false;
        }

        private async void NewUser()
        {
            var result = await _watchApp.NewUser(_newUserHandle, _newUserEmail, _newUserPassword);
            if (result)
                MessageBox.Show($"New user {_newUserHandle} created");
        }

        private async void Login()
        {
            Status = "Please wait";
            var result = await _watchApp.Login(_loginHandle, _loginPassword);
            LoginHandle = String.Empty;
            LoginPassword = String.Empty;
            if (result)
            {
                Status = String.Empty;
                State = WatchVmState.Reading;
                _watchApp.Read();
            }
            else
                Status = "Login failed";
        }
                
        private bool WriteOK()
        {
            return _newMessage.Length > 0;
        }

        private async void Write()
        {
            Status = "Please wait";
            var result = await _watchApp.Write(NewMessage);
            if (result == "OK")
            {
                NewMessage = string.Empty;
                Status = string.Empty;
                _watchApp.Read();
            }
            else
                Status = result;            
        }

        private bool LoginOK()
        {
            if (_loginHandle?.Length >= 4 && _loginPassword?.Length >= 8)
                return true;
            return false;
        }

        private void ToggleChannelInput()
        {
            if (_watchApp.ToggleChannelInput() == false)
                State = WatchVmState.LogIn;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddChannelPart(object channelPart)
        {
            if (channelPart is string partString)
                _watchApp.AddChannelPart(partString);
            else
                throw new ArgumentException(nameof(AddChannelPart));
        }

        private void WatchApp_ItemAddedOrRemoved(object sender, WatchApp.ItemAddedOrRemovedEventArgs args)
        {
            if (args.Item is TimeSync timeSync)
                UpdateTimeSyncs(timeSync, args.ChangeType);
            if (args.Item is Message message)
                UpdateMessages(message, args.ChangeType);
        }

        private void UpdateTimeSyncs(TimeSync timeSync, WatchApp.ChangeType changeType)
        {
            lock (_timeSyncLock)
            {
                if (changeType == WatchApp.ChangeType.Added)
                    TimeSyncs!.Add(timeSync);
                else
                    TimeSyncs!.Remove(timeSync);
            }            
        }

        private void UpdateMessages(Message message, WatchApp.ChangeType changeType)
        {
            lock (_messageLock)
            {
                if (changeType == WatchApp.ChangeType.Added)
                    Messages!.Add(message);
                else
                    Messages!.Remove(message);
            }
        }
    }

    public enum WatchVmState
    {
        Normal,
        LogIn,
        Reading
    }
}
