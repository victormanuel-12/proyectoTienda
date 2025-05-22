using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel; 

namespace proyectoTienda.DTO
{
    public class ContactoDto
    {
        public int Id { get; set; }
        public string? Correo { get; set; }
        public string? Mensaje { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
