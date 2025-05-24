using System.Diagnostics;
using System.Text.Json;
using proyectoTienda.Models.DTO;

namespace proyectoTienda.Servicios
{
    public class SubstackService
    {
        private readonly string _scriptPath = "PythonScripts/substack_reader.py";

        public async Task<List<SubstackPostDto>> ObtenerPublicacionesAsync()
        {
            var start = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = _scriptPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(start)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var publicaciones = JsonSerializer.Deserialize<List<SubstackPostDto>>(output);
            return publicaciones ?? new List<SubstackPostDto>();
        }


    }

}
