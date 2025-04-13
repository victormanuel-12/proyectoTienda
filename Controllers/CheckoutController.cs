using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace proyectoTienda.Controllers
{
  [Authorize(Roles = "USER")]
  public class CheckoutController : Controller
  {
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ILogger<CheckoutController> logger)
    {
      _logger = logger;
    }






    public IActionResult Entrega()
    {
      return View();
    }




    public IActionResult FechaEntrega()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}