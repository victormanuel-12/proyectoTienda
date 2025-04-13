using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Models;
using proyectoTienda.Data;
using System.Dynamic;
using proyectoTienda.Models.Model;
using proyectoTienda.Servicios;
using Microsoft.AspNetCore.Authorization;

namespace proyectoTienda.Controllers
{
  [Authorize(Roles = "USER")]
  public class CarritoController : Controller
  {
    private readonly ILogger<CarritoController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ICarritoService _carritoService;

    public CarritoController(
        ILogger<CarritoController> logger,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context,
        ICarritoService carritoService) // Asegúrate de que esta dependencia esté siendo inyectada
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
      _carritoService = carritoService;
    }
    public async Task<IActionResult> Carrito()
    {
      var userID = _userManager.GetUserName(User);
      _logger.LogInformation("Usuario actual: {UserID}", userID ?? "null");
      if (userID == null)
      {
        _logger.LogInformation("No existe usuario");
        TempData["Mensaje"] = "Usuario No autenticado, inicia sesión por favor";
        return RedirectToAction("Index", "Catalogo");
      }

      var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userID);
      // Buscar el usuario de AspNetUsers con el nombre de usuario
      var usuarioAspNet = await _userManager.FindByNameAsync(userID);
      // Log information about the AspNetUser

      if (usuarioExistente == null)
      {
        var nuevoUsuario = new Usuario
        {
          ID = usuarioAspNet.Id,
          Nombre = "User",
          Email = usuarioAspNet?.Email ?? "email@gamil.com",
          TipoUsuario = "CLIENTE" // Asigna un valor por defecto o ajusta según tu lógica
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();
      }

      var preciosOriginales = await _context.Productos
          .Select(o => o.PrecioOriginal)
          .ToListAsync();

      var model = await _carritoService.ObtenerCarritoActual(userID);
      model.preciosOriginales = preciosOriginales;

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AgregarAlCarrito(int productoId, int cantidad)
    {
      var userID = _userManager.GetUserName(User);

      if (userID == null)
      {
        _logger.LogInformation("No existe usuario");
        return Json(new { success = false, message = "Usuario no autenticado, inicia sesión por favor" });
      }

      var producto = await _context.Productos.FindAsync(productoId);

      if (producto == null)
      {
        _logger.LogInformation($"Producto con ID {productoId} no encontrado");
        return Json(new { success = false, message = "Producto no encontrado" });
      }

      if (producto.Stock < cantidad)
      {
        return Json(new { success = false, message = $"Stock insuficiente. Solo hay {producto.Stock} disponibles." });
      }

      var existingItem = await _context.ItemsCarrito
          .FirstOrDefaultAsync(i => i.UserName == userID &&
                                    i.Producto.IDProducto == productoId &&
                                    i.Status == "PENDIENTE");

      try
      {
        if (existingItem != null)
        {
          existingItem.Cantidad = cantidad;
          await _context.SaveChangesAsync();
          _logger.LogInformation("Cantidad actualizada en el carrito");
          return Json(new { success = true, message = "Se actualizó la cantidad del producto en el carrito" });
        }
        else
        {
          var item = new ItemCarrito
          {
            UserName = userID,
            Producto = producto,
            Cantidad = cantidad,
            Status = "PENDIENTE"
          };

          _context.ItemsCarrito.Add(item);
          await _context.SaveChangesAsync();

          _logger.LogInformation("Producto agregado al carrito");
          return Json(new { success = true, message = "Producto agregado al carrito" });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al guardar el carrito: {ex.Message}", ex);
        return Json(new { success = false, message = "Hubo un error al intentar actualizar el carrito. Intente más tarde." });
      }
    }

    public async Task<IActionResult> Eliminar(int? idProductoEliminar)
    {
      var response = new { success = false, message = "No se pudo eliminar el producto.", carrito = (object)null };

      try
      {
        // Validar si el idProductoEliminar está presente
        if (!idProductoEliminar.HasValue)
        {
          response = new { success = false, message = "El ID del producto es obligatorio.", carrito = (object)null };
          return Json(response);
        }

        // Buscar el producto en la base de datos
        var item = await _context.ItemsCarrito.FindAsync(idProductoEliminar.Value);
        if (item == null)
        {
          response = new { success = false, message = "Producto no encontrado.", carrito = (object)null };
          return Json(response);
        }

        // Eliminar el producto del carrito
        _context.ItemsCarrito.Remove(item);
        await _context.SaveChangesAsync();
        var userID = _userManager.GetUserName(User);
        var modelo = await _carritoService.ObtenerCarritoActual(userID);
        response = new { success = true, message = "Producto eliminado correctamente.", carrito = modelo };
      }
      catch (Exception ex)
      {
        response = new { success = false, message = "Error al procesar la solicitud.", carrito = (object)null };
        _logger.LogError(ex, "Error al eliminar producto del carrito.");
      }

      return Json(response);
    }





    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}
