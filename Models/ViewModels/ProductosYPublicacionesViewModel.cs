using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using proyectoTienda.Models.DTO;
using System.ComponentModel.DataAnnotations;
using proyectoTienda.Servicios;

namespace proyectoTienda.Models.ViewModels
{
    public class ProductosYPublicacionesViewModel
    {
        public List<Producto> ProductosRecientes { get; set; } = new();
        public List<SubstackPostDto> Publicaciones { get; set; } = new();
    }
}
