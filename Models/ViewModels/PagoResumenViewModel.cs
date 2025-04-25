using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proyectoTienda.Models.DTO;
using System.ComponentModel.DataAnnotations;
namespace proyectoTienda.Models.ViewModels
{
  public class PagoResumenViewModel
  {
    public PagoDTO Pago { get; set; } = new PagoDTO();

    // Atributos para el resumen
    public decimal montoOriginal { get; set; }

    public decimal montoActual { get; set; }
    public int elementosCarrito { get; set; }
  }
}