namespace Woldon.Models;

public class Device
{
    public string Name { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
}

public class WakeOnLanSettings
{
    public List<Device> Devices { get; set; } = new();
}