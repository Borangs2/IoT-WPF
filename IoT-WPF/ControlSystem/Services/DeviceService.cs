using ControlSystem.MVVM.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace ControlSystem.Services
{
    internal class DeviceService
    {
        private readonly RegistryManager _registryManager;
        private readonly string _dbConnectionString = "Server=tcp:iot-wpf.database.windows.net,1433;Initial Catalog=IoT-Devices;Persist Security Info=False;User ID=borangs;Password=HansIsBest1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public DeviceService(string ioTHubConnectionString)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(ioTHubConnectionString);
        }

        /// <summary>
        /// Populates <paramref name="_deviceItems"/> with all devices from <paramref name="locationName"/>
        /// </summary>
        /// <param name="locationName"></param>
        /// <param name="_deviceItems"></param>
        /// <returns>
        /// A list of type <see cref="ObservableCollection{DeviceItem}"/> with all devices at <paramref name="locationName"/>
        /// </returns>
        public async Task<ObservableCollection<DeviceItem>> PopulateDeviceItemsAsync(string locationName, ObservableCollection<DeviceItem> _deviceItems)
        {

            var result = _registryManager.CreateQuery($"SELECT * FROM devices WHERE properties.reported.location = '{locationName.ToLower()}' AND properties.reported.deviceType != 'thermometer'");

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

        /// <summary>
        /// Gets any thermometers at <paramref name="locationName"/> if any exists
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns>
        /// The first ThermometerDevice at <paramref name="locationName"/>
        /// </returns>
        public async Task<ThermometerDevice> GetThermometerAsync(string locationName)
        {
            var device = new ThermometerDevice();
            var result = _registryManager.CreateQuery($"SELECT * FROM devices WHERE properties.reported.location = '{locationName.ToLower()}' AND properties.reported.deviceType = 'thermometer' ");

            foreach (var twin in await result.GetNextAsTwinAsync())
            {
                try { device.Temperature = twin.Properties.Reported["currentTemperature"].ToString(); }
                catch { device.Temperature = "No connection"; }

                try { device.Humidity = twin.Properties.Reported["currentHumidity"].ToString(); }
                catch { device.Humidity = "No connection"; }
            }

            return device;
        }

        /// <summary>
        /// Updates the <paramref name="_deviceItems"/> collection
        /// </summary>
        /// <remarks>
        /// Inserts new devices and removes deleted devices from the list
        /// </remarks>
        /// <param name="_deviceItems"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the powered state in the twin of <paramref name="device"/> based on <paramref name="isChecked"/>
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="device"></param>
        /// <param name="IoTHubConnectionString"></param>
        public async Task UpdatePoweredStateAsync(bool? isChecked, DeviceItem device, string IoTHubConnectionString)
        {
            if (isChecked != null)
            {
                using ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);

                var directMethod = new CloudToDeviceMethod("ChangePoweredState");
                directMethod.SetPayloadJson(JsonConvert.SerializeObject(new { poweredState = isChecked }));
                var result = await serviceClient.InvokeDeviceMethodAsync(device.DeviceId, directMethod);
            }
        }

        /// <summary>
        /// Deletes the specified <paramref name="deviceItem"/>
        /// </summary>
        /// <param name="deviceItem"></param>
        /// <param name="IoTHubConnectionString"></param>
        public async void DeleteDeviceAsync(DeviceItem deviceItem, string IoTHubConnectionString)
        {
            using ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);
            var directMethod = new CloudToDeviceMethod("StopDevice");

            serviceClient.InvokeDeviceMethodAsync(deviceItem.DeviceId, directMethod);

            await _registryManager.RemoveDeviceAsync(deviceItem.DeviceId);
            using IDbConnection connection = new SqlConnection(_dbConnectionString);
            await connection.ExecuteAsync($"DELETE FROM devices WHERE DeviceId = @DeviceId", new { DeviceId = deviceItem.DeviceId });
        }

        /// <summary>
        /// Gets the specified devices twin of device with id <paramref name="deviceId"/>
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns>The twin of the device</returns>
        public async Task<Twin> GetDeviceTwinAsync(string deviceId)
        {
            return await _registryManager.GetTwinAsync(deviceId);
        }
    }
}
