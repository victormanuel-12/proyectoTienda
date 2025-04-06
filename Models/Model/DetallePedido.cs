using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models
{
  [Table("DetallesPedidos")]
  public class DetallePedido
  {
    
    [Required]
    public int IDPedido { get; set; }

    
    [Required]
    public int IDProducto { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Cantidad { get; set; }

  
    
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    // Propiedades de navegación
    
    public Pedido? Pedido { get; set; }

   
    public Producto? Producto { get; set; }
    // Método para calcular el subtotal
    public decimal CalcularSubtotal()
        {
            if (Producto == null)
            {
                throw new InvalidOperationException("El producto no puede ser nulo al calcular el subtotal.");
            }
            
            // Calcular el subtotal multiplicando la cantidad por el precio del producto
      
         return Cantidad * Producto.PrecioActual;
      
                
           
        }
        
        // Método para actualizar el subtotal
        public void ActualizarSubtotal()
        {
            Subtotal = CalcularSubtotal();
        }
  }
}