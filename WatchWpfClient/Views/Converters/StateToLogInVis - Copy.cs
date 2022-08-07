using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WatchWpfClient.ViewModels;

namespace WatchWpfClient.Views.Converters
{
    public class StateToBackButtonVis : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Hidden;
            if (value is WatchVmState state)
            {
                return state == WatchVmState.Normal ? Visibility.Hidden : Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
