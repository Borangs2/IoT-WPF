using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Json;
using Dapper;
using Newtonsoft.Json;
using System.Text;

//Andreas port = 7153
string connectUrl = "http://localhost:*port*/api/devices/connect";
string IoTHubConnectionString =
    "HostName=WebAPI-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ut5rWU8YxIdBNmKaS74GcXGwJuqus3SiFqkQTSj0Ymo=";
string DbConnectionString =
    "Server=tcp:iot-wpf.database.windows.net,1433;Initial Catalog=IoT-Devices;Persist Security Info=False;User ID=borangs;Password=HansIsBest1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
bool poweredState = false;
int interval = 30000;
bool connected = false;

Guid deviceId = Guid.Empty;
string deviceName = "IntelliThermometer";
string deviceType = "Thermometer";
string deviceOwner = "Andreas Boräng";
string location = "Bedroom";




Console.WriteLine("Initializing Device. Please wait ...");

//Replace port
Console.Write("What port is being used: ");
connectUrl = connectUrl.Replace("*port*", Console.ReadLine());

using IDbConnection connection = new SqlConnection(DbConnectionString);

deviceId = await connection.QueryFirstOrDefaultAsync<Guid>($"SELECT DeviceId FROM devices WHERE DeviceLocation = @DeviceLocation", new {DeviceLocation = location});
if (deviceId == Guid.Empty)
{
    Console.WriteLine("New device detected. Generating new deviceId ...");

    deviceId = Guid.NewGuid();
    await connection.ExecuteAsync("INSERT INTO devices (DeviceId, DeviceTitle, DeviceType, DeviceLocation, DeviceOwner, ConnectionString) VALUES (@DeviceId,@DeviceName, @DeviceType, @DeviceLocation, @DeviceOwner, @ConnectionString )",
        new { DeviceId = deviceId, DeviceName = deviceName, DeviceType = deviceType, DeviceLocation = location, DeviceOwner = deviceOwner, ConnectionString = String.Empty });
}
else
{
    Console.WriteLine("Device already exists. Getting connection string");
}


var deviceConnectionString = await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM devices WHERE DeviceId = @DeviceId", new { DeviceId = deviceId });
if (string.IsNullOrEmpty(deviceConnectionString))
{
    Console.WriteLine("Initializing Connection string ...");

    using var http = new HttpClient();
    var result = await http.PostAsJsonAsync($"{connectUrl}?deviceId={deviceId}", new { deviceId = deviceId });

    deviceConnectionString = await result.Content.ReadAsStringAsync();
    await connection.ExecuteAsync("UPDATE devices SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId",
         new { DeviceId = deviceId, ConnectionString = deviceConnectionString });
}

DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(IoTHubConnectionString, deviceId.ToString());

Console.WriteLine("Updating twin properties ...");
var twinCollection = new TwinCollection();
twinCollection["deviceTitle"] = deviceName.ToLower();
twinCollection["deviceType"] = deviceType.ToLower();
twinCollection["deviceOwner"] = deviceOwner.ToLower();
twinCollection["poweredState"] = poweredState;
twinCollection["location"] = location.ToLower();
await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

connected = true;
Console.WriteLine("Device Connected");

// ----------------------------------------- \\

bool poweredPreviousState = true;

while (true)
{
    if (connected)
    {
        if (poweredState == poweredPreviousState)
        {
            poweredPreviousState = !poweredState;

            // d2c
            var json = JsonConvert.SerializeObject(new { lightState = poweredState });
            var message = new Message(Encoding.UTF8.GetBytes(json));

            message.Properties.Add("deviceId", deviceId.ToString());
            message.Properties.Add("deviceName", deviceName);
            message.Properties.Add("deviceType", deviceType);
            message.Properties.Add("deviceOwner", deviceOwner);

            await _deviceClient.SendEventAsync(message);

            // device twin
            twinCollection = new TwinCollection();
            twinCollection["poweredState"] = poweredState;
            await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

            Console.WriteLine($"Message sent {DateTime.Now}");
        }
    }
    await Task.Delay(interval);
}

