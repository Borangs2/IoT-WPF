using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using ControlSystem.MVVM.Models;
using ControlSystem.Services;
using Microsoft.Azure.Devices;

namespace ControlSystem.MVVM.ViewModels;

public class BedroomViewModel : ObservableObject
{
    public string Title { get; set; } = "Bedroom";

    private DeviceService _deviceService = new DeviceService("HostName=WebAPI-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ut5rWU8YxIdBNmKaS74GcXGwJuqus3SiFqkQTSj0Ymo=");
    private ObservableCollection<DeviceItem> _deviceItems;
    private DispatcherTimer _timer;
    private ThermometerDevice _thermometer = new ThermometerDevice();

    public IEnumerable<DeviceItem> DeviceItems => _deviceItems;

    public ThermometerDevice Thermometer
    {
        get { return _thermometer; }
        set
        {
            _thermometer = value;
            OnPropertyChanged();
        }
    }

    private string _currentTemperature;

    

    public BedroomViewModel()
    {
        _deviceItems = new ObservableCollection<DeviceItem>();
        PopulateDeviceItemsAsync().ConfigureAwait(false);
        SetTimer(TimeSpan.FromSeconds(30));
    }


    private void SetTimer(TimeSpan interval)
    {
        _timer = new DispatcherTimer()
        {
            Interval = interval
        };
        _timer.Tick += new EventHandler(timer_tick);
        _timer.Start();
    }

    private async void timer_tick(object sender, EventArgs e)
    {
        await PopulateDeviceItemsAsync();
        await GetThermometerData();
        await UpdateDeviceItemsAsync();
    }

    private async Task GetThermometerData()
    {
        Thermometer = await _deviceService.GetThermometerAsync("Bedroom");
    }

    public async Task PopulateDeviceItemsAsync()
    {
        _deviceItems = await _deviceService.PopulateDeviceItemsAsync(Title, _deviceItems);
    }
    public async Task UpdateDeviceItemsAsync()
    {
        _deviceItems = await _deviceService.UpdateDeviceItemsAsync(_deviceItems);
    }
}