using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Armbian_Monitor.Data
{
    public class OrangePiSshProvider : IDeviceStatusProvider
    {
        private static readonly Regex UptimeRegex = new Regex(@"\d+\.\d+");

        private readonly string IP, User, Password;
        private SshClient Client;

        public string RootPassword { get; set; }

        private DeviceStatus LastData;
        private TaskCompletionSource<bool> ReadComplete;

        public OrangePiSshProvider(string ip, string user, string pwd)
        {
            this.IP = ip;
            this.User = user;
            this.Password = pwd;
        }

        public void Init()
        {
            Client = new SshClient(IP, User, Password);
            Client.HostKeyReceived += (sender, e) => e.CanTrust = true;

            try
            {
                Client.Connect();
            }
            catch
            {
                Client = null;
                return;
            }

            string cmdStr = "armbianmonitor -m";

            if (RootPassword != null)
                cmdStr = $"echo {RootPassword} | sudo -S {cmdStr}";

            var shell = Client.CreateShellStream("bash", 80, 50, 1024, 1024, 1024);
            shell.WriteLine(cmdStr + "\n");
            shell.Flush();

            new Thread(() =>
            {
                while (ReadComplete == null)
                {
                    string line = shell.ReadLine(TimeSpan.FromSeconds(1));

                    if (string.IsNullOrEmpty(line) || ReadComplete != null)
                        continue;

                    Console.WriteLine(line);

                    string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length != 11)
                        continue;

                    try
                    {
                        LastData = new DeviceStatus(
                            timeTaken: DateTime.Now,
                            cpuClock: new ValueAndUnit(split[1]),//
                            percCpu: float.Parse(split[3].Substring(0, split[3].Length - 1)),
                            percSys: float.Parse(split[4].Substring(0, split[4].Length - 1)),
                            percUsr: float.Parse(split[5].Substring(0, split[5].Length - 1)),
                            percNice: float.Parse(split[6].Substring(0, split[6].Length - 1)),
                            percIo: float.Parse(split[7].Substring(0, split[7].Length - 1)),
                            percIrq: float.Parse(split[8].Substring(0, split[8].Length - 1)),
                            cpuTemp: new ValueAndUnit(split[9]));//
                    }
                    catch (FormatException)
                    {
                    }
                }

                ReadComplete.SetResult(true);
            })
            {
                IsBackground = true
            }.Start();
        }
        
        public DeviceStatus GetCurrentStatus()
        {
            if (Client?.IsConnected == false)
                return default;

            string[] ram = Client.RunCommand("egrep 'Mem' /proc/meminfo").Result.Split('\n');
            LastData.PercRamFree = GetLine(ram[1]).Value / GetLine(ram[0]).Value * 100;
            LastData.PercRamAvail = GetLine(ram[2]).Value / GetLine(ram[0]).Value * 100;

            string cpuTemp = Client.RunCommand("cat /sys/class/thermal/thermal_zone0/temp").Result;
            LastData.CpuTemp = new ValueAndUnit($"{int.Parse(cpuTemp) / 1000f:0.00}ºC");

            string uptime = Client.RunCommand("uptime").Result;
            LastData.LoadAvgs = UptimeRegex.Matches(uptime)
                .Cast<Match>()
                .Select(o => float.Parse(o.Value, CultureInfo.InvariantCulture))
                .ToArray();

            string clock = Client.RunCommand("cat /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq").Result;
            LastData.CpuClock = new ValueAndUnit($"{int.Parse(clock) / 1000}Mhz");

            //string mpstat = Client.RunCommand("mpstat").Result;
            //string cpuPerc = MpStatCpuPercRegex.Matches(mpstat).Cast<Match>().Last().Value;
            //LastData.PercCpu = 100 - float.Parse(cpuPerc, CultureInfo.InvariantCulture);

            return LastData;

            ValueAndUnit GetLine(string line) => new ValueAndUnit(line.Substring(line.IndexOf(":") + 1).Trim());
        }

        public void Dispose()
        {
            Client?.Dispose();
        }

        public void Restart()
        {
            Task.Run(async () =>
            {
                ReadComplete = new TaskCompletionSource<bool>();
                await ReadComplete.Task;

                ReadComplete = null;

                Client?.Disconnect();

                Init();
            });
        }
    }
}
