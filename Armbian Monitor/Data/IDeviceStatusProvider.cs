using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armbian_Monitor.Data
{
    public interface IDeviceStatusProvider : IDisposable
    {
        void Init();
        void Restart();
        DeviceStatus GetCurrentStatus();
    }
}
