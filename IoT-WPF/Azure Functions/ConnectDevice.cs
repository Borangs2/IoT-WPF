using System;
using System.IO;
using System.Threading.Tasks;
using Azure_Functions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure_Functions
{
    public static class ConnectDevice
    {
        private static readonly RegistryManager _registryManager =
            RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHub"));
        [FunctionName("ConnectDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/connect")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<DeviceRequestModel>(await new StreamReader(req.Body).ReadToEndAsync());
                var device = await _registryManager.AddDeviceAsync(new Device(body?.DeviceId));

                var connectionString =
                    $"{Environment.GetEnvironmentVariable("IoTHub").Split(";")[0]};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";

                return new OkObjectResult(connectionString);
            }
            catch (Exception e)
            {
                return new BadRequestResult();
            }
        }
    }
}
