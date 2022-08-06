﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        private string _grantAccessHandle;
        private string _status;
        private Timer _readTimer;
        private ICommand? _toggleChannelInputCommand;
        private ICommand? _addChannelPartCommand;
        private ICommand? _newUserCommand;
        private ICommand? _showNewUserCommand;
        private ICommand? _loginCommand;
        private ICommand? _writeCommand;
        private ICommand? _grantAccessCommand;
        private ICommand? _backCommand;
        private ICommand? _quitCommand;

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

        public string GrantAccessHandle
        {
            get => _grantAccessHandle;
            set
            {
                _grantAccessHandle = value;
                OnPropertyChanged(nameof(_grantAccessHandle));
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
                if (_readTimer != null)
                {
                    if (_state == WatchVmState.Reading)
                        _readTimer.Start();
                    else
                        _readTimer.Stop();
                }
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
            _newUserHandle = string.Empty;
            _newUserPassword = string.Empty;
            _newUserEmail = string.Empty;
            _grantAccessHandle = string.Empty;
            _readTimer = new Timer()
            {
                Interval = 15000
            };
            _readTimer.Elapsed += ReadTimer_Elapsed;
            TimeSyncs = new ObservableCollection<TimeSync>();
            Messages = new ObservableCollection<Message>();
            BindingOperations.EnableCollectionSynchronization(TimeSyncs, _timeSyncLock);
            BindingOperations.EnableCollectionSynchronization(Messages, _messageLock);
        }

        private void ReadTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Read();
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

        public ICommand ShowNewUserCommand
        {
            get
            {
                if (_showNewUserCommand == null)
                    _showNewUserCommand = new RelayCommand((exec) => ShowNewUser());
                return _showNewUserCommand;
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

        public ICommand GrantAccessCommand
        {
            get
            {
                if (_grantAccessCommand == null)
                    _grantAccessCommand = new RelayCommand((exec) => GrantAccess(), (canExec) => GrantAccessOK());
                return _grantAccessCommand;
            }
        }

        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                    _backCommand = new RelayCommand((exec) => Back());
                return _backCommand;
            }
        }

        public ICommand QuitCommand
        {
            get
            {
                if (_quitCommand == null)
                    _quitCommand = new RelayCommand((exec) => Quit());
                return _quitCommand;
            }
        }

        private bool NewUserOK()
        {
            if (_newUserHandle.Length >= 8 && _newUserPassword.Length >= 8)
                return true;
            return false;
        }

        private async void NewUser()
        {
            Status = "Please wait...";
            var result = await _watchApp.NewUser(_newUserHandle, _newUserEmail, _newUserPassword);
            if (result)
            {
                MessageBox.Show($"New user {_newUserHandle} created");
                State = WatchVmState.LogIn;
            }
            Status = String.Empty;
        }

        private void ShowNewUser()
        {
            State = WatchVmState.NewUser;
        }

        private async void Login()
        {
            Status = "Please wait...";
            var result = await _watchApp.Login(_loginHandle, _loginPassword);
            LoginHandle = String.Empty;
            LoginPassword = String.Empty;
            if (result)
            {
                State = WatchVmState.Reading;
                Read();
            }
            else
                Status = "Login failed";
        }

        private async void Read()
        {
            try
            {
                Status = "Please wait...";
                await _watchApp.Read();
                Status = String.Empty;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                State = WatchVmState.LogIn;
            }
        }
                
        private bool WriteOK()
        {
            return _newMessage.Length > 0;
        }

        private bool GrantAccessOK()
        {
            return _grantAccessHandle.Length > 0;
        }

        private async void Write()
        {
            Status = "Please wait...";
            var result = await _watchApp.Write(NewMessage);
            if (result == "OK")
            {
                NewMessage = string.Empty;
                Status = string.Empty;
                Read();
            }
            else
                Status = result;            
        }

        private async void GrantAccess()
        {
            Status = "Please wait...";
            var result = await _watchApp.GrantAccess(GrantAccessHandle);
            if (result == "OK")
            {
                GrantAccessHandle = string.Empty;
                Status = string.Empty;
                Read();
            }
            else
                Status = result;
        }

        public void Quit()
        {
            Application.Current.Shutdown();
        }

        private void Back()
        {
            Status = String.Empty;
            State = WatchVmState.Normal;
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
        Reading,
        NewUser
    }
}
