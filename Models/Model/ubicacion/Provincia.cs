using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.Model.ubicacion
{   [Table("Provincias")]
    public class Provincia
    {
        [Key]
        public int ProvinciaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        // Relación con Departamento
        
        public int DepartamentoId { get; set; }
        
        [ForeignKey("DepartamentoId")]
        public Departamento? Departamento { get; set; }

        // Relación con Distritos
        public ICollection<Distrito>? Distritos { get; set; }
    }
}