using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proyectoTienda.Models;
namespace proyectoTienda.ViewModel
{
    public class ProductoCategoriaViewModel
{
    public Producto Producto { get; set; }
    public IEnumerable<Categoria> Categorias { get; set; }  // Cambiar aquí a una colección
}

}