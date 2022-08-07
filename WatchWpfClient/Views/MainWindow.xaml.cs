using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace WatchWpfClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            var resource = this.FindResource("SecsAfterFinishSb");
            var newSB = (Storyboard)resource;
            newSB.Begin();                
        }

        private void DoubleAnimation_Completed_1(object sender, EventArgs e)
        {
            var resource = this.FindResource("MinsAfterFinishSb");
            var newSB = (Storyboard)resource;
            newSB.Begin();
        }

        private void DoubleAnimation_Completed_2(object sender, EventArgs e)
        {
            var resource = this.FindResource("HoursAfterFinishSb");
            var newSB = (Storyboard)resource;
            newSB.Begin();
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked");
        }
    }
}
