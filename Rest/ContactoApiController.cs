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
using proyectoTienda.DTO;
using proyectoTienda.Models.Model;
using proyectoTienda.Models.DTO;

namespace proyectoTienda.Rest
{
    [ApiController]
    [Route("api/contacto")]
    [Authorize] // Solo usuarios autenticados pueden acceder
    public class ContactoApiController : Controller
    {
        private readonly ILogger<ContactoApiController> _logger;
        private readonly ApplicationDbContext _context;

        public ContactoApiController(ILogger<ContactoApiController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // POST: api/contacto
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ContactoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ContactoDto>> Create([FromForm] ContactoCreacionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var correoUsuario = User.Identity?.Name;
            if (correoUsuario == null)
            {
                _logger.LogWarning("Usuario no autenticado intentó enviar mensaje.");
                return Unauthorized();
            }

            var contacto = new Contacto
            {
                Correo = correoUsuario,
                Telefono = dto.Telefono,
                Mensaje = dto.Mensaje,
                FechaEnvio = DateTime.UtcNow
            };

            _context.Contacto.Add(contacto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar contacto.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al guardar el mensaje.");
            }

            var contactoDto = new ContactoDto
            {
                Id = contacto.Id,
                Correo = contacto.Correo,
                Mensaje = contacto.Mensaje,
                FechaEnvio = contacto.FechaEnvio
            };

            return CreatedAtAction(nameof(Get), new { id = contactoDto.Id }, contactoDto);
        }

        // GET: api/contacto
        [HttpGet]
        [Authorize(Roles = "ADMIN")] // Solo administradores pueden listar
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ContactoDto>))]
        public async Task<ActionResult<IEnumerable<ContactoDto>>> Get()
        {
            var contacto = await _context.Contacto
                .OrderByDescending(c => c.FechaEnvio)
                .Select(c => new ContactoDto
                {
                    Id = c.Id,
                    Correo = c.Correo,
                    Mensaje = c.Mensaje,
                    FechaEnvio = c.FechaEnvio
                }).ToListAsync();

            return Ok(contacto);
        }

        // GET: api/contacto/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")] // Solo administradores pueden ver un mensaje específico
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContactoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactoDto>> Get(int id)
        {
            var contacto = await _context.Contacto.FindAsync(id);
            if (contacto == null)
                return NotFound();

            var dto = new ContactoDto
            {
                Id = contacto.Id,
                Correo = contacto.Correo,
                Mensaje = contacto.Mensaje,
                FechaEnvio = contacto.FechaEnvio
            };

            return Ok(dto);
        }
    }
}
