using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armbian_Monitor.Data
{
    public struct DeviceStatus
    {
        public DateTime TimeTaken { get; }

        public ValueAndUnit CpuClock { get; set; }
        public ValueAndUnit CpuTemp { get; set; }
        public float[] LoadAvgs { get; set; }

        public float PercCpu { get; set; }
        public float PercSys { get; }
        public float PercUsr { get; }
        public float PercNice { get; }
        public float PercIo { get; }
        public float PercIrq { get; }

        public float PercRamFree { get; set; }
        public float PercRamAvail { get; set; }

        public DeviceStatus(DateTime timeTaken, ValueAndUnit cpuClock, ValueAndUnit cpuTemp, float percCpu,
            float percSys, float percUsr, float percNice, float percIo, float percIrq) : this()
        {
            this.TimeTaken = timeTaken;
            this.CpuClock = cpuClock;
            this.CpuTemp = cpuTemp;

            this.PercCpu = percCpu;
            this.PercSys = percSys;
            this.PercUsr = percUsr;
            this.PercNice = percNice;
            this.PercIo = percIo;
            this.PercIrq = percIrq;

            this.PercRamFree = -1;
        }
    }
}
