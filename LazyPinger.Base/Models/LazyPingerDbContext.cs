using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using Microsoft.EntityFrameworkCore;

namespace LazyPinger.Base.Models;

public partial class LazyPingerDbContext : DbContext
{
    private string DbFilePath = Path.Combine(AppContext.BaseDirectory, "lazypinger_database.db");
    private string DbSecret = "";
    public bool IsAndroid = false;

    public LazyPingerDbContext()
    {

    }

    public LazyPingerDbContext(DbContextOptions<LazyPingerDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(DbSecret);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public void CheckDatabasePath()
    {
        DbSecret = $"Data Source={DbFilePath}";

        if (File.Exists(DbFilePath) || IsAndroid)
            return;

        var localFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LazyPinger");

        if (!File.Exists(localFolder))
            Directory.CreateDirectory(localFolder);

        DbSecret = $"Data Source={Path.Combine(localFolder, "lazypinger_database.db")}";
    }

    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<UserSelection> UserSelections { get; set; }
    public DbSet<DevicePing> DevicePings { get; set; }
    public DbSet<DevicesGroup> DevicesGroups { get; set; }

}
