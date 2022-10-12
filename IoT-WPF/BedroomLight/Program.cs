//Andreas port = 7153

using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Json;
using System.Net.Mime;
using Dapper;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

string connectUrl = "http://localhost:*port*/api/devices/connect";
string IoTHubConnectionString =
    "HostName=WebAPI-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ut5rWU8YxIdBNmKaS74GcXGwJuqus3SiFqkQTSj0Ymo=";
string DbConnectionString =
    "Server=tcp:iot-wpf.database.windows.net,1433;Initial Catalog=IoT-Devices;Persist Security Info=False;User ID=borangs;Password=HansIsBest1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
bool poweredState = false;
TimeSpan interval = TimeSpan.FromHours(1);
bool connected = false;

Guid deviceId = Guid.Empty;
string deviceName = "IntelliLight1";
string deviceType = "Light";
string deviceOwner = "Andreas Boräng";
string location = "Bedroom";




Console.WriteLine("Initializing Device. Please wait ...");

//Replace port
Console.Write("What port is being used: ");
connectUrl = connectUrl.Replace("*port*", Console.ReadLine());

using IDbConnection connection = new SqlConnection(DbConnectionString);

deviceId = await connection.QueryFirstOrDefaultAsync<Guid>("SELECT DeviceId FROM devices WHERE DeviceLocation = @DeviceLocation AND DeviceType = @DeviceType AND DeviceTitle = @DeviceName", new { DeviceLocation = location, DeviceType = deviceType, DeviceName = deviceName }); 
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

var twin = await _deviceClient.GetTwinAsync();
poweredState = twin.Properties.Reported["poweredState"];



await _deviceClient.SetMethodHandlerAsync("ChangePoweredState", ChangePoweredState, null);
await _deviceClient.SetMethodHandlerAsync("StopDevice", StopDevice, null);

connected = true;
Console.WriteLine("Device Connected");

while (true)
{
    twinCollection["update"] = "jag existerar";
    await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

    Console.WriteLine("Message sent");
    await Task.Delay(interval);
}

Task<MethodResponse> ChangePoweredState(MethodRequest methodRequest, object userContext)
{
    poweredState = !poweredState;
    twinCollection["poweredState"] = poweredState;
    _deviceClient.UpdateReportedPropertiesAsync(twinCollection).GetAwaiter();
    Console.WriteLine($"PoweredState now {poweredState}");

    return Task.FromResult(new MethodResponse(new byte[0], 200));
}

Task<MethodResponse> StopDevice(MethodRequest methodRequest, object userContext)
{
    Task.Delay(10000);
    Environment.Exit(0);
    return null;
}