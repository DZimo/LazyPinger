using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using Microsoft.EntityFrameworkCore;

namespace LazyPinger.Base.Models;

public partial class LazyPingerDbContext : DbContext
{
    private string DbFilePath = $"Data Source={Path.Combine(AppContext.BaseDirectory, "lazypinger_database.db")}";

    public LazyPingerDbContext()
    {
        var localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LazyPinger");

        if (!File.Exists(localFolder))
            Directory.CreateDirectory(localFolder);

        DbFilePath = $"Data Source={Path.Combine(localFolder, "lazypinger_database.db")}";
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
