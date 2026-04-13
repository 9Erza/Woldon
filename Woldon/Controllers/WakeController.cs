using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Woldon.Models;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace Woldon.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WakeController : ControllerBase
{
    private readonly WakeOnLanSettings _settings;

    public WakeController(IOptions<WakeOnLanSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("devices")]
    public IActionResult GetDevices()
    {
        return Ok(_settings.Devices);
    }

    [HttpPost("run/{mac}")]
    public async Task<IActionResult> WakeDevice(string mac)
    {
        try
        {
            await SendMagicPacket(mac);
            return Ok(new { success = true, message = $"Wysłano impuls do: {mac}" });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    private async Task SendMagicPacket(string macAddress)
    {

        string cleanMac = macAddress.Replace("-", "").Replace(":", "");
        byte[] macBytes = Enumerable.Range(0, cleanMac.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(cleanMac.Substring(x, 2), 16))
                                    .ToArray();


        byte[] packet = new byte[6 + 16 * 6];
        for (int i = 0; i < 6; i++) packet[i] = 0xFF;
        for (int i = 0; i < 16; i++)
            Array.Copy(macBytes, 0, packet, 6 + i * 6, 6);


        using var client = new UdpClient();
        await client.SendAsync(packet, packet.Length, new IPEndPoint(IPAddress.Broadcast, 9));
    }
}