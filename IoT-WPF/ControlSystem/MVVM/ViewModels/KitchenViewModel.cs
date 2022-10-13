using ControlSystem.MVVM.Models;
using ControlSystem.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;

namespace ControlSystem.MVVM.ViewModels;

public class KitchenViewModel : ObservableObject
{
    public string Title { get; set; } = "Kitchen";

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



    public KitchenViewModel()
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