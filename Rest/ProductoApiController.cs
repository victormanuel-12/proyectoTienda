using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using proyectoTienda.Data;
using proyectoTienda.Models;
using proyectoTienda.ViewModel;
using proyectoTienda.Models.DTO;

namespace proyectoTienda.Rest
{
    [ApiController]
    [Route("api/producto")]
    public class ProductoApiController : Controller
    {
        private readonly ILogger<ProductoApiController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductoApiController(ILogger<ProductoApiController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodosLosProductos()
        {
            var listaDeProductos = await _context.Productos.ToListAsync();

            if (listaDeProductos == null || !listaDeProductos.Any())
            {
                return NotFound();
            }


            var listaDeProductosDto = listaDeProductos.Select(producto => new ProductoDto
            {
                IDProducto = producto.IDProducto,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioOriginal = producto.PrecioOriginal,
                PrecioActual = producto.PrecioActual,
                Stock = producto.Stock,
                Talla = producto.Talla,
                Detalle = producto.Detalle,
                ImagenURL = producto.ImagenURL,
                IDCategoria = producto.IDCategoria
            }).ToList();

            return Ok(listaDeProductosDto);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductoDto>> Get(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            var productoDto = new ProductoDto
            {
                IDProducto = producto.IDProducto,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioOriginal = producto.PrecioOriginal,
                PrecioActual = producto.PrecioActual,
                Stock = producto.Stock,
                Talla = producto.Talla,
                Detalle = producto.Detalle,
                ImagenURL = producto.ImagenURL,
                IDCategoria = producto.IDCategoria
            };

            return Ok(productoDto);
        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductoDto>> Create([FromForm] ProductoCreacionDto productoCreacionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? imagenUrl = null;

            if (productoCreacionDto.ImagenFile != null && productoCreacionDto.ImagenFile.Length > 0)
            {
                if (productoCreacionDto.ImagenFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("La imagen supera el límite de 5MB permitido.");
                }

                try
                {
                    using var ms = new MemoryStream();
                    await productoCreacionDto.ImagenFile.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var base64Image = Convert.ToBase64String(bytes);

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(productoCreacionDto.ImagenFile.FileName);

                    var gitHubToken = _configuration["GitHub:Token"];
                    var repo = _configuration["GitHub:Repo"];

                    if (string.IsNullOrEmpty(gitHubToken) || string.IsNullOrEmpty(repo))
                    {
                        _logger.LogError("Faltan configuraciones para GitHub Token o Repo en appsettings.json.");
                        return StatusCode(StatusCodes.Status500InternalServerError, "Error de configuración: GitHub Token o Repo no configurados.");
                    }

                    var url = $"https://api.github.com/repos/{repo}/contents/images/{nombreArchivo}";

                    var payload = new
                    {
                        message = "Subida de imagen desde API",
                        content = base64Image
                    };

                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("proyectoTienda/1.0");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gitHubToken);

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PutAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Error al subir a GitHub: {response.StatusCode}, {responseContent}");
                        return StatusCode(StatusCodes.Status500InternalServerError, $"Error al subir la imagen a GitHub: {response.StatusCode} - {responseContent}");
                    }

                    imagenUrl = $"https://raw.githubusercontent.com/{repo}/main/images/{nombreArchivo}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar la subida de imagen.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la imagen.");
                }
            }
            else
            {
                return BadRequest("No se proporcionó una imagen para el producto.");
            }

            var producto = new Producto
            {
                Nombre = productoCreacionDto.Nombre,
                Descripcion = productoCreacionDto.Descripcion,
                PrecioOriginal = productoCreacionDto.PrecioOriginal,
                PrecioActual = productoCreacionDto.PrecioActual,
                Stock = productoCreacionDto.Stock,
                Talla = productoCreacionDto.Talla,
                Detalle = productoCreacionDto.Detalle,
                IDCategoria = productoCreacionDto.IDCategoria,
                ImagenURL = imagenUrl
            };

            _context.Productos.Add(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar el nuevo producto en la base de datos.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al crear el producto en la base de datos.");
            }

            var createdProductoDto = new ProductoDto
            {
                IDProducto = producto.IDProducto,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioOriginal = producto.PrecioOriginal,
                PrecioActual = producto.PrecioActual,
                Stock = producto.Stock,
                Talla = producto.Talla,
                Detalle = producto.Detalle,
                ImagenURL = producto.ImagenURL,
                IDCategoria = producto.IDCategoria
            };

            return CreatedAtAction(nameof(Get), new { id = createdProductoDto.IDProducto }, createdProductoDto);
        }



        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductoDto>> Update(int id, [FromForm] ProductoActualizacionDto productoActualizacionDto)
        {
            if (id != productoActualizacionDto.IDProducto)
            {
                return BadRequest("El ID del producto en la URL no coincide con el ID en el cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productoExistente = await _context.Productos.FindAsync(id);

            if (productoExistente == null)
            {
                return NotFound($"Producto con ID {id} no encontrado.");
            }


            string? nuevaImagenUrl = productoExistente.ImagenURL;

            if (productoActualizacionDto.ImagenFile != null && productoActualizacionDto.ImagenFile.Length > 0)
            {
                if (productoActualizacionDto.ImagenFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("La nueva imagen supera el límite de 5MB permitido.");
                }

                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedMimeTypes.Contains(productoActualizacionDto.ImagenFile.ContentType))
                {
                    return BadRequest("Tipo de archivo de imagen no permitido. Solo se aceptan JPEG, PNG, GIF o WEBP.");
                }

                try
                {
                    using var ms = new MemoryStream();
                    await productoActualizacionDto.ImagenFile.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var base64Image = Convert.ToBase64String(bytes);

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(productoActualizacionDto.ImagenFile.FileName);

                    var gitHubToken = _configuration["GitHub:Token"];
                    var repo = _configuration["GitHub:Repo"];

                    if (string.IsNullOrEmpty(gitHubToken) || string.IsNullOrEmpty(repo))
                    {
                        _logger.LogError("Faltan configuraciones para GitHub Token o Repo en appsettings.json.");
                        return StatusCode(StatusCodes.Status500InternalServerError, "Error de configuración: GitHub Token o Repo no configurados.");
                    }

                    var url = $"https://api.github.com/repos/{repo}/contents/images/{nombreArchivo}";

                    var payload = new
                    {
                        message = $"Actualización de imagen para producto {id}",
                        content = base64Image
                    };

                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("proyectoTienda/1.0");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gitHubToken);

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PutAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Error al subir nueva imagen a GitHub: {response.StatusCode}, {responseContent}");
                        return StatusCode(StatusCodes.Status500InternalServerError, $"Error al subir la nueva imagen a GitHub: {response.StatusCode} - {responseContent}");
                    }

                    nuevaImagenUrl = $"https://raw.githubusercontent.com/{repo}/main/images/{nombreArchivo}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar la subida de la nueva imagen.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la nueva imagen.");
                }
            }

            if (productoActualizacionDto.Nombre != null) productoExistente.Nombre = productoActualizacionDto.Nombre;
            if (productoActualizacionDto.Descripcion != null) productoExistente.Descripcion = productoActualizacionDto.Descripcion;
            if (productoActualizacionDto.PrecioOriginal.HasValue) productoExistente.PrecioOriginal = productoActualizacionDto.PrecioOriginal.Value;
            if (productoActualizacionDto.PrecioActual.HasValue) productoExistente.PrecioActual = productoActualizacionDto.PrecioActual.Value;
            if (productoActualizacionDto.Stock.HasValue) productoExistente.Stock = productoActualizacionDto.Stock.Value;
            if (productoActualizacionDto.Talla != null) productoExistente.Talla = productoActualizacionDto.Talla;
            if (productoActualizacionDto.Detalle != null) productoExistente.Detalle = productoActualizacionDto.Detalle;
            if (productoActualizacionDto.IDCategoria.HasValue) productoExistente.IDCategoria = productoActualizacionDto.IDCategoria.Value;
            if (nuevaImagenUrl != null) productoExistente.ImagenURL = nuevaImagenUrl;
            if (productoActualizacionDto.IDCategoria.HasValue && productoExistente.IDCategoria != productoActualizacionDto.IDCategoria.Value)
            {
                var nuevaCategoria = await _context.Categorias.FindAsync(productoActualizacionDto.IDCategoria.Value);
                if (nuevaCategoria != null)
                {
                    productoExistente.Categoria = nuevaCategoria;
                }
                else
                {
                    return BadRequest($"La nueva categoría con ID {productoActualizacionDto.IDCategoria.Value} no existe.");
                }
            }


            _context.Productos.Update(productoExistente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error al actualizar el producto con ID {id} en la base de datos.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocurrió un error al actualizar el producto.");
            }

            var updatedProductoDto = new ProductoDto
            {
                IDProducto = productoExistente.IDProducto,
                Nombre = productoExistente.Nombre,
                Descripcion = productoExistente.Descripcion,
                PrecioOriginal = productoExistente.PrecioOriginal,
                PrecioActual = productoExistente.PrecioActual,
                Stock = productoExistente.Stock,
                Talla = productoExistente.Talla,
                Detalle = productoExistente.Detalle,
                ImagenURL = productoExistente.ImagenURL,
                IDCategoria = productoExistente.IDCategoria
            };

            return Ok(updatedProductoDto);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound($"Producto con ID {id} no encontrado.");
            }

            _context.Productos.Remove(producto);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error al eliminar el producto con ID {id} de la base de datos.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocurrió un error al eliminar el producto.");
            }
        }

    }   
}