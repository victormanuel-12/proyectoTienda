using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace proyectoTienda.Models.DTO
{
    public class CategoriaActualizacionDto
    {
        [Required(ErrorMessage = "El ID de la categoría es obligatorio para la actualización.")]
        [Display(Prompt = "ID de la categoría a actualizar")]
        public int IDCategoria { get; set; }

        [Display(Prompt = "Nuevo nombre de la categoría")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string? Nombre { get; set; } 

        [Display(Prompt = "Nueva descripción de la categoría")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Descripcion { get; set; }
    }
}