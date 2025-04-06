using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Models;
namespace proyectoTienda.Data;
using proyectoTienda.Models.Model;

public class ApplicationDbContext : IdentityDbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }
  public DbSet<Usuario> Usuarios { get; set; }
  public DbSet<Producto> Productos { get; set; }
  public DbSet<Pedido> Pedidos { get; set; }
  public DbSet<DetallePedido> DetallesPedidos { get; set; }
  public DbSet<Categoria> Categorias { get; set; }
  public DbSet<Pago> Pagos { get; set; }
  public DbSet<ItemCarrito> ItemsCarrito { get; set; }
  
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<DetallePedido>()
        .HasKey(dp => new { dp.IDPedido, dp.IDProducto });

    modelBuilder.Entity<DetallePedido>()
        .HasOne(dp => dp.Pedido)
        .WithMany(p => p.DetallesPedidos)
        .HasForeignKey(dp => dp.IDPedido);

    modelBuilder.Entity<DetallePedido>()
        .HasOne(dp => dp.Producto)
        .WithMany(p => p.DetallesPedidos)
        .HasForeignKey(dp => dp.IDProducto);
  }

      
}
