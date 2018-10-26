using Armbian_Monitor.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Armbian_Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDeviceStatusProvider Provider;

        public MainWindow()
        {
            InitializeComponent();

            string[] config = File.ReadAllLines("config.txt");

            Provider = new OrangePiSshProvider(config[0], config[1], config[2])
            {
                RootPassword = config[3]
            };
            Provider.Init();
            Provider.GetCurrentStatus();

            var timer = new Timer(1000);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.HasShutdownStarted)
                return;

            var state = Provider.GetCurrentStatus();

            var str = new StringBuilder();
            str.AppendLine($"CPU temp:\t{state.CpuTemp}");
            str.AppendLine($"CPU clock:\t{state.CpuClock}");
            str.AppendLine($"CPU:\t\t{state.PercCpu}%");

            str.AppendLine($"Load:\t\t{state.LoadAvgs?[0]}");
            for (int i = 1; i < (state.LoadAvgs?.Length ?? 3); i++)
                str.AppendLine($"\t\t{state.LoadAvgs?[i]}");

            str.AppendLine($"IO:\t\t{state.PercIo}%");
            str.AppendLine($"IRQ:\t\t{state.PercIrq}%");
            str.AppendLine($"NICE:\t\t{state.PercNice}%");
            str.AppendLine($"SYS:\t\t{state.PercSys}%");
            str.AppendLine($"USR:\t\t{state.PercUsr}%");
            str.AppendLine($"RAM free:\t{state.PercRamFree:0.00}%");
            str.AppendLine($"RAM avail.:\t{state.PercRamAvail:0.00}%");

            Dispatcher.Invoke(() =>
            {
                Status.Text = str.ToString();
                TrayIcon.ToolTipText = state.CpuTemp.ToString();
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
        
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            Provider.Restart();
        }
    }
}
