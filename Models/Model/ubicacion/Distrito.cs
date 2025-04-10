using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.Model.ubicacion
{[Table("Distritos")]
    public class Distrito
    {   
        [Key]
        public int DistritoId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        // Relación con Provincia
        
        public int ProvinciaId { get; set; }
        [ForeignKey("ProvinciaId")]
        public Provincia? Provincia { get; set; }
    }
}