using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoTienda.Models.DTO
{
    public class ProductoDto
    {
        public int IDProducto { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioOriginal { get; set; }
        public decimal PrecioActual { get; set; }
        public int Stock { get; set; }
        public string? Talla { get; set; }
        public string? Detalle { get; set; }
        public string? ImagenURL { get; set; }
        public int IDCategoria { get; set; }

    }
}