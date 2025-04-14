using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Models;
namespace proyectoTienda.Data;
using proyectoTienda.Models.Model;
using proyectoTienda.Models.Model.ubicacion;

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
   public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Distrito> Distritos { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Contacto> Contacto { get; set; }
  
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
