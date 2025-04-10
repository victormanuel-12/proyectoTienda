using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.Model.ubicacion
{    [Table("Departamentos")]
    public class Departamento
    {
        [Key]
        public int DepartamentoId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        // Relación con Provincias
        public ICollection<Provincia>? Provincias { get; set; }
    }
}