using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.Model
{
    [Table("Contacto")]
    public class Contacto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set;}

        public string? Nombre { get; set; }

        public string? Correo { get; set; }

        [Required]
        public string? Mensaje { get; set; }

        public string? Telefono { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}