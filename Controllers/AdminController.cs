using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyectoTienda.Data;
using proyectoTienda.Models;
using proyectoTienda.ViewModel;

namespace proyectoTienda.Controllers
{
  [Authorize(Roles = "ADMIN")]
  public class AdminController : Controller
  {
    private readonly ILogger<AdminController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    // Constructor con la inyección de dependencias para el DbContext
    public AdminController(ILogger<AdminController> logger, ApplicationDbContext context, IConfiguration configuration)
    {
      _logger = logger;
      _context = context;
      _configuration = configuration;
    }

    public IActionResult HomeAdmin()
    {
      return View();
    }
    // Categoria
    [HttpGet]
    public async Task<IActionResult> Categoria()
    {

      var categorias = await _context.Categorias
.Select(c => new CategoriaViewModel
{
  Categoria = c,
  ProductosCount = c.Productos.Count()
})
.ToListAsync();

      // Verificar si no hay categorías
      if (categorias == null || categorias.Count == 0)
      {
        ViewBag.Message = "No hay categorías disponibles en el sistema.";
      }

      return View(categorias);
    }




    // Agregar Categoria
    public IActionResult AgregarCategoria()
    {
      var categoria = new Categoria(); // Asegúrate de inicializar el modelo
      return View(categoria);
    }


    [HttpPost]
    public async Task<IActionResult> AgregarCategoria(Categoria nuevaCategoria)
    {
      if (ModelState.IsValid)
      {
        // Validar que el nombre no exista ya (ignorando mayúsculas/minúsculas)
        bool existeCategoria = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == nuevaCategoria.Nombre.ToLower());

        if (existeCategoria)
        {
          ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
          return View(nuevaCategoria); // Retorna la vista con los datos ingresados
        }

        try
        {
          nuevaCategoria.FechaCreacion = DateTime.Now; // Asignar la fecha de creación
          _context.Categorias.Add(nuevaCategoria);
          await _context.SaveChangesAsync();
          return RedirectToAction("Categoria"); // o el nombre real de tu acción/lista
        }
        catch (Exception ex)
        {
          // Puedes tener una vista Error.cshtml que reciba un modelo personalizado si quieres
          return View("Error", new { message = ex.Message });
        }
      }

      return View(nuevaCategoria); // Retorna la vista con errores si ModelState es inválido
    }


    [HttpPost]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
      try
      {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
          return NotFound();
        }

        // Eliminar la categoría
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        // Redirigir a la vista de categorías después de eliminar
        return RedirectToAction("Categoria");
      }
      catch (Exception ex)
      {
        // En caso de error, puedes manejarlo como desees
        return View("Error", new { message = ex.Message });
      }
    }


    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id)
    {
      // Buscar la categoría en la base de datos
      var categoria = await _context.Categorias
                                    .FirstOrDefaultAsync(c => c.IDCategoria == id);

      // Si no se encuentra la categoría, devolver una vista de error o redirigir a otro lugar
      if (categoria == null)
      {
        return NotFound();
      }

      // Crear el ViewModel para pasar los datos al formulario
      var viewModel = new EditarCategoriaViewModel
      {
        Id = categoria.IDCategoria,
        Nombre = categoria.Nombre,
        Descripcion = categoria.Descripcion
      };

      return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(EditarCategoriaViewModel model)
    {
      if (ModelState.IsValid)
      {
        // Buscar la categoría por ID
        var categoria = await _context.Categorias.FindAsync(model.Id);

        if (categoria == null)
        {
          return NotFound();
        }

        // Actualizar los valores de la categoría
        categoria.Nombre = model.Nombre;
        categoria.Descripcion = model.Descripcion;

        // Guardar los cambios en la base de datos
        _context.Update(categoria);
        await _context.SaveChangesAsync();

        // Redirigir a la página de categorías
        return RedirectToAction("Categoria");
      }

      // Si el modelo no es válido, devolver la vista con el formulario
      return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Productos(int page = 1)
    {
      int pageSize = 7; // Tamaño de página: 5 productos por página

      // Obtenemos todos los productos ordenados por categoría
      // Primero los de categoría ID=1, luego las demás categorías en orden ascendente
      var productos = _context.Productos.Include(p => p.Categoria)
          .OrderBy(p => p.IDCategoria == 1 ? 0 : 1) // Prioriza categoría ID=1
          .ThenBy(p => p.IDCategoria)               // Luego ordena por las demás categorías
          .ThenBy(p => p.Nombre);                   // Orden secundario por nombre

      // Verificamos la cantidad total para depuración
      int totalProductos = await productos.CountAsync();
      ViewBag.TotalProductosEnBD = totalProductos;

      // Crear el modelo con paginación
      var model = await ProductoPaginado<Producto>.CreateAsync(
          productos,
          page,
          pageSize);

      return View(model);
    }



    [HttpGet]
    public IActionResult AgregarProducto()
    {
      var producto = new Producto();
      var categorias = _context.Categorias.ToList(); // Asegúrate de obtener todas las categorías

      var viewModel = new ProductoCategoriaViewModel
      {
        Producto = producto,
        Categorias = categorias  // Pasa la lista de categorías
      };

      return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AgregaProducto(ProductoCategoriaViewModel viewModel)
    {
      try
      {
        var imagen = Request.Form.Files.FirstOrDefault(f => f.Name == "Producto.ImagenURL");
        if (imagen == null || imagen.Length == 0)
        {
          TempData["ErrorMessage"] = "No se seleccionó ninguna imagen";
          return RedirectToAction("AgregarProducto");
        }

        // Verificar el tamaño de la imagen (máximo 5MB para GitHub)
        if (imagen.Length > 5 * 1024 * 1024)
        {
          TempData["ErrorMessage"] = "La imagen supera el límite de 5MB permitido";
          return RedirectToAction("AgregarProducto");
        }

        using var ms = new MemoryStream();
        await imagen.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var base64Image = Convert.ToBase64String(bytes);

        string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);

        // Leer token y repo desde appsettings.json
        var gitHubToken = _configuration["GitHub:Token"];
        var repo = _configuration["GitHub:Repo"];

        if (string.IsNullOrEmpty(gitHubToken) || string.IsNullOrEmpty(repo))
        {
          _logger.LogError("Faltan configuraciones para GitHub Token o Repo");
          TempData["ErrorMessage"] = "Error en la configuración de GitHub";
          return RedirectToAction("AgregarProducto");
        }

        var url = $"https://api.github.com/repos/{repo}/contents/images/{nombreArchivo}";

        var payload = new
        {
          message = "Subida de imagen desde formulario",
          content = base64Image
        };

        using var httpClient = new HttpClient();
        // Ajuste del User-Agent (GitHub requiere un User-Agent válido)
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("proyectoTienda/1.0");
        // Usando Bearer token en lugar de "token"
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gitHubToken);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
          var responseContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Error al subir a GitHub: {response.StatusCode}, {responseContent}");
          TempData["ErrorMessage"] = $"Error al subir la imagen: {response.StatusCode}";
          return RedirectToAction("AgregarProducto");
        }

        // Construir URL pública de la imagen
        var imagenUrl = $"https://raw.githubusercontent.com/{repo}/main/images/{nombreArchivo}";

        // Buscar si ya existe un producto con ese ID
        var productoExistente = await _context.Productos
            .FirstOrDefaultAsync(p => p.IDProducto == viewModel.Producto.IDProducto);

        // Buscar la categoría
        var categoriaSeleccionada = await _context.Categorias
            .FirstOrDefaultAsync(c => c.IDCategoria == viewModel.Producto.IDCategoria);

        // Guardar en la BD
        if (productoExistente != null)
        {
          // Actualizar los campos del producto existente
          productoExistente.Nombre = viewModel.Producto.Nombre;
          productoExistente.Descripcion = viewModel.Producto.Descripcion;
          productoExistente.PrecioActual = viewModel.Producto.PrecioActual;
          productoExistente.Stock = viewModel.Producto.Stock;
          productoExistente.IDCategoria = viewModel.Producto.IDCategoria;
          productoExistente.Categoria = categoriaSeleccionada;
          productoExistente.ImagenURL = imagenUrl; // Actualizar URL de imagen

          _context.Productos.Update(productoExistente);
        }
        else
        {
          // Asignar categoría al nuevo producto
          viewModel.Producto.Categoria = categoriaSeleccionada;
          viewModel.Producto.ImagenURL = imagenUrl; // Asignar la URL de la imagen al nuevo producto

          // Agregar nuevo producto
          _context.Productos.Add(viewModel.Producto);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Producto guardado correctamente";
        return RedirectToAction("Productos");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al procesar la solicitud");
        TempData["ErrorMessage"] = "Ocurrió un error al procesar la solicitud: " + ex.Message;
        return RedirectToAction("AgregarProducto");
      }
    }


    [HttpGet]
    public IActionResult Edit(int id)
    {
      // Buscar el producto en la base de datos
      var producto = _context.Productos.Include(p => p.Categoria).FirstOrDefault(p => p.IDProducto == id);
      if (producto == null)
      {
        return NotFound();
      }

      // Crear el ViewModel para pasar los datos al formulario
      var viewModel = new ProductoCategoriaViewModel
      {
        Producto = producto,
        Categorias = _context.Categorias.ToList() // Asegúrate de obtener todas las categorías
      };

      return View("AgregarProducto", viewModel);

    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProducto(int id)
    {
      _logger.LogInformation("Intentando eliminar producto con ID: {ProductId}", id);

      var producto = await _context.Productos.FindAsync(id);
      if (producto == null)
      {
        return NotFound();
      }

      try
      {

        var detallesPedidosRelacionados = _context.DetallesPedidos.Where(d => d.IDProducto == id);
        _context.DetallesPedidos.RemoveRange(detallesPedidosRelacionados);


        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Producto eliminado correctamente.";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al eliminar el producto con ID {ProductId}", id);
        TempData["ErrorMessage"] = "No se pudo eliminar el producto. Es posible que tenga referencias en otros registros.";
      }

      return RedirectToAction("Productos");
    }











    // Lista de Pedidos
    [HttpGet]
    public async Task<IActionResult> ListaOrdenes()
    {
      try
      {
        var ordenes = await _context.Pedidos.ToListAsync();
        return View(ordenes);
      }
      catch (Exception ex)
      {
        return View("Error", new { message = ex.Message });
      }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}