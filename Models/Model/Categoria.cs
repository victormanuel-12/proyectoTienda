using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models
{
  [Table("Categorias")]
  public class Categoria
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public int IDCategoria { get; set; }
    public string Nombre { get; set; }

    public string Descripcion { get; set; }
    
  public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    
    // Propiedad adicional para contar productos
    public int ProductosCount { get; set; }

public Categoria()
    {
        FechaCreacion = DateTime.Now;
    }
    // Propiedad de navegación
    public ICollection<Producto>? Productos { get; set; }
    public int ContarProductos() 
{
    return Productos?.Count() ?? 0; // Devuelve 0 si Productos es null
}

  }
}