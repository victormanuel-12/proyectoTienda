using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace proyectoTienda.DTO
{
    public class ContactoCreacionDto
    {
        [Required]
        public string? Mensaje { get; set; }

        public string? Telefono { get; set; }
    }
}
    