using System.ComponentModel.DataAnnotations;

namespace Woldon.Models;

// ==========================================
// DEVICE ENTITY
// Represents a target machine in the network
// ==========================================
public class Device
{
    /// <summary>
    /// Unique identifier for the device (Primary Key).
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User-friendly name of the machine (e.g., "Home Server").
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical MAC address used for Wake-on-LAN broadcast.
    /// Format: AA:BB:CC:DD:EE:FF
    /// </summary>
    [Required]
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// Local IP address used for ICMP Status checks (Ping).
    /// </summary>
    [Required]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// CSS class or identifier for the UI icon representation.
    /// Default: "desktop"
    /// </summary>
    public string Icon { get; set; } = "desktop";
}

// ==========================================
// USER ENTITY
// Represents an authorized system administrator
// ==========================================
public class User
{
    /// <summary>
    /// Unique identifier for the user (Primary Key).
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Authentication username (Unique index defined in AppDbContext).
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Credential storage. 
    /// Should store salted hashes in a production environment.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Secondary security layer for executing Wake-on-LAN actions.
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string WakePin { get; set; } = string.Empty;
}