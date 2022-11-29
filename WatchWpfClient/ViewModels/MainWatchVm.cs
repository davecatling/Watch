using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
        private string _resetPassword;
        private string _resetPasswordValidation;
        private string _loginHandle;
        private string _loginPassword;
        private string _newMessage;
        private string _newMessageTo;
        private string _grantAccessHandle;
        private string _status;
        private Timer _readTimer;
        private ICommand? _toggleChannelInputCommand;
        private ICommand? _addChannelPartCommand;
        private ICommand? _newUserCommand;
        private ICommand? _showNewUserCommand;
        private ICommand? _loginCommand;
        private ICommand? _showResetPasswordCommand;
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

        public string ResetPassword
        {
            get => _resetPassword;
            set
            {
                _resetPassword = value;
                OnPropertyChanged(nameof(ResetPassword));
            }
        }

        public string ResetPasswordValidation
        {
            get => _resetPasswordValidation;
            set
            {
                _resetPasswordValidation = value;
                OnPropertyChanged(nameof(ResetPasswordValidation));
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

        public string NewMessageTo
        {
            get => _newMessageTo ?? "ALL";
            set
            {
                _newMessageTo = value;
                OnPropertyChanged(nameof(NewMessageTo));
            }
        }

        public string GrantAccessHandle
        {
            get => _grantAccessHandle;
            set
            {
                _grantAccessHandle = value;
                OnPropertyChanged(nameof(GrantAccessHandle));
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
                ClearValues();
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
            _timeSyncLock = new object();
            _messageLock = new object();            
            _readTimer = new Timer()
            {
                Interval = 15000
            };
            _readTimer.Elapsed += ReadTimer_Elapsed;
            TimeSyncs = new ObservableCollection<TimeSync>();
            Messages = new ObservableCollection<Message>();
            State = WatchVmState.Normal;
            BindingOperations.EnableCollectionSynchronization(TimeSyncs, _timeSyncLock);
            BindingOperations.EnableCollectionSynchronization(Messages, _messageLock);
        }

        private void ClearValues()
        {
            NewUserHandle = String.Empty;
            NewUserPassword = String.Empty;
            LoginHandle = String.Empty;
            LoginPassword = String.Empty;
            NewMessage = String.Empty;
            GrantAccessHandle = String.Empty;
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

        public ICommand ShowResetPasswordCommand
        {
            get
            {
                if (_showResetPasswordCommand == null)
                    _showResetPasswordCommand = new RelayCommand((exec) => ShowResetPassword());
                return _showResetPasswordCommand;
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
            try
            {                
                var result = await _watchApp.NewUser(_newUserHandle, _newUserPassword);
                if (result)
                {
                    Status = $"New user {_newUserHandle} created";
                    State = WatchVmState.LogIn;
                }
                else
                    Status = "New user creation failed";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private void ShowNewUser()
        {
            State = WatchVmState.NewUser;
        }

        private void ShowResetPassword()
        {
            State = WatchVmState.PasswordReset;
        }

        private async void Login()
        {
            Status = "Please wait...";
            try
            {
                var result = await _watchApp.Login(_loginHandle, _loginPassword);
                if (result)
                {
                    if (Messages != null)
                        Messages.Clear();
                    var readSuccess = await Read();
                    if (readSuccess)
                        State = WatchVmState.Reading;
                }
                else
                    Status = "Login failed";
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private async Task<bool> Read()
        {
            Status = "Please wait...";
            try
            {                
                var result = await _watchApp.Read();
                if (result)
                    Status = String.Empty;
                return result;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                State = WatchVmState.LogIn;
                return false;
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
            try
            {
                var result = await _watchApp.Write(NewMessage, NewMessageTo);
                if (result == "OK")
                {
                    NewMessage = string.Empty;
                    Status = string.Empty;
                    await Read();
                }
                else
                    Status = result;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private async void GrantAccess()
        {
            Status = "Please wait...";
            try
            {               
                var result = await _watchApp.GrantAccess(GrantAccessHandle);
                if (result == "OK")
                {
                    GrantAccessHandle = string.Empty;
                    Status = string.Empty;
                    await Read();
                }
                else
                    Status = result;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
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

        private bool ResetPasswordOK()
        {
            if (_loginHandle?.Length >= 4 && _resetPassword?.Length >= 8
                && (_resetPassword == _resetPasswordValidation))
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
        NewUser,
        PasswordReset
    }
}
