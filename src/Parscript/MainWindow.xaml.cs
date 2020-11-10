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

namespace Parscript
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetWindowState(WindowState.Minimized);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Click += this.notifyIcon_Click;
            this.notifyIcon.Icon = new System.Drawing.Icon(@"Icons\Parscript.ico");

            this.StateChanged += this.OnStateChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Loaded -= this.MainWindow_Loaded;
            this.notifyIcon.Click -= this.notifyIcon_Click;
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
            ((MainWindowViewModel)this.DataContext).Dispose();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.SetWindowState(WindowState.Normal);
        }

        private void SetWindowState(WindowState state)
        {
            this.WindowState = state;
            this.OnStateChanged(this, EventArgs.Empty);
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.notifyIcon.Visible = true;
            }
            else
            {
                this.notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }
    }
}
