
using Microsoft.AspNetCore.Mvc;
using proyectoTienda.Models;
using proyectoTienda.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
namespace proyectoTienda.Controllers
{

  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
      _logger = logger;
      _context = context;
    }

    public IActionResult Index()
    {
      // Consulta los últimos 4 productos de categoría Jeans (2) o Polos (1)
      var productosRecientes = _context.Productos
          .Where(p => p.IDCategoria == 1 || p.IDCategoria == 2)
          .OrderByDescending(p => p.IDProducto)
          .Take(4)
          .ToList();

      return View(productosRecientes);
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
