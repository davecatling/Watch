using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchWpfClient.Model;

namespace WatchWpfClient.ViewModels
{
    public class MainWatchVm : INotifyPropertyChanged
    {
        private readonly Clock _clock;
        private readonly WatchApp _watchApp;

        public ObservableCollection<TimeSync> TimeSyncs { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public Clock? Clock { get => _clock; }

        public MainWatchVm()
        {
            _clock = new Clock();
            TimeSyncs = new ObservableCollection<TimeSync>();
            _watchApp = new WatchApp();
            _watchApp.ItemAddedOrRemoved += WatchApp_ItemAddedOrRemoved;
        }

        private void WatchApp_ItemAddedOrRemoved(object sender, WatchApp.ItemAddedOrRemovedEventArgs args)
        {
            if (args.Item is TimeSync timeSync)
                UpdateTimeSyncs(timeSync, args.ChangeType);
        }

        private void UpdateTimeSyncs(TimeSync timeSync, WatchApp.ChangeType changeType)
        {
            if (changeType == WatchApp.ChangeType.Added)
                TimeSyncs.Add(timeSync);
            else
                TimeSyncs.Remove(timeSync);
        }
    }
}
