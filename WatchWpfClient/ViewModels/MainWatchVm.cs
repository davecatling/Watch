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
        private ICommand? _toggleChannelInputCommand;
        private ICommand? _addChannelPartCommand;

        public ObservableCollection<TimeSync>? TimeSyncs
        {
            get => _timeSyncs;
            private set
            {
                _timeSyncs = value;
                OnPropertyChanged(nameof(TimeSyncs));   
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

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
            TimeSyncs = new ObservableCollection<TimeSync>();
            BindingOperations.EnableCollectionSynchronization(TimeSyncs, _timeSyncLock);            
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
    }

    public enum WatchVmState
    {
        Normal,
        LogIn
    }
}
