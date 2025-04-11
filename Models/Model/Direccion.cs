using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.Model
{
  [Table("Direcciones")]
  public class Direccion
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDireccion { get; set; }



    [Required]
    [MaxLength(200)]
    public string? DireccionTexto { get; set; }


    [MaxLength(100)]
    public string? Complemento { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Distrito { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Provincia { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Departamento { get; set; }

    // Relación de navegación

    public ICollection<Pedido>? Pedidos { get; set; }
  }
}