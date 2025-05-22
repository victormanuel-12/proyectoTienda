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



using MercadoPago.Config;
using MercadoPago.Client.Common;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;


namespace proyectoTienda.Controllers
{
  [Authorize(Roles = "USER")]
  public class PagoController : Controller
  {
    private readonly ILogger<PagoController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICarritoService _carritoService;
    private readonly IConfiguration _configuration;

    public PagoController(ILogger<PagoController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, ICarritoService carritoService, IConfiguration configuration)
    {
      _logger = logger;
      _context = context;
      _userManager = userManager;
      _carritoService = carritoService;
      _configuration = configuration;
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


    /*[HttpPost]
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
    }*/



    // NUEVA ACCIÓN PARA INICIAR PAGO CON MERCADO PAGO
    [HttpPost]
    //[ValidateAntiForgeryToken] // Mantenlo comentado mientras depuras
    public async Task<IActionResult> PagarConMercadoPago()
    {
        try
        {
            _logger.LogInformation("DEBUG: DENTRO DEL TRY de PagarConMercadoPago..."); 
            var userID = _userManager.GetUserName(User);
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userID);
            
            if (usuarioExistente == null)
            {
                _logger.LogWarning("Usuario no encontrado al intentar pagar con Mercado Pago.");
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Pago));
            }

            var carritoDynamic = await _carritoService.ObtenerCarritoActual(userID); 

            if (carritoDynamic == null)
            {
                _logger.LogWarning("El servicio de carrito devolvió null para el usuario {UserID}.", userID);
                TempData["ErrorMessage"] = "No se pudo obtener tu carrito de compras.";
                return RedirectToAction("Index", "Carrito"); 
            }

            var elementosDelCarrito = (List<proyectoTienda.Models.Model.ItemCarrito>)carritoDynamic.elementosCarrito;

            if (elementosDelCarrito == null || !elementosDelCarrito.Any()) 
            {
                _logger.LogWarning("Carrito vacío al intentar pagar con Mercado Pago para el usuario {UserID}.", userID);
                TempData["ErrorMessage"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            var direccionEnSesion = SessionExtension.Get<Direccion>(HttpContext.Session, "direccionPedido");
            if (direccionEnSesion == null)
            {
                _logger.LogWarning("No se encontró dirección en la sesión para Mercado Pago. Usuario: {UserID}", userID);
                TempData["ErrorMessage"] = "No se encontró dirección de envío.";
                return RedirectToAction(nameof(Pago));
            }

            string currentAccessToken = _configuration.GetValue<string>("MercadoPago:AccessToken");
            _logger.LogInformation("Usando AccessToken para MP: {Token}", currentAccessToken);
            MercadoPagoConfig.AccessToken = currentAccessToken;

            if (string.IsNullOrEmpty(MercadoPagoConfig.AccessToken))
            {
                 _logger.LogError("AccessToken de MercadoPago NO está configurado en appsettings.json o es nulo/vacío.");
                 TempData["ErrorMessage"] = "Error de configuración del sistema de pagos (Token MP no encontrado). Por favor, contacte a soporte.";
                 return RedirectToAction(nameof(Pago));
            }

            var preferenceItems = new List<PreferenceItemRequest>();
            foreach (var itemCarrito in elementosDelCarrito) 
            {
                if (itemCarrito.Producto == null)
                {
                    _logger.LogWarning("ItemCarrito con ID {IDItem} tiene Producto null. Se omitirá.", itemCarrito.IDItem);
                    continue; 
                }

                preferenceItems.Add(new PreferenceItemRequest
                {
                    Id = itemCarrito.Producto.IDProducto.ToString(),
                    Title = !string.IsNullOrEmpty(itemCarrito.Producto.Nombre) ? itemCarrito.Producto.Nombre : "Producto sin nombre",
                    Quantity = itemCarrito.Cantidad > 0 ? itemCarrito.Cantidad : 1, 
                    CurrencyId = "PEN", 
                    UnitPrice = itemCarrito.Producto.PrecioActual > 0 ? itemCarrito.Producto.PrecioActual : 0.01m, 
                });
            }

            if (!preferenceItems.Any())
            {
                 _logger.LogWarning("No hay ítems válidos para enviar a Mercado Pago después de procesar el carrito para el usuario {UserID}.", userID);
                 TempData["ErrorMessage"] = "No hay productos válidos en tu carrito para procesar el pago.";
                 return RedirectToAction("Index", "Carrito");
            }

            decimal costoEnvio = 12.90m; 
            if (costoEnvio > 0)
            {
                preferenceItems.Add(new PreferenceItemRequest
                {
                    Id = "ENVIO",
                    Title = "Costo de Envío",
                    Quantity = 1,
                    CurrencyId = "PEN",
                    UnitPrice = costoEnvio,
                });
            }

            var idPedidoTemporal = Guid.NewGuid();
            SessionExtension.Set(HttpContext.Session, "IdPedidoTemporalMP", idPedidoTemporal); 

            var emailDeTuCompradorDePruebaMP = "test_user_1845902594@testuser.com"; // Reemplaza con el email real de tu comprador de prueba MP

            var request = new PreferenceRequest
            {
                Items = preferenceItems,
                 Payer = new PreferencePayerRequest // COMENTA O ELIMINA ESTE BLOQUE COMPLETAMENTE
                 {
                    Email = usuarioExistente.Email, // Email del comprador
                    Name = usuarioExistente.Nombre,          
                //   Surname = "Pedroso",                   
                //   Identification = new IdentificationRequest 
                //   {
                //     Type = "DNI", 
                //     Number = "12345678" 
                //   },
                //   Phone = new PhoneRequest                 
                //   {
                //     Number = "987654321" 
                //   },
                //   Address = new AddressRequest              
                //   {
                //     StreetName = "Jirón de la Unión",
                //     StreetNumber = "1234",
                //     ZipCode = "15001",                       
                //   }
                 },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{Request.Scheme}://{Request.Host}/Pago/MercadoPagoSuccess",
                    Failure = $"{Request.Scheme}://{Request.Host}/Pago/MercadoPagoFailure",
                    Pending = $"{Request.Scheme}://{Request.Host}/Pago/MercadoPagoPending"
                },
                AutoReturn = "approved",
                ExternalReference = idPedidoTemporal.ToString(),
                //NotificationUrl = $"{Request.Scheme}://{Request.Host}/Pago/MercadoPagoNotification", 
                StatementDescriptor = "TuTiendaOnline",
            };

            _logger.LogInformation("PAYER MINIMALISTA - Objeto PreferenceRequest ANTES DE ENVIAR A MP: {PreferenceRequestJson}", JsonSerializer.Serialize(request));
            
            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request); 

            if (preference == null || string.IsNullOrEmpty(preference.InitPoint)) 
            {
                _logger.LogError("Error al crear la preferencia de Mercado Pago: SandboxInitPoint es nulo o vacío después de la llamada a CreateAsync.");
                TempData["ErrorMessage"] = "No se pudo generar el enlace de pago con Mercado Pago (SandboxInitPoint nulo). Inténtalo de nuevo o contacta a soporte.";
                return RedirectToAction(nameof(Pago));
            }

            SessionExtension.Set(HttpContext.Session, "MercadoPagoPreferenceId", preference.Id);

            _logger.LogInformation("Preferencia de Mercado Pago creada: {PreferenceId}, Redirigiendo a: {SandboxInitPoint}", preference.Id, preference.SandboxInitPoint);
            return Redirect(preference.InitPoint); 
        }
        catch (MercadoPago.Error.MercadoPagoApiException mpEx) // CAMBIADO AL NAMESPACE DEL LOG ANTERIOR
        {
             _logger.LogError(mpEx, "API ERROR MP: Status: {Status}, API Message: {ApiMessage}, Full Exception: {FullEx}", 
                mpEx.StatusCode,    
                mpEx.Message,       
                mpEx.ToString());   

            TempData["ErrorMessage"] = $"Error de Mercado Pago: {mpEx.Message}. Por favor, revisa los datos e inténtalo de nuevo.";
            return RedirectToAction(nameof(Pago));
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "GENERAL ERROR: Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace); 
            TempData["ErrorMessage"] = "Ocurrió un error al intentar procesar el pago con Mercado Pago. Por favor, inténtalo de nuevo.";
            return RedirectToAction(nameof(Pago));
        }
    }




// ACCIÓN PARA CUANDO MERCADO PAGO REDIRIGE POR PAGO EXITOSO
  [HttpGet]
  public async Task<IActionResult> MercadoPagoSuccess(
      [FromQuery] string collection_id,
      [FromQuery] string collection_status,
      [FromQuery] string payment_id,
      [FromQuery] string status,
      [FromQuery] string external_reference,
      [FromQuery] string payment_type,
      [FromQuery] string merchant_order_id,
      [FromQuery] string preference_id,
      [FromQuery] string site_id,
      [FromQuery] string processing_mode,
      [FromQuery] string merchant_account_id)
  {
      _logger.LogInformation(
          "Retorno exitoso de Mercado Pago. PaymentId: {PaymentId}, ExternalReference: {ExternalReference}, Status: {Status}",
          payment_id, external_reference, status);

      var idPedidoTemporalGuardado = SessionExtension.Get<Guid?>(HttpContext.Session, "IdPedidoTemporalMP");

      if (string.IsNullOrEmpty(external_reference) || !Guid.TryParse(external_reference, out Guid refGuid) ||
          idPedidoTemporalGuardado == null || refGuid != idPedidoTemporalGuardado.Value)
      {
          _logger.LogWarning(
              "Referencia externa inválida o no coincide con la sesión en MercadoPagoSuccess. Recibido: {ExternalReference}, Esperado: {IdPedidoTemporalGuardado}",
              external_reference, idPedidoTemporalGuardado);
          TempData["ErrorMessage"] = "Hubo un problema al verificar la referencia de tu pago. Contacta a soporte.";
          return RedirectToAction("Index", "Home");
      }

      if (status == "approved" && collection_status == "approved")
      {
          try
          {
              var userID = _userManager.GetUserName(User);
              var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == userID);
              var carritoDynamic = await _carritoService.ObtenerCarritoActual(userID); // carritoDynamic es 'dynamic'
              var direccionEnSesion = SessionExtension.Get<Direccion>(HttpContext.Session, "direccionPedido");

              // --- INICIO DE LA CORRECCIÓN para RuntimeBinderException ---
              List<proyectoTienda.Models.Model.ItemCarrito> elementosDelCarritoEnSuccess = null;
              if (carritoDynamic != null && carritoDynamic.elementosCarrito != null)
              {
                  // Casteamos aquí una vez que sabemos que no es null
                  elementosDelCarritoEnSuccess = (List<proyectoTienda.Models.Model.ItemCarrito>)carritoDynamic.elementosCarrito;
              }

              // Ahora validamos con la lista casteada
              if (usuarioExistente == null || carritoDynamic == null || elementosDelCarritoEnSuccess == null || !elementosDelCarritoEnSuccess.Any() || direccionEnSesion == null)
              {
                  TempData["ErrorMessage"] = "Error al procesar tu pedido después del pago (datos críticos o carrito vacío). Por favor, contacta a soporte con la referencia: " + external_reference;
                  return RedirectToAction("Index", "Home");
              }
              // --- FIN DE LA CORRECCIÓN ---

              // Manejo de Dirección
              var direccionExistente = await _context.Direcciones.FirstOrDefaultAsync(d => d.IdDireccion == direccionEnSesion.IdDireccion);
              if (direccionExistente == null && direccionEnSesion.IdDireccion == 0) // Solo agregar si es realmente nueva (ID 0) y no existe
              {
                  _context.Direcciones.Add(direccionEnSesion);
                  // EF Core se encargará de asignar el ID al guardar el pedido si la relación está bien
              }
              // Si direccionEnSesion.IdDireccion > 0 y direccionExistente es null, podría ser un problema de datos o una dirección eliminada.
              // Aquí asumimos que si IdDireccion > 0, ya está en la BD o se manejará como una referencia existente.

              // Calcular el total del pedido basado en los elementos del carrito recuperados
              // Es importante usar la misma fuente de datos (elementosDelCarritoEnSuccess) para consistencia.
              decimal montoTotalCarrito = 0;
              foreach (var item in elementosDelCarritoEnSuccess)
              {
                  if (item.Producto != null)
                  {
                      montoTotalCarrito += item.Producto.PrecioActual * item.Cantidad;
                  }
              }
              decimal costoEnvio = 12.90m; // Asegúrate que este sea el costo de envío correcto
              decimal totalDelPedido = montoTotalCarrito + costoEnvio;


              var pedido = new Pedido
              {
                  IDPedido = refGuid,
                  IDCliente = usuarioExistente.ID,
                  FechaPedido = DateTime.Now,
                  Estado = "PAGADO_MP",
                  IdDireccion = direccionEnSesion.IdDireccion, // Se actualizará si es nueva y se guarda
                  Direccion = direccionEnSesion,
                  Total = totalDelPedido, // Usar el total calculado consistentemente
                  TipoEntrega = "DELIVERY"
              };

              var nuevoPago = new Pago
              {
                IDPedido = pedido.IDPedido,
                FechaPago = DateTime.Now,
                Monto = pedido.Total,
                MetodoPago = "MercadoPago",
                NumeroTarjeta = !string.IsNullOrEmpty(payment_id) ? $"MP-{payment_id.Substring(0, Math.Min(10, payment_id.Length))}" : "MercadoPago-IDNoProvisto",
                NombreTarjeta = "Pago vía MercadoPago",
                NumeroDocumento = "12345678", // O algún otro placeholder si es string
                TipoDocumento = "DNI",   // Si también es NOT NULL
                Telefono = "987654321", 
              };
              // Si tienes una propiedad de navegación Pedido.Pago y está configurada para cascada,
              // EF Core podría manejar la inserción de 'nuevoPago' al agregar 'pedido'.
              // Si no, o para ser explícito:
              _context.Pagos.Add(nuevoPago); // Asegúrate de que la entidad Pago esté correctamente configurada para ser agregada así
              pedido.Pago = nuevoPago; // Asigna la propiedad de navegación si existe y es relevante


              _context.Pedidos.Add(pedido);

              // Usar la lista con tipo 'elementosDelCarritoEnSuccess' para crear detalles y actualizar stock/carrito
              foreach (var item in elementosDelCarritoEnSuccess)
              {
                  // 'item' aquí es de tipo ItemCarrito
                  if (item.Producto == null) continue;

                  var detalle = new DetallePedido
                  {
                      IDPedido = pedido.IDPedido,
                      IDProducto = item.Producto.IDProducto,
                      // Producto = item.Producto, // No necesitas asignar la entidad de navegación aquí si IDProducto es la FK
                      Cantidad = item.Cantidad,
                      Subtotal = item.Cantidad * item.Producto.PrecioActual
                  };
                  _context.DetallesPedidos.Add(detalle);

                  var productoEnDb = await _context.Productos.FindAsync(item.Producto.IDProducto);
                  if (productoEnDb != null)
                  {
                      productoEnDb.Stock -= item.Cantidad;
                      if (productoEnDb.Stock < 0) productoEnDb.Stock = 0;
                      _context.Productos.Update(productoEnDb);
                  }

                  // 'item' es la entidad ItemCarrito rastreada si _carritoService.ObtenerCarritoActual los devuelve así.
                  // Si no, necesitarías buscar el ItemCarrito en la BD para actualizarlo.
                  // Asumiendo que 'item' es la entidad rastreada:
                  item.Status = "PAGADO";
                  _context.ItemsCarrito.Update(item);
              }

              await _context.SaveChangesAsync();

              _logger.LogInformation(
                  "Pedido {IDPedido} creado y procesado exitosamente tras pago con Mercado Pago {PaymentId}.",
                  pedido.IDPedido, payment_id);

              HttpContext.Session.Remove("IdPedidoTemporalMP");
              HttpContext.Session.Remove("MercadoPagoPreferenceId");

              return RedirectToAction(nameof(confirmacion), new { pedidoId = pedido.IDPedido });
          }
          catch (DbUpdateException dbEx)
          {
              _logger.LogError(dbEx, "Error de BD al procesar pedido después de pago exitoso con MP. ExternalReference: {ExternalReference}. InnerException: {InnerEx}", external_reference, dbEx.InnerException?.Message);
              TempData["ErrorMessage"] = "Hubo un error crítico al registrar tu pedido (DB). Por favor, contacta a soporte con la referencia: " + external_reference;
              return RedirectToAction("Index", "Home");
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error general al procesar pedido después de pago exitoso con Mercado Pago. ExternalReference: {ExternalReference}", external_reference);
              TempData["ErrorMessage"] = "Hubo un error al registrar tu pedido después del pago. Por favor, contacta a soporte con la referencia: " + external_reference;
              return RedirectToAction("Index", "Home");
          }
      }
      else
      {
          _logger.LogWarning(
              "Retorno de Mercado Pago con estado no aprobado. PaymentId: {PaymentId}, Status: {Status}, CollectionStatus: {CollectionStatus}",
              payment_id, status, collection_status);
          TempData["ErrorMessage"] = $"El pago no fue aprobado ({status}). Inténtalo de nuevo o prueba con otro método.";
          return RedirectToAction(nameof(Pago));
      }
  }




    // ACCIÓN PARA CUANDO MERCADO PAGO REDIRIGE POR PAGO FALLIDO
    [HttpGet]
    public IActionResult MercadoPagoFailure(
        [FromQuery] string collection_id,
        [FromQuery] string collection_status,
        [FromQuery] string payment_id,
        [FromQuery] string status,
        [FromQuery] string external_reference,
        [FromQuery] string payment_type,
        [FromQuery] string merchant_order_id,
        [FromQuery] string preference_id,
        [FromQuery] string site_id,
        [FromQuery] string processing_mode,
        [FromQuery] string merchant_account_id)
    {
        _logger.LogWarning("Retorno fallido de Mercado Pago. PaymentId: {PaymentId}, ExternalReference: {ExternalReference}, Status: {Status}", payment_id, external_reference, status);
        TempData["ErrorMessage"] = "El pago con Mercado Pago falló o fue cancelado. Por favor, inténtalo de nuevo.";

        HttpContext.Session.Remove("IdPedidoTemporalMP");
        HttpContext.Session.Remove("MercadoPagoPreferenceId");

        return RedirectToAction(nameof(Pago));
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




    /*[HttpGet]
    public async Task<IActionResult> confirmacion(Guid? pedidoId)
    {
      if (pedidoId == null)
      {

          ViewBag.MensajeConfirmacion = "Tu pedido ha sido procesado.";
          return View();
      }

      var pedido = await _context.Pedidos
                            .Include(p => p.Direccion)
                            .Include(p => p.Pago)
                            .FirstOrDefaultAsync(p => p.IDPedido == pedidoId);

      if (pedido == null)
      {
          _logger.LogWarning("No se encontró el pedido {PedidoId} para la página de confirmación.", pedidoId);
          TempData["ErrorMessage"] = "No se pudo encontrar tu pedido.";
          return RedirectToAction("Index", "Home");
      }

      return View(pedido);
    }*/



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}