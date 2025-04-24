using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.DTO
{
  public class PagoDTO
  {
    [Required(ErrorMessage = "El número de tarjeta es obligatorio")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
    public string NumeroTarjeta { get; set; }

    [Required(ErrorMessage = "El mes de vencimiento es obligatorio")]
    public string MesVencimiento { get; set; }

    [Required(ErrorMessage = "El año de vencimiento es obligatorio")]
    public string AnioVencimiento { get; set; }

    [Required(ErrorMessage = "El código de seguridad es obligatorio")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El código de seguridad debe tener 3 o 4 dígitos")]
    public string CodigoSeguridad { get; set; }

    [Required(ErrorMessage = "El nombre en la tarjeta es obligatorio")]
    public string NombreTarjeta { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    public string TipoDocumento { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio")]
    public string NumeroDocumento { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El número de teléfono debe tener 9 dígitos")]
    public string Telefono { get; set; }

    [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
    [Display(Name = "Aceptar términos y condiciones")]
    public bool AceptaTerminos { get; set; }
  }
}