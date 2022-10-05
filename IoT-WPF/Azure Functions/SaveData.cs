using System;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Azure_Functions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure_Functions
{
    public class SaveData
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("SaveData")]
        public void Run([IoTHubTrigger("messages/events", Connection = "IoTHubEndpoint")]EventData message,
            [CosmosDB(databaseName: "iot-wpf", collectionName: "Data", CreateIfNotExists = true, ConnectionStringSetting = "CosmosDb")] out dynamic output,
            ILogger log)
        {
            try
            {
                using var registryManager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHub"));
                var twin = Task.Run(() =>
                    registryManager.GetTwinAsync(message.SystemProperties["iothub-connect"].ToString())).Result;

                var data = new SaveDataModel();
                try { data.DeviceId = message.SystemProperties["iothub-connection-device-id"].ToString() ?? twin.DeviceId; }
                catch { }
                try { data.DeviceName = message.SystemProperties["iothub-connection-device-type"].ToString() ?? twin.Properties.Reported["deviceName"]; }
                catch { }
                try { data.DeviceType = message.SystemProperties["iothub-connection-device-type"].ToString() ?? twin.Properties.Reported["deviceType"]; }
                catch { }
                try { data.Location = message.SystemProperties["iothub-connection-location"].ToString() ?? twin.Properties.Reported["location"]; }
                catch { }
                try { data.Owner = message.SystemProperties["iothub-connection-owner"].ToString() ?? twin.Properties.Reported["owner"]; }
                catch { }
                try { data.Data = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(message.Body.Array)); }
                catch { }

                output = data;
            }
            catch
            {
                output = null;
            }
            log.LogInformation($"Message: {Encoding.UTF8.GetString(message.Body.Array)}");
        }
    }
}