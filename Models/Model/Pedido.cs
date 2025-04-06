using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using proyectoTienda.Areas.Identity.Pages.Account; // Corrected namespace for ApplicationUser

namespace proyectoTienda.Models
{
  [Table("Pedidos")]
  public class Pedido
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IDPedido { get; set; }

    [Required]
    public string? IDCliente { get; set; }

    [Required]
    public DateTime FechaPedido { get; set; } = DateTime.Now;

    [Required]
    public int Estado { get; set; } = 0; // 0=Pendiente, 1=Pagado, 2=Enviado, 3=Entregado, 4=Cancelado

  [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }
        
    // Propiedades de navegación
    [ForeignKey("IDCliente")]
        public Usuario? Cliente { get; set; }

    public ICollection<DetallePedido>? DetallesPedidos { get; set; }

    public Pago? Pago { get; set; }
    // Método para calcular el total del pedido
    public decimal CalcularTotal()
        {
            decimal total = 0;

            if (DetallesPedidos != null && DetallesPedidos.Any())
            {
                total = DetallesPedidos.Sum(d => d.Subtotal);
            }

            return total;
        }

        // Método para actualizar los subtotales y el total
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