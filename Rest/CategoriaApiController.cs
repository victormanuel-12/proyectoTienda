using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using proyectoTienda.Data;
using proyectoTienda.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Models.DTO;

namespace proyectoTienda.Rest
{
    [ApiController]
    [Route("api/categoria")]
    public class CategoriaApiController : Controller
    {
        private readonly ILogger<CategoriaApiController> _logger;
        private readonly ApplicationDbContext _context;

        public CategoriaApiController(ILogger<CategoriaApiController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoriaDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetTodos()
        {
            var categorias = await _context.Categorias.ToListAsync();
            if (categorias == null || !categorias.Any())
            {
                return NotFound();
            }

            var categoriasDto = categorias.Select(c => new CategoriaDto
            {
                IDCategoria = c.IDCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                FechaCreacion = c.FechaCreacion
            }).ToList();

            return Ok(categoriasDto);
        }


        

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoriaDto>> Get(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            var categoriaDto = new CategoriaDto
            {
                IDCategoria = categoria.IDCategoria,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                FechaCreacion = categoria.FechaCreacion
            };
            return Ok(categoriaDto);
        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoriaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CategoriaDto>> Create([FromForm] CategoriaCreacionDto categoriaCreacionDto)
        {
            if (categoriaCreacionDto == null)
            {
                return BadRequest("Los datos de la categoría son inválidos.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validación de modelo fallida para nueva categoría: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            if (await _context.Categorias.AnyAsync(c => c.Nombre == categoriaCreacionDto.Nombre))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");
                _logger.LogWarning("Intento de crear categoría con nombre duplicado: {Nombre}", categoriaCreacionDto.Nombre);
                return Conflict(ModelState);
            }

            var categoria = new Categoria
            {
                Nombre = categoriaCreacionDto.Nombre,
                Descripcion = categoriaCreacionDto.Descripcion,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Categorias.Add(categoria);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Categoría {Nombre} (ID: {IDCategoria}) creada exitosamente.", categoria.Nombre, categoria.IDCategoria);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la nueva categoría '{Nombre}' en la base de datos.", categoria.Nombre);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al crear la categoría.");
            }

            var createdCategoriaDto = new CategoriaDto
            {
                IDCategoria = categoria.IDCategoria,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                FechaCreacion = categoria.FechaCreacion
            };

            return CreatedAtAction(nameof(Get), new { id = createdCategoriaDto.IDCategoria }, createdCategoriaDto);
        }




        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoriaDto>> Update(int id, [FromForm] CategoriaActualizacionDto categoriaActualizacionDto)
        {
            if (id != categoriaActualizacionDto.IDCategoria)
            {
                return BadRequest("El ID de la categoría en la URL no coincide con el ID en el cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoriaExistente = await _context.Categorias.FindAsync(id);

            if (categoriaExistente == null)
            {
                return NotFound($"Categoría con ID {id} no encontrada.");
            }

            if (categoriaActualizacionDto.Nombre != null &&
                !string.Equals(categoriaActualizacionDto.Nombre, categoriaExistente.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Categorias.AnyAsync(c => c.Nombre!.ToLower() == categoriaActualizacionDto.Nombre.ToLower()))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otra categoría con este nombre.");
                    return Conflict(ModelState);
                }
            }

            if (categoriaActualizacionDto.Nombre != null)
            {
                categoriaExistente.Nombre = categoriaActualizacionDto.Nombre;
            }
            if (categoriaActualizacionDto.Descripcion != null)
            {
                categoriaExistente.Descripcion = categoriaActualizacionDto.Descripcion;
            }

            _context.Entry(categoriaExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categorias.Any(e => e.IDCategoria == id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoría con ID {Id} en la base de datos.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocurrió un error al actualizar la categoría.");
            }

            var updatedCategoriaDto = new CategoriaDto
            {
                IDCategoria = categoriaExistente.IDCategoria,
                Nombre = categoriaExistente.Nombre,
                Descripcion = categoriaExistente.Descripcion,
                FechaCreacion = categoriaExistente.FechaCreacion
            };

            return Ok(updatedCategoriaDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound($"Categoría con ID {id} no encontrada.");

            _context.Categorias.Remove(categoria);
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error al eliminar la categoría con ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al eliminar la categoría.");
            }
        }

    }
}