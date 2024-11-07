using backend.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicalCategoriesController : ControllerBase
    {
        private MusicalCategoryHandler _handler;

        public MusicalCategoriesController()
        {
            _handler = new MusicalCategoryHandler();
        }
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok("El servidor está en funcionamiento.");
        }


        [HttpGet("hierarchical")]
        public async Task<IActionResult> GetHierarchicalCategories()
        {
            try
            {
                var result = await _handler.GetHierarchicalCategories();
                if (result == null || !result.Any())
                {
                    return Ok("No se encontraron categorías jerárquicas.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Registra el error detallado en los logs de la aplicación
                Console.WriteLine($"Error en el servidor: {ex.Message}");
                return Ok("Error interno en el servidor.");
            }
        }


        [HttpGet("GetExamples")]
        public async Task<IActionResult> GetExamples([FromQuery] List<string> categorias)
        {
            // Registra información general de la solicitud
            Console.WriteLine($"Método: {HttpContext.Request.Method}");
            Console.WriteLine($"URL: {HttpContext.Request.Path}");
            Console.WriteLine($"Cabeceras: {HttpContext.Request.Headers}");

            // Registra las categorías recibidas
            if (categorias == null || !categorias.Any())
            {
                Console.WriteLine("No se han proporcionado categorías.");
            }
            else
            {
                Console.WriteLine("Categorías recibidas: " + string.Join(", ", categorias));
            }

            var result = await _handler.ObtenerEjemplosPorCategorias(categorias);

            return Ok(result);
        }

    }
}
