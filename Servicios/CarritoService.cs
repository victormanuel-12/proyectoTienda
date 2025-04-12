using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Models;
using proyectoTienda.Data;
using System.Dynamic;
using proyectoTienda.Models.Model;
using proyectoTienda.Servicios;


namespace proyectoTienda.Servicios
{
  public class CarritoService : ICarritoService
  {
    private ApplicationDbContext _context;


    public CarritoService(ApplicationDbContext context)
    {
      _context = context;

    }

    public async Task<dynamic> ObtenerCarritoActual(string userID) // Cambié el tipo de retorno a Task<dynamic>
    {
      // Cambié a GetUserNameAsync, que es un método asincrónico
      var elementos = await _context.ItemsCarrito
          .Include(o => o.Producto)
          .Where(o => o.UserName == userID && o.Status == "PENDIENTE")
          .ToListAsync();

      decimal montoOriginal = elementos.Sum(e => e.Producto.PrecioOriginal * e.Cantidad);
      decimal montoActual = elementos.Sum(e => e.Producto.PrecioActual * e.Cantidad);
      dynamic model = new ExpandoObject();
      model.montoOriginal = montoOriginal;
      model.montoActual = montoActual;
      model.elementosCarrito = elementos;

      return model; // Cambié a return model; para devolver el modelo dinámico
    }
  }
}
