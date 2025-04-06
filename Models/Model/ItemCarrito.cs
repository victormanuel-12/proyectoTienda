using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

using System.Diagnostics.CodeAnalysis;


namespace proyectoTienda.Models.Model
{
  [Table("ItemsCarrito")]
  public class ItemCarrito

  {


    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IDItem { get; set; }

    public int Cantidad { get; set; }

    
    public Producto? Producto { get; set; }

    public string? UserName { get; set; }  
    [NotNull]
    public Decimal Precio { get; set; }
     public string Status { get; set; } = "PENDIENTE"; 
  }
}