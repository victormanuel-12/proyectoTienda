using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proyectoTienda.Models;

namespace proyectoTienda.ViewModel
{
  public class ProductoDetalleViewModel
  {
    public Producto? ProductoPrincipal { get; set; }
    public IEnumerable<Producto>? ProductosSimilares { get; set; }
  }
}