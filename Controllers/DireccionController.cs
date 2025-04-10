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
using proyectoTienda.ViewModel;
using Microsoft.AspNetCore.Identity;


namespace proyectoTienda.Controllers
{

  public class DireccionController : Controller
  {
    private readonly ILogger<DireccionController> _logger;
    private readonly ApplicationDbContext _context; // Ajusta según tu DbContext
    private readonly UserManager<IdentityUser> _userManager;
    public DireccionController(ILogger<DireccionController> logger, ApplicationDbContext context,
    UserManager<IdentityUser> userManager)
    {
      _logger = logger;
      _context = context;
      _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
      // Cargar los departamentos para el dropdown inicial
      ViewBag.Departamentos = await _context.Departamentos
          .OrderBy(d => d.Nombre)
          .Select(d => new SelectListItem { Value = d.DepartamentoId.ToString(), Text = d.Nombre })
          .ToListAsync();

      return View(new DireccionViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> GetProvincias(int departamentoId)
    {
      var provincias = await _context.Provincias
          .Where(p => p.DepartamentoId == departamentoId)
          .OrderBy(p => p.Nombre)
          .Select(p => new { id = p.ProvinciaId, nombre = p.Nombre })
          .ToListAsync();

      return Json(provincias);
    }

    [HttpGet]
    public async Task<IActionResult> GetDistritos(int provinciaId)
    {
      var distritos = await _context.Distritos
          .Where(d => d.ProvinciaId == provinciaId)
          .OrderBy(d => d.Nombre)
          .Select(d => new { id = d.DistritoId, nombre = d.Nombre })
          .ToListAsync();

      return Json(distritos);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarDireccion(DireccionViewModel model)
    {
      if (ModelState.IsValid)
      {
        // Obtener los nombres de departamento, provincia y distrito
        var departamento = await _context.Departamentos.FindAsync(model.DepartamentoId);
        var provincia = await _context.Provincias.FindAsync(model.ProvinciaId);
        var distrito = await _context.Distritos.FindAsync(model.DistritoId);
        var userID = _userManager.GetUserName(User);
        var usuarioAspNet = await _userManager.FindByNameAsync(userID);
        // Crear la dirección
        // Log model information
        _logger.LogInformation("Guardando dirección para usuario: {UserId}", userID);

        var direccion = new Direccion
        {
          IDCliente = usuarioAspNet.Id, // O el ID del usuario actual
          DireccionTexto = model.DireccionTexto,
          Departamento = departamento.Nombre,
          Complemento = model.Complemento,
          Provincia = provincia.Nombre,
          Distrito = distrito.Nombre
        };
        // Log direccion object details
        _logger.LogInformation("Direccion creada: ID Cliente: {IDCliente}, Dirección: {DireccionTexto}, " +
                     "Departamento: {Departamento}, Provincia: {Provincia}, " +
                     "Distrito: {Distrito}, Complemento: {Complemento}",
                     direccion.IDCliente, direccion.DireccionTexto,
                     direccion.Departamento, direccion.Provincia,
                     direccion.Distrito, direccion.Complemento);

        _context.Direcciones.Add(direccion);
        await _context.SaveChangesAsync();

        return RedirectToAction("Entrega", "Checkout");
      }

      // Si hay errores, volvemos a cargar los departamentos
      ViewBag.Departamentos = await _context.Departamentos
          .OrderBy(d => d.Nombre)
          .Select(d => new SelectListItem { Value = d.DepartamentoId.ToString(), Text = d.Nombre })
          .ToListAsync();

      return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}