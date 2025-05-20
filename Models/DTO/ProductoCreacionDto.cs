using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel;

namespace proyectoTienda.Models.DTO
{
    public class ProductoCreacionDto
    {
        [Required]
        [StringLength(150)]
        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public decimal PrecioOriginal { get; set; }

        public decimal PrecioActual { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [DefaultValue(0)]
        public int Stock { get; set; }

        [StringLength(10)]
        public string? Talla { get; set; }

        [Required]
        public string? Detalle { get; set; }

        [Required]
        public int IDCategoria { get; set; }

        [Required(ErrorMessage = "Se requiere una imagen para el producto.")]
        public IFormFile? ImagenFile { get; set; }
    }
}