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

namespace proyectoTienda.Controllers
{
  public class CarritoController : Controller
  {
    private readonly ILogger<CarritoController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CarritoController(
      ILogger<CarritoController> logger,
      UserManager<IdentityUser> userManager, 
      ApplicationDbContext context)
    {
      _userManager = userManager;
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Carrito()
    {
      var userID = _userManager.GetUserName(User);

      if (userID == null)
      {
        _logger.LogInformation("No existe usuario");
        TempData["Mensaje"] = "Usuario No autenticado, inicia sesión por favor";
        return RedirectToAction("Index", "Catalogo");
      }

      var items = await _context.ItemsCarrito
        .Include(o => o.Producto)
        .Where(o => o.UserName == userID && o.Status == "PENDIENTE")
        .ToListAsync();
        
      var totalOriginal = items.Sum(o => o.Precio * o.Cantidad); // Calcula el total del carrito

      var preciosOriginales = await _context.Productos
        .Select(o => o.PrecioOriginal)
        .ToListAsync();

      var totalActual = items.Sum(o => (o.Producto?.PrecioActual ?? 0) * o.Cantidad);
      
      dynamic model = new ExpandoObject();
      model.montoOriginal = totalOriginal;
      model.montoActual = totalActual;
      model.elementosCarrito = items;
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
        TempData["Mensaje"] = "Usuario No autenticado, inicia sesión por favor";
        return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
      }
      else
      {
        var producto = await _context.Productos.FindAsync(productoId);

        if (producto == null)
        {
          _logger.LogInformation($"Producto con ID {productoId} no encontrado");
          TempData["Mensaje"] = "Producto no encontrado";
          return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
        }

        if (producto.Stock < cantidad)
        {
          TempData["Mensaje"] = $"Stock insuficiente. Solo hay {producto.Stock} disponibles.";
          return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
        }
        
        // Check if the product already exists in the user's cart with PENDIENTE status
        var existingItem = await _context.ItemsCarrito
          .Include(i => i.Producto)
          .FirstOrDefaultAsync(i => i.UserName == userID && 
                    i.Producto.IDProducto == productoId && 
                    i.Status == "PENDIENTE");

        if (existingItem != null)
        {
          // Update the quantity of the existing item
          existingItem.Cantidad = cantidad;
          
          try
          {
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Se actualizó la cantidad del producto en el carrito";
            _logger.LogInformation("Se actualizó la cantidad de un producto en el carrito");
            return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
          }
          catch (Exception ex)
          {
            _logger.LogError($"Error al actualizar la cantidad en el carrito: {ex.Message}", ex);
            TempData["Mensaje"] = "Hubo un error al intentar actualizar el carrito. Por favor, intente de nuevo más tarde.";
            return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
          }
        }

        // If item doesn't exist in cart, continue with creation
        ItemCarrito itemCarrito = new ItemCarrito
        {
          Cantidad = cantidad,
          Producto = producto,
          UserName = userID,
          Precio = producto.PrecioOriginal, 
          Status = "PENDIENTE"
        };
        
        try
        {
          _context.Add(itemCarrito);
          await _context.SaveChangesAsync();

          TempData["Mensaje"] = "Producto agregado al carrito correctamente";
          _logger.LogInformation("Se agregó un producto al carrito");

          return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
        }
        catch (Exception ex)
        {
          _logger.LogError($"Error al guardar en la base de datos al agregar el producto al carrito: {ex.Message}", ex);
          TempData["Mensaje"] = "Hubo un error al intentar agregar el producto al carrito. Por favor, intente de nuevo más tarde.";
          
          return RedirectToAction("Detalles", "Catalogo", new { id = productoId });
        }
      }
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int? idProductoEliminar)
    {
      if (idProductoEliminar == null)
      {
        _logger.LogWarning("Intento de eliminar con id nulo");
        return NotFound();
      }

      var item = await _context.ItemsCarrito.FindAsync(idProductoEliminar);

      if (item != null)
      {
        _context.ItemsCarrito.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Producto eliminado del carrito correctamente.";
        _logger.LogInformation($"Se eliminó el item con ID {idProductoEliminar} del carrito.");
      }
      else
      {
        _logger.LogWarning($"No se encontró item con ID {idProductoEliminar} para eliminar.");
        TempData["Mensaje"] = "No se encontró el producto en el carrito.";
      }

      return RedirectToAction("Carrito");
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}
