using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyectoTienda.Servicios
{
  public interface ICarritoService
  {
    Task<dynamic> ObtenerCarritoActual(string userID);
  }
}