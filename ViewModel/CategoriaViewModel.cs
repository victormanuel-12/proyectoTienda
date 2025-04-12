using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proyectoTienda.Models; // Assuming Categoria is in the Models namespace


namespace proyectoTienda.ViewModel
{
  public class CategoriaViewModel
  {
    public Categoria? Categoria { get; set; }
    public int? ProductosCount { get; set; }
  }
}