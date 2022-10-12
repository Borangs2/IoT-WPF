using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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
using ControlSystem.MVVM.Models;
using ControlSystem.Services;

namespace ControlSystem.Components
{
    /// <summary>
    /// Interaction logic for DeviceTile.xaml
    /// </summary>
    public partial class DeviceTile : UserControl
    {
        private DeviceService _deviceService;
        private string _connectionString = "HostName=WebAPI-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ut5rWU8YxIdBNmKaS74GcXGwJuqus3SiFqkQTSj0Ymo=";
        public DeviceTile()
        {
            _deviceService =
                new DeviceService(_connectionString);
            InitializeComponent();
        }

        public static readonly DependencyProperty DeviceTitleProperty = DependencyProperty.Register("DeviceTitle", typeof(string), typeof(DeviceTile));
        public string DeviceTitle
        {
            get { return (string)GetValue(DeviceTitleProperty); }
            set { SetValue(DeviceTitleProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty = DependencyProperty.Register("DeviceType", typeof(string), typeof(DeviceTile));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty DeviceIconActiveProperty = DependencyProperty.Register("DeviceIconActive", typeof(string), typeof(DeviceTile));
        public string DeviceIconActive
        {
            get { return (string)GetValue(DeviceIconActiveProperty); }
            set { SetValue(DeviceIconActiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceIconInactiveProperty = DependencyProperty.Register("DeviceIconInactive", typeof(string), typeof(DeviceTile));
        public string DeviceIconInactive
        {
            get { return (string)GetValue(DeviceIconInactiveProperty); }
            set { SetValue(DeviceIconInactiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceFontActiveProperty = DependencyProperty.Register("DeviceFontActive", typeof(string), typeof(DeviceTile));
        public string DeviceFontActive
        {
            get { return (string)GetValue(DeviceFontActiveProperty); }
            set { SetValue(DeviceFontActiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceFontInactiveProperty = DependencyProperty.Register("DeviceFontInactive", typeof(string), typeof(DeviceTile));
        public string DeviceFontInactive
        {
            get { return (string)GetValue(DeviceFontActiveProperty); }
            set { SetValue(DeviceFontActiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceStateInactiveProperty = DependencyProperty.Register("DeviceStateActive", typeof(string), typeof(DeviceTile));
        public string DeviceStateActive
        {
            get { return (string)GetValue(DeviceFontInactiveProperty); }
            set { SetValue(DeviceFontInactiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceStateActiveProperty = DependencyProperty.Register("DeviceStateInactive", typeof(string), typeof(DeviceTile));
        public string DeviceStateInactive
        {
            get { return (string)GetValue(DeviceFontActiveProperty); }
            set { SetValue(DeviceFontActiveProperty, value); }
        }

        private async void OnOffSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            OnOffSwitch.IsEnabled = false;

            var button = sender as CheckBox;
            var deviceItem = (DeviceItem) button!.DataContext;

            await _deviceService.UpdatePoweredStateAsync(OnOffSwitch.IsChecked, deviceItem, _connectionString);
            OnOffSwitch.IsEnabled = true;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            var deviceItem = (DeviceItem) button!.DataContext;

            _deviceService.DeleteDeviceAsync(deviceItem, _connectionString);
        }
    }
}
