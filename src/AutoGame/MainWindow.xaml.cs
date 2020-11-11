using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
        }

        private MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Click += this.NotifyIcon_Click;
            this.notifyIcon.Icon = new Icon(@"Icons\AutoGame.ico");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Loaded -= this.MainWindow_Loaded;
            this.notifyIcon.Click -= this.NotifyIcon_Click;
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
            this.ViewModel?.Dispose();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon.Visible = true;
            }

            base.OnStateChanged(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.MainWindow_Loaded;
            this.WindowState = WindowState.Minimized;
            this.ViewModel?.LoadedCommand?.Execute(null);
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.notifyIcon.Visible = false;
            this.WindowState = WindowState.Normal;
        }
    }
}
