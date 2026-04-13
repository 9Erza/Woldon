using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Woldon.Data;
using Woldon.Models;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Woldon.Controllers;

/// <summary>
/// Core controller for managing network devices and executing Wake-on-LAN actions.
/// Provides full CRUD operations and device status monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WakeController : ControllerBase
{
    // ==========================================
    // CONFIGURATION & CONSTRUCTOR
    // ==========================================
    private readonly AppDbContext _context;

    public WakeController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // GET METHODS (Read)
    // ==========================================

    /// <summary>
    /// Retrieves all registered devices from the database.
    /// </summary>
    [HttpGet("devices")]
    public IActionResult GetDevices()
    {
        var devices = _context.Devices.ToList();
        return Ok(devices);
    }

    /// <summary>
    /// Checks the availability of a device via ICMP Ping.
    /// </summary>
    [HttpGet("status/{ip}")]
    public async Task<IActionResult> GetStatus(string ip)
    {
        try
        {
            using var ping = new Ping();
            // 1000ms timeout ensures the dashboard remains responsive
            var reply = await ping.SendPingAsync(ip, 1000);

            return Ok(new
            {
                online = (reply.Status == IPStatus.Success)
            });
        }
        catch (Exception)
        {
            // Fail silently and return offline status on network errors
            return Ok(new { online = false });
        }
    }

    // ==========================================
    // POST / PUT METHODS (Create & Update)
    // ==========================================

    /// <summary>
    /// Adds a new device configuration to the database.
    /// </summary>
    [HttpPost("add")]
    public IActionResult AddDevice([FromBody] Device device)
    {
        if (device == null) return BadRequest();

        _context.Devices.Add(device);
        _context.SaveChanges();

        return Ok(new { success = true });
    }

    /// <summary>
    /// Updates an existing device configuration.
    /// </summary>
    [HttpPut("update")]
    public IActionResult UpdateDevice([FromBody] Device device)
    {
        var existingDevice = _context.Devices.Find(device.Id);
        if (existingDevice == null) return NotFound();

        // Manual mapping to prevent ID/Foreign Key corruption
        existingDevice.Name = device.Name;
        existingDevice.MacAddress = device.MacAddress;
        existingDevice.IpAddress = device.IpAddress;
        existingDevice.Icon = device.Icon;

        _context.SaveChanges();
        return Ok(new { success = true });
    }

    // ==========================================
    // DELETE METHODS (Remove)
    // ==========================================

    /// <summary>
    /// Removes a device from the system by ID.
    /// </summary>
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteDevice(int id)
    {
        var device = _context.Devices.Find(id);
        if (device == null) return NotFound();

        _context.Devices.Remove(device);
        _context.SaveChanges();
        return Ok(new { success = true });
    }

    // ==========================================
    // ACTIONS (Wake-on-LAN)
    // ==========================================

    /// <summary>
    /// Triggers the Wake-on-LAN process for a specific MAC address.
    /// </summary>
    [HttpPost("run/{mac}")]
    public IActionResult WakeDevice(string mac)
    {
        try
        {
            SendMagicPacket(mac);
            return Ok(new { message = $"Magic Packet successfully broadcasted to {mac}" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // PRIVATE HELPER METHODS
    // ==========================================

    /// <summary>
    /// Constructs and sends a WoL Magic Packet over UDP Port 9.
    /// </summary>
    /// <param name="macAddress">Target device MAC address (format: AA:BB:CC:DD:EE:FF)</param>
    private void SendMagicPacket(string macAddress)
    {
        try
        {
            // 1. Validate MAC Address format
            if (string.IsNullOrEmpty(macAddress) || !macAddress.Contains(":"))
                throw new Exception("Invalid MAC format. Use AA:BB:CC:DD:EE:FF");

            // 2. Convert string to byte array
            byte[] macBytes = macAddress.Split(':')
                .Select(s => Convert.ToByte(s, 16))
                .ToArray();

            if (macBytes.Length != 6)
                throw new Exception("MAC Address must consist of exactly 6 bytes.");

            // 3. Construct Magic Packet structure: 
            // Header (6x 0xFF) + Data (16x Target MAC)
            byte[] packet = new byte[17 * 6];
            for (int i = 0; i < 6; i++) packet[i] = 0xFF;
            for (int i = 1; i <= 16; i++)
                Array.Copy(macBytes, 0, packet, i * 6, 6);

            // 4. Broadcast packet via UDP
            using var client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 9);
            client.Send(packet, packet.Length);
        }
        catch (Exception ex)
        {
            throw new Exception("Broadcast failure: " + ex.Message);
        }
    }
}