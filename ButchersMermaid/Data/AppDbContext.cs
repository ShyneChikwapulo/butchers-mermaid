using ButchersMermaid.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ButchersMermaid.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuPrice> MenuPrices => Set<MenuPrice>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.DisplayGroup).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.SortOrder);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).HasMaxLength(1000);
            entity.Property(i => i.PriceNote).HasMaxLength(200);

            // Prices are stored as decimal(18,2) — Rands and cents.
            // No currency conversion, no floating point rounding bugs.
            entity.Property(i => i.BasePrice).HasPrecision(18, 2);

            entity.HasOne(i => i.Category)
                  .WithMany(c => c.Items)
                  .HasForeignKey(i => i.MenuCategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => i.SortOrder);
        });

        modelBuilder.Entity<MenuPrice>(entity =>
        {
            entity.Property(p => p.PortionLabel).HasMaxLength(50);
            entity.Property(p => p.Amount).HasPrecision(18, 2);

            entity.HasOne(p => p.Item)
                  .WithMany(i => i.Prices)
                  .HasForeignKey(p => p.MenuItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.SortOrder);
        });

        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.BusinessName).HasMaxLength(200);
            entity.Property(e => e.BusinessType).HasMaxLength(100);
            entity.Property(e => e.MonthlyVolume).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.SubmittedFromIp).HasMaxLength(64);
            entity.Property(e => e.EmailError).HasMaxLength(1000);

            // Type is stored as int (default), but index it because we'll
            // filter by type on the admin page.
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.SubmittedAt);
            entity.HasIndex(e => e.IsRead);
        });

    }
}