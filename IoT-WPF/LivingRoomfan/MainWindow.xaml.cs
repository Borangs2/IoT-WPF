using Dapper;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LivingRoomfan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string connectUrl = "http://localhost:*port*/api/devices/connect";
        private readonly string IoTHubConnectionString =
            "HostName=WebAPI-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ut5rWU8YxIdBNmKaS74GcXGwJuqus3SiFqkQTSj0Ymo=";
        private readonly string DbConnectionString =
            "Server=tcp:iot-wpf.database.windows.net,1433;Initial Catalog=IoT-Devices;Persist Security Info=False;User ID=borangs;Password=HansIsBest1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private bool poweredState = false;
        private readonly TimeSpan interval = TimeSpan.FromHours(1);
        private bool connected = false;

        private Guid deviceId = Guid.Empty;
        private readonly string deviceName = "IntelliFan";
        private readonly string deviceType = "Fan";
        private readonly string deviceOwner = "Andreas Boräng";
        private readonly string location = "Living room";

        

        private async void SubmitPort(object sender, RoutedEventArgs e)
        {
            if (Int16.TryParse(TBoxPort.Text, out short n))
            {
                //Replace port
                connectUrl = connectUrl.Replace("*port*", TBoxPort.Text);

                TBlockInfo.Text = "Initializing Device. Please wait ...";

                using IDbConnection connection = new SqlConnection(DbConnectionString);

                deviceId = await connection.QueryFirstOrDefaultAsync<Guid>("SELECT DeviceId FROM devices WHERE DeviceLocation = @DeviceLocation AND DeviceType = @DeviceType AND DeviceTitle = @DeviceName", new
                {
                    DeviceLocation = location,
                    DeviceType = deviceType,
                    DeviceName = deviceName
                });
                if (deviceId == Guid.Empty)
                {
                    TBlockInfo.Text = "New device detected. Generating new deviceId ...";

                    deviceId = Guid.NewGuid();
                    await connection.ExecuteAsync("INSERT INTO devices (DeviceId, DeviceTitle, DeviceType, DeviceLocation, DeviceOwner, ConnectionString) VALUES (@DeviceId,@DeviceName, @DeviceType, @DeviceLocation, @DeviceOwner, @ConnectionString )",
                        new
                        {
                            DeviceId = deviceId,
                            DeviceName = deviceName,
                            DeviceType = deviceType,
                            DeviceLocation = location,
                            DeviceOwner = deviceOwner,
                            ConnectionString = String.Empty
                        });
                }
                else
                {
                    TBlockInfo.Text = "Device already exists. Getting connection string";
                }


                var deviceConnectionString = await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM devices WHERE DeviceId = @DeviceId", new { DeviceId = deviceId });
                if (string.IsNullOrEmpty(deviceConnectionString))
                {
                    TBlockInfo.Text = "Initializing Connection string ...";

                    using var http = new HttpClient();
                    var result = await http.PostAsJsonAsync($"{connectUrl}?deviceId={deviceId}", new { deviceId = deviceId });

                    deviceConnectionString = await result.Content.ReadAsStringAsync();
                    await connection.ExecuteAsync("UPDATE devices SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId",
                         new { DeviceId = deviceId, ConnectionString = deviceConnectionString });
                }

                DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(IoTHubConnectionString, deviceId.ToString());

                TBlockInfo.Text = "Updating twin properties ...";
                var twinCollection = new TwinCollection();
                twinCollection["deviceTitle"] = deviceName.ToLower();
                twinCollection["deviceType"] = deviceType.ToLower();
                twinCollection["deviceOwner"] = deviceOwner.ToLower();
                twinCollection["poweredState"] = poweredState;
                twinCollection["location"] = location.ToLower();
                await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

                var twin = await _deviceClient.GetTwinAsync();
                poweredState = twin.Properties.Reported["poweredState"];



                await _deviceClient.SetMethodHandlerAsync("ChangePoweredState", ChangePoweredState, null);
                await _deviceClient.SetMethodHandlerAsync("StopDevice", StopDevice, null);

                connected = true;
                TBlockInfo.Text = "Device Connected";

                StackPanelPort.IsEnabled = false;

               
                Loop();
                
            }
            else
            {
                TBlockPortValidation.Text = "Input has to be a number";

            }
        }

        public async Task Loop()
        {
            var twinCollection = new TwinCollection();
            DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(IoTHubConnectionString, deviceId.ToString());

            while (true)
            {
                twinCollection["update"] = "jag existerar";
                await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

                TBlockInfo.Text = $"Last Message sent: {DateTime.Now}";
                await Task.Delay(interval);
            }
        }


        Task<MethodResponse> ChangePoweredState(MethodRequest methodRequest, object userContext)
        {
            var twinCollection = new TwinCollection();
            DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(IoTHubConnectionString, deviceId.ToString());

            poweredState = !poweredState;
            twinCollection["poweredState"] = poweredState;
            _deviceClient.UpdateReportedPropertiesAsync(twinCollection).GetAwaiter();
            TBlockInfo.Text = $"PoweredState now {poweredState}";

            return Task.FromResult(new MethodResponse(new byte[0], 200));
        }

        Task<MethodResponse> StopDevice(MethodRequest methodRequest, object userContext)
        {
            Task.Delay(10000);
            Environment.Exit(0);
            return null;
        }
    }
}
