using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using proyectoTienda.Models.ENUM;

namespace proyectoTienda.Models
{
  [Table("Usuarios")]
  public class Usuario
  {
    [Key]
    public string? ID { get; set; }


    [StringLength(100)]
    public string? Nombre { get; set; }


    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }




    public string? TipoUsuario { get; set; } // 0 = Cliente, 1 = Admin
                                             // Enum para representar los tipos de usuario


    public ICollection<Pedido>? Pedidos { get; set; }
  }
}