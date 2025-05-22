using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Facebook;
using Newtonsoft.Json.Linq;

namespace proyectoTienda.Controllers
{
    public class PublicarController : Controller
    {
        private readonly ILogger<PublicarController> _logger;

        private readonly string _facebookToken = "EAAIXIQC596ABOxPaz1N71RFnZBF866n9AE96vG6yFBfEnLeVidoC6ImMZA1tUkf53YO3DtTf0R5vMKH7aEYcNf8w7bGgPnv8IjdFyxrXaLtZAegY8hWphTUahyH0An4Oj7LT7TDeOR9U017AZC1DTQxhjMCp9QKAJ1e8ZBJIRs6Si1nlZCdkeBezHsDQUsz3eP";

        public PublicarController(ILogger<PublicarController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Entrando a la vista Index.");
            return View();
        }


        [HttpGet("PublicacionExitosa")] 
        public IActionResult PublicacionExitosa(string fbUrl, string igUrl)
        {
            ViewBag.FacebookUrl = fbUrl;
            ViewBag.InstagramUrl = igUrl;
            return View();
        }

        
        [HttpPost("Publicar")]
        public async Task<IActionResult> Publicar(string titulo, string texto, string link, string comentario, string imagenUrl)
        {
            _logger.LogInformation("Iniciando proceso de publicación.");
            
            try
            {
                var fb = new FacebookClient("EAAIXIQC596ABO7p89qNLrdqLnhneewGrx9yR8zL3X4MwZAo7gOTx4O4ZAhiFeNWVVvymukt6W13uWDjDeO3Km9kHCYZC2HNyxfZCicgY507pRsgCZCpFV0drRIRqJebZA9opn3f9XvQGzOFJSOAzsePpZCNaFY1rtjYWHx2nKz8ZB8SnPYZA4yCl2PSrNrmjbHTYRlHGGTRDb");
                
        
                var fbParameters = new
                {
                    message = $"{titulo}\n{texto}\n{link}\n{comentario}",
                    link = imagenUrl
                };
                
                dynamic fbResult = fb.Post("668668769656344/feed", fbParameters);
                string fbPostId = fbResult.id;
                string fbPostUrl = $"https://www.facebook.com/{fbPostId}";
                
                _logger.LogInformation($"Publicación en Facebook exitosa: {fbPostUrl}");
                
                // 2. Publicar en Instagram
                try 
                {
                    // Obtener el ID de Instagram vinculado a la página
                    dynamic instagramAccount = fb.Get("668668769656344?fields=instagram_business_account");
                    string instagramId = instagramAccount.instagram_business_account.id;
                    
                    // Subir la imagen a Instagram (container)
                    var creationParams = new {
                        image_url = imagenUrl,
                        caption = $"{titulo}\n{texto}\n{link}\n{comentario}"
                    };
                    
                    dynamic container = fb.Post($"{instagramId}/media", creationParams);
                    string containerId = container.id;
                    
                    // Esperar a que el contenedor esté listo (puedes implementar retries)
                    await Task.Delay(5000);
                    
                    // Publicar el contenedor
                    dynamic publishResult = fb.Post($"{instagramId}/media_publish", new { creation_id = containerId });
                    string instagramPostUrl = $"https://www.instagram.com/p/{publishResult.id}";
                    
                    _logger.LogInformation($"Publicación en Instagram exitosa: {instagramPostUrl}");
                    
                    // Redirigir a ambas publicaciones (o mostrar ambas URLs)
                    return RedirectToAction("PublicacionExitosa", new { fbUrl = fbPostUrl, igUrl = instagramPostUrl });
                }
                catch (Exception instaEx)
                {
                    _logger.LogError($"Error al publicar en Instagram: {instaEx.Message}");
                    // Si falla Instagram pero Facebook tuvo éxito, igual redirigir
                    return Redirect(fbPostUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error general: {ex.Message}");
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogInformation("Mostrando vista de error.");
            return View("Error");
        }
    }
}
