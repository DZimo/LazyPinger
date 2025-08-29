using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LazyPinger.Base.Models;

public partial class LazyPingerDbContext : DbContext
{
    private static readonly string DbFilePath = $"Data Source={Path.Combine(AppContext.BaseDirectory, "lazypinger_database.db")}";


    public LazyPingerDbContext()
    {

    }

    public LazyPingerDbContext(DbContextOptions<LazyPingerDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(DbFilePath);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<UserSelection> UserSelections { get; set; }
    public DbSet<DevicePing> DevicePings { get; set; }
    public DbSet<DevicesGroup> DevicesGroups { get; set; }

}
