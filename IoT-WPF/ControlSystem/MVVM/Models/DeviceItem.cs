namespace ControlSystem.MVVM.Models;

public class DeviceItem
{
    public string DeviceId { get; set; } = "";
    public string DeviceTitle { get; set; } = "";
    public string DeviceType { get; set; } = "";


    public string IconActive { get; set; } = "";
    public string IconInactive { get; set; } = "";
    public string FontActive { get; set; } = "";
    public string FontInactive { get; set; } = "";
    public string StateActive { get; set; } = "";
    public string StateInactive { get; set; } = "";
}