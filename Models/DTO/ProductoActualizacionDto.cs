using Microsoft.AspNetCore.Http; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace proyectoTienda.Models.DTO
{
    public class ProductoActualizacionDto
    {
        [Required]
        [Display(Prompt = "ID del producto")]
        public int IDProducto { get; set; }

        [Display(Prompt = "Nombre del producto")]
        [StringLength(150)]
        public string? Nombre { get; set; }

        [Display(Prompt = "Descripción del producto")]
        public string? Descripcion { get; set; }

        [Display(Prompt = "Precio original")]
        public decimal? PrecioOriginal { get; set; } 

        [Display(Prompt = "Precio actual")]
        public decimal? PrecioActual { get; set; }

        [Display(Prompt = "Stock disponible")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [DefaultValue(0)]
        public int? Stock { get; set; }

        [Display(Prompt = "Talla del producto")]
        [StringLength(10)]
        public string? Talla { get; set; }

        [Display(Prompt = "Detalles del producto")]
        public string? Detalle { get; set; }

        [Display(Prompt = "ID de categoria")]
        public int? IDCategoria { get; set; }

        [Display(Prompt = "Subir imagen (opcional)")]
        public IFormFile? ImagenFile { get; set; }
    }
}