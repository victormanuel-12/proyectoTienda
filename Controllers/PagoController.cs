using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyectoTienda.Data;
using proyectoTienda.Models.Model;
using proyectoTienda.Models.Model.ubicacion;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;
using proyectoTienda.Session;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using proyectoTienda.Models;
using proyectoTienda.Servicios;
using proyectoTienda.Models.DTO;
using System.Dynamic;
using proyectoTienda.Models.ViewModels;


namespace proyectoTienda.Controllers
{
  [Authorize(Roles = "USER")]
  public class PagoController : Controller
  {
    private readonly ILogger<PagoController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICarritoService _carritoService;

    public PagoController(ILogger<PagoController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, ICarritoService carritoService)
    {
      _logger = logger;
      _context = context;
      _userManager = userManager;
      _carritoService = carritoService;
    }

    [HttpGet]
    public async Task<IActionResult> Pago()
    {

      var userID = _userManager.GetUserName(User);
      var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userID);
      var elementos = await _context.ItemsCarrito
          .Include(o => o.Producto)
          .Where(o => o.UserName == userID && o.Status == "PENDIENTE")
          .ToListAsync();

      decimal montoOriginal = elementos.Sum(e => e.Producto.PrecioOriginal * e.Cantidad);
      decimal montoActual = elementos.Sum(e => e.Producto.PrecioActual * e.Cantidad);
      var elemento = await _context.ItemsCarrito
        .Where(o => o.UserName == userID && o.Status == "PENDIENTE")
        .CountAsync(); // Contar los registros

      var direccionEnSesion = SessionExtension.Get<Direccion>(HttpContext.Session, "direccionPedido");
      if (direccionEnSesion == null)
      {
        _logger.LogWarning("No se encontró dirección en la sesión para el pedido de pago de {0}", userID);
        return Json(new { success = false, message = "No se encontró dirección en la sesión" });
      }
      else
      {
        _logger.LogWarning("se recupero y este es hjasjasjasjasjajsajsjasajs");
      }

      var model = new PagoResumenViewModel
      {
        Pago = new PagoDTO(),
        montoOriginal = montoOriginal,
        elementosCarrito = elemento,
        montoActual = montoActual

      };
      // Agregar los otros atributos

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> PagoHecho([FromBody] PagoDTO pago)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          var errores = ModelState.Values
              .SelectMany(v => v.Errors)
              .Select(e => e.ErrorMessage)
              .ToList();

          _logger.LogWarning("Errores de validación: {@Errores}", errores);
          return Json(new
          {
            success = false,
            message = "Por favor corrija los errores en el formulario",
            errors = errores
          });
        }
        var direccionEnSesion = SessionExtension.Get<Direccion>(HttpContext.Session, "direccionPedido");
        if (direccionEnSesion == null)
        {
          _logger.LogWarning("No se encontró dirección en la sesión para el pedido");
          return Json(new { success = false, message = "No se encontró dirección en la sesión" });
        }
        if (direccionEnSesion != null)
        {
          var direccionExistente = await _context.Direcciones
              .FirstOrDefaultAsync(d => d.IdDireccion == direccionEnSesion.IdDireccion);
          _logger.LogInformation("Dirección mostrada solo si hay en la base de datos se muestra: {0}", direccionExistente);
          if (direccionExistente == null)
          {
            _context.Direcciones.Add(direccionEnSesion);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Dirección guardada en la base de datos: {0}", JsonSerializer.Serialize(direccionEnSesion));
          }
        }

        // Obtener el ID del usuario actual
        var userID = _userManager.GetUserName(User);
        var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userID);
        var item = await _carritoService.ObtenerCarritoActual(userID);
        _logger.LogInformation("Usuario encontrado: {0}", usuarioExistente != null ? usuarioExistente.Email : "No encontrado");
        if (usuarioExistente == null)
        {
          _logger.LogWarning("Usuario no encontrado");
          return Json(new { success = false, message = "Usuario no encontrado" });
        }

        // Crear el pedido
        var pedido = new Pedido
        {
          IDPedido = Guid.NewGuid(),
          IDCliente = usuarioExistente.ID,
          FechaPedido = DateTime.Now,
          Estado = "PAGADO", // Estado del pedido
          IdDireccion = direccionEnSesion?.IdDireccion ?? 0, // Asignar la dirección de la sesión
          Direccion = direccionEnSesion,
          Total = item.montoActual, // Total del pedido (puedes calcularlo si es necesario)
          TipoEntrega = "DELIVERY" // Ajustar según tus necesidades
        };

        // Crear el pago
        var nuevoPago = new Pago
        {

          IDPedido = pedido.IDPedido,
          NumeroTarjeta = pago.NumeroTarjeta,
          MesVencimiento = pago.MesVencimiento,
          AnioVencimiento = pago.AnioVencimiento,
          CodigoSeguridad = pago.CodigoSeguridad,
          NombreTarjeta = pago.NombreTarjeta,
          TipoDocumento = pago.TipoDocumento,
          NumeroDocumento = pago.NumeroDocumento,
          Telefono = pago.Telefono,
          FechaPago = DateTime.Now,
          Monto = item.montoActual,
          MetodoPago = "Tarjeta" // 0=Tarjeta, 1=PayPal, 2=Transferencia
        };

        // Asociar el pago al pedido
        pedido.Pago = nuevoPago;

        // Guardar el pedido y el pago en la base de datos
        _context.Pedidos.Add(pedido);
        _context.Pagos.Add(nuevoPago);

        var itemsCarrito = item.elementosCarrito;
        decimal total = 0M;

        // Crear los detalles del pedido basados en los items del carrito
        foreach (var items in itemsCarrito)
        {
          var detalle = new DetallePedido
          {
            IDPedido = pedido.IDPedido,
            IDProducto = items.Producto.IDProducto,
            Cantidad = items.Cantidad,
            Subtotal = items.Cantidad * items.Producto.PrecioActual
          };

          total += detalle.Subtotal;

          // Verificar si el producto existe y actualizar su stock
          var producto = await _context.Productos.FindAsync(items.Producto.IDProducto);
          if (producto != null)
          {
            producto.Stock -= items.Cantidad;
            if (producto.Stock < 0)
            {
              producto.Stock = 0; // Evitar stocks negativos
            }
            _context.Productos.Update(producto);
          }

          // Agregar el detalle a la base de datos
          _context.DetallesPedidos.Add(detalle);

          // Actualizar el estado del item en el carrito
          items.Status = "PAGADO";
          _context.ItemsCarrito.Update(items);
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pedido y pago creados correctamente - ID Pedido: {IDPedido}, ID Pago: {IDPago}", pedido.IDPedido, nuevoPago.IDPago);

        return Json(new
        {
          success = true,
          message = "Pago procesado exitosamente",
          pedidoId = pedido.IDPedido
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al procesar el pago");
        return Json(new
        {
          success = false,
          message = "Error interno al procesar el pago"
        });
      }
    }
    public IActionResult confirmacion()
    {
      var pedidoEnSesion = SessionExtension.Get<Pedido>(HttpContext.Session, "Pedido");
      _logger.LogInformation("Valor pedido en sesión: {0}", pedidoEnSesion);
      var direccionEnSesion = SessionExtension.Get<Direccion>(HttpContext.Session, "direccionPedido");
      if (direccionEnSesion != null)
      {
        _logger.LogInformation("Dirección del pedido: {0}", JsonSerializer.Serialize(direccionEnSesion));
      }
      else
      {
        _logger.LogWarning("No se encontró dirección en la sesión para el pedido");
      }
      return View();

    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}