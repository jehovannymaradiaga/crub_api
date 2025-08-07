using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Models;

namespace ProductManagementAPI.Data;

public partial class ProductosDbContext : DbContext
{
    public ProductosDbContext(DbContextOptions<ProductosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Productos> Productos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Productos>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaModificacion).HasDefaultValueSql("(getdate())");
        });
    }
}
