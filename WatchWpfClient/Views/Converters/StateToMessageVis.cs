using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WatchWpfClient.ViewModels;

namespace WatchWpfClient.Views.Converters
{
    public class StateToMsgVis : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Hidden;
            if (value is WatchVmState state)
            {
                return state == WatchVmState.Reading ? Visibility.Visible : Visibility.Hidden;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
