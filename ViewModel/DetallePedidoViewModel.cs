using proyectoTienda.Models;
using System.Collections.Generic; 

namespace proyectoTienda.ViewModel
{
    public class DetallesPedidoViewModel
    {
        public Pedido? Pedido { get; set; } 
        public IEnumerable<DetallePedido>? Detalles { get; set; } 
    }
}