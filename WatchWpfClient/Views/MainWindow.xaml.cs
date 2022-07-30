using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
