
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace proyectoTienda.Models.DTO
{
    public class CategoriaCreacionDto
    {
        [Display(Prompt = "Nombre de la categoría")]
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")] 
        public string? Nombre { get; set; }

        [Display(Prompt = "Descripción de la categoría")] 
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")] 
        public string? Descripcion { get; set; } 
    }
}