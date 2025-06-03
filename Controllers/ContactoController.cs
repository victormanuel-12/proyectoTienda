using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyectoTienda.Data;
using proyectoTienda.Models.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using proyectoTienda.ML;


namespace proyectoTienda.Controllers
{
    public class ContactoController : Controller
    {
        private readonly ILogger<ContactoController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ContactoController(ILogger<ContactoController> logger,UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {   
            var user = await _userManager.GetUserAsync(User);

            Contacto contacto = new Contacto();

            if (user != null)
            {
                contacto.Nombre = user.UserName;
                contacto.Correo = user.Email;
                contacto.Telefono = user.PhoneNumber ?? string.Empty;

            }

            if (TempData["Resultado"] != null)
            {
                ViewData["Message"] = TempData["Resultado"].ToString();
            }

            return View("Index", contacto);
        }


        [HttpPost]
        public async Task<IActionResult> RegistrarContacto(Contacto contacto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        user.PhoneNumber = contacto.Telefono;
                        await _userManager.UpdateAsync(user);
                    }
                }


                    //Load sample data
                var sampleData = new MLModelSentimentalAnalysis.ModelInput()
                {
                    Comentario = contacto.Mensaje
                };

                //Load model and predict output
                var result = MLModelSentimentalAnalysis.Predict(sampleData);
                var predictedLabel = result.PredictedLabel;
                var scorePositive = result.Score[0];
                var scoreNegative = result.Score[1];
                //Check if the result is positive or negative
                if (predictedLabel == 1)
                {
                    contacto.Etiqueta = "Positivo";
                    contacto.Puntuacion = scorePositive;
                }
                else
                {
                    contacto.Etiqueta = "Negativo";
                    contacto.Puntuacion = scoreNegative;
                }

                _context.Contacto.Add(contacto);
                await _context.SaveChangesAsync();

                TempData["Resultado"] = "Mensaje enviado correctamente. El comentario es " + contacto.Etiqueta;
                return RedirectToAction("Index", contacto);
            }

            return View("Index", contacto);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}