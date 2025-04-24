using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models
{
  [Table("Pagos")]
  public class Pago
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IDPago { get; set; }

    [Required]
    public Guid IDPedido { get; set; }

    [Required]
    public string? MetodoPago { get; set; } // 0=Tarjeta, 1=PayPal, 2=Transferencia

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Monto { get; set; }

    [Required]
    public DateTime FechaPago { get; set; } = DateTime.Now;

    // Datos de tarjeta
    [Required(ErrorMessage = "El número de tarjeta es obligatorio")]
    [Display(Name = "Número de tarjeta")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "El número debe contener exactamente 16 dígitos.")]
    [NotMapped]
    public string NumeroTarjeta { get; set; }

    [Required(ErrorMessage = "El mes de vencimiento es obligatorio")]
    [Display(Name = "Mes de vencimiento")]
    [NotMapped]
    public string MesVencimiento { get; set; }

    [Required(ErrorMessage = "El año de vencimiento es obligatorio")]
    [Display(Name = "Año de vencimiento")]
    [NotMapped]
    public string AnioVencimiento { get; set; }

    [Required(ErrorMessage = "El código de seguridad es obligatorio")]
    [Display(Name = "Código de seguridad")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El código debe contener 3 o 4 dígitos")]
    [NotMapped]
    public string CodigoSeguridad { get; set; }

    [Required(ErrorMessage = "El nombre del titular es obligatorio")]
    [Display(Name = "Nombre en la tarjeta")]
    [NotMapped]
    public string NombreTarjeta { get; set; }

    // Datos personales
    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    [Display(Name = "Tipo de documento")]
    public string TipoDocumento { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio")]
    [Display(Name = "Número de documento")]
    public string NumeroDocumento { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Display(Name = "Teléfono")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe contener 9 dígitos")]

    public string Telefono { get; set; }

    [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
    [Display(Name = "Aceptar términos y condiciones")]
    [NotMapped]
    public bool AceptaTerminos { get; set; }

    // Propiedad de navegación
    [ForeignKey("IDPedido")]
    public Pedido? Pedido { get; set; }
  }
}