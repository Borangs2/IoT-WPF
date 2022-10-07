using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlSystem.MVVM.Models
{
    public class ThermometerDevice
    {
        public string DeviceName { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string DeviceLocation { get; set; } = "";
        public string DeviceOwner { get; set; } = "";
        public string Temperature { get; set; } = "";
        public string Humidity { get; set; } = "";


    }
}
