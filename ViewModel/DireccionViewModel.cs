using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.ViewModel
{
  public class DireccionViewModel
  {
    [Required(ErrorMessage = "El departamento es obligatorio")]
    public int DepartamentoId { get; set; }

    [Required(ErrorMessage = "La provincia es obligatoria")]
    public int ProvinciaId { get; set; }

    [Required(ErrorMessage = "El distrito es obligatorio")]
    public int DistritoId { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria")]
    public string? DireccionTexto { get; set; }

    public string? Complemento { get; set; }

    // Propiedades de solo lectura para mostrar en la vista
    public string? DepartamentoNombre { get; set; }
    public string? ProvinciaNombre { get; set; }
    public string? DistritoNombre { get; set; }
  }
}