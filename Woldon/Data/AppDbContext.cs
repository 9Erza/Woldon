using Microsoft.EntityFrameworkCore;
using Woldon.Models;

namespace Woldon.Data;

/// <summary>
/// Main Database Context for the Woldon application.
/// Manages persistence for network devices and user accounts using Entity Framework Core.
/// </summary>
public class AppDbContext : DbContext
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ==========================================
    // DATA SETS (Tables)
    // ==========================================

    /// <summary>
    /// Registered network devices for Wake-on-LAN management.
    /// </summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>
    /// System users authorized to access the dashboard.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    // ==========================================
    // MODEL CONFIGURATION (Fluent API)
    // ==========================================

    /// <summary>
    /// Configures the database schema and constraints.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Entity Configuration:
        // Ensures that usernames are unique at the database level to prevent duplicates.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Potential for future relations (e.g., linking devices to specific users) 
        // can be defined here using Fluent API.
    }
}