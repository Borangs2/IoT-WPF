using ControlSystem.MVVM.Models;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlSystem.Services
{
    internal class DeviceService
    {
        private readonly RegistryManager _registryManager;

        public DeviceService(string ioTHubConnectionString)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(ioTHubConnectionString);
        }

        public async Task<ObservableCollection<DeviceItem>> PopulateDeviceItemsAsync(string locationName, ObservableCollection<DeviceItem> _deviceItems)
        {
            
            var result = _registryManager.CreateQuery($"SELECT * FROM devices WHERE properties.reported.location = '{locationName.ToLower()}'");

            if (result.HasMoreResults)
            {
                foreach (var twin in await result.GetNextAsTwinAsync())
                {
                    var device = _deviceItems.FirstOrDefault(x => x.DeviceId == twin.DeviceId);
                    if (device == null)
                    {
                        device = new DeviceItem
                        {
                            DeviceId = twin.DeviceId,
                        };

                        try { device.DeviceTitle = twin.Properties.Reported["deviceTitle"].ToString(); }
                        catch
                        {
                            //device.DeviceTitle = twin.DeviceId;
                            device.DeviceTitle = "Name Unknown";
                        }

                        try { device.DeviceType = twin.Properties.Reported["deviceType"].ToString(); }
                        catch { device.DeviceType = "Type Unknown"; }


                        switch (device.DeviceType.ToLower())
                        {
                            case "light":
                                device.IconActive = "\uf0eb";
                                device.IconInactive = "\uf0eb";
                                device.FontActive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.FontInactive = "/Assets/Fonts/fa-regular-400.ttf#Font Awesome 6 Free Regular";
                                device.StateActive = "On";
                                device.StateInactive = "Off";
                                break;

                            case "fan":
                                device.IconActive = "\uf863";
                                device.IconInactive = "\uf863";
                                device.FontActive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.FontInactive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.StateActive = "On";
                                device.StateInactive = "Off";
                                break;

                            case "thermometer":
                                device.IconActive = "\uf2c9";
                                device.IconInactive = "\uf2cb";
                                device.FontActive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.FontInactive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.StateActive = "On";
                                device.StateInactive = "Off";
                                break;

                            default:
                                device.IconActive = "\uf2db";
                                device.IconInactive = "\uf2db";
                                device.FontActive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.FontInactive = "/Assets/Fonts/fa-Solid-900.ttf#Font Awesome 6 Free Solid";
                                device.StateActive = "On";
                                device.StateInactive = "Off";
                                break;
                        }
                        _deviceItems.Add(device);
                    }
                }
            }
            else
            {
                _deviceItems.Clear();
            }

            return _deviceItems;
        }

        public async Task<ObservableCollection<DeviceItem>> UpdateDeviceItemsAsync(ObservableCollection<DeviceItem> _deviceItems)
        {
            var removeList = new List<DeviceItem>();
            foreach (var item in _deviceItems)
            {
                var device = await _registryManager.GetDeviceAsync(item.DeviceId);
                if (device == null)
                    removeList.Add(item);
            }

            foreach (var device in removeList)
            {
                _deviceItems.Remove(device);
            }

            return _deviceItems;
        }
    }
}
