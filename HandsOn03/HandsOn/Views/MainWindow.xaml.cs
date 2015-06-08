using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HandsOn.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.MainViewModel ViewModel = new ViewModels.MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel;
            this.ViewModel.LeapStart();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ViewModel.LeapStop();
        }
    }
}
