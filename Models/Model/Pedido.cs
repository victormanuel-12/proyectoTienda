using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using proyectoTienda.Areas.Identity.Pages.Account;
using proyectoTienda.Models.Model;

namespace proyectoTienda.Models
{
  [Table("Pedidos")]
  public class Pedido
  {

    [Key]
    public Guid IDPedido { get; set; }



    [Required]
    public string? IDCliente { get; set; }

    [Required]
    public DateTime FechaPedido { get; set; } = DateTime.Now;

    [Required]
    public string? Estado { get; set; } = "PENDIENTE"; // 0=Pendiente, 1=Pagado 

    [Required]
    public int IdDireccion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [Required]
    [StringLength(100)]
    public string? TipoEntrega { get; set; }

    [ForeignKey("IDCliente")]
    public Usuario? Cliente { get; set; }

    [ForeignKey("IdDireccion")]
    public Direccion? Direccion { get; set; }

    public ICollection<DetallePedido>? DetallesPedidos { get; set; }

    public Pago? Pago { get; set; }

    // Methods
    public decimal CalcularTotal()
    {
      decimal total = 0;

      if (DetallesPedidos != null && DetallesPedidos.Any())
      {
        total = DetallesPedidos.Sum(d => d.Subtotal);
      }

      return total;
    }

    public void ActualizarTotales()
    {
      if (DetallesPedidos != null)
      {
        foreach (var detalle in DetallesPedidos)
        {
          detalle.ActualizarSubtotal();
        }

        Total = CalcularTotal();
      }
    }
  }
}