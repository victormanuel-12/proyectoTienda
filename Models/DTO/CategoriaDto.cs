// Models/DTO/CategoriaDto.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel; 

namespace proyectoTienda.Models.DTO
{
    public class CategoriaDto
    {
        public int IDCategoria { get; set; } 
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; } 
    }
}