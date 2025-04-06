using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace proyectoTienda.Models
{
  [Table("Productos")]
    public class Producto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IDProducto { get; set; }

    [Required]
    [StringLength(150)]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 1)")]
    public decimal PrecioOriginal { get; set; }

[Column(TypeName = "decimal(10, 1)")]
    public decimal PrecioActual { get; set; }

    [Required]
[Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
public int Stock { get; set; }

    [StringLength(10)]
    public string? Talla { get; set; }

    [NotNull]
    public string? Detalle { get; set; }

    [StringLength(255)]
    public string? ImagenURL { get; set; }

    [Required]
    public int IDCategoria { get; set; }



    // Propiedades de navegación
    [ForeignKey("IDCategoria")]
    public Categoria? Categoria { get; set; }



    public ICollection<DetallePedido>? DetallesPedidos { get; set; }
  }
}