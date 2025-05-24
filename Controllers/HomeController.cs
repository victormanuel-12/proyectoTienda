
using Microsoft.AspNetCore.Mvc;
using proyectoTienda.Models;
using proyectoTienda.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using proyectoTienda.Servicios;
using proyectoTienda.Models.ViewModels;

namespace proyectoTienda.Controllers
{

  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly SubstackService _substackService;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
      _logger = logger;
      _context = context;
      _substackService = new SubstackService();
    }

    public async Task<IActionResult> Index()
    {
      var productosRecientes = _context.Productos
            .Where(p => p.IDCategoria == 1 || p.IDCategoria == 2)
            .OrderByDescending(p => p.IDProducto)
            .Take(4)
            .ToList();

        var publicaciones = await _substackService.ObtenerPublicacionesAsync();

        var modelo = new ProductosYPublicacionesViewModel
        {
            ProductosRecientes = productosRecientes,
            Publicaciones = publicaciones
        };

        return View(modelo);
    }


    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
