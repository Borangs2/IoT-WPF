using Newtonsoft.Json;

namespace Azure_Functions.Models;

public class SaveDataModel
{
    [JsonProperty("deviceId")] public string DeviceId { get; set; } = "";

    [JsonProperty("deviceName")] public string DeviceName { get; set; } = "";

    [JsonProperty("deviceType")] public string DeviceType { get; set; } = "";

    [JsonProperty("location")] public string Location { get; set; } = "";

    [JsonProperty("owner")] public string Owner { get; set; } = "";

    [JsonProperty("data")] public dynamic Data { get; set; } = "";
}