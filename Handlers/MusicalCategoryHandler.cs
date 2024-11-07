using backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace backend.Handlers
{
    public class MusicalCategoryHandler
    {
        private readonly SqlConnection _conexion;

        public MusicalCategoryHandler()
        {
            var builder = WebApplication.CreateBuilder();
            var _rutaConexion = builder.Configuration.GetConnectionString("CompanyDataContext");
            _conexion = new SqlConnection(_rutaConexion);
        }

        public async Task<string> GetHierarchicalCategories()
        {
            // Consulta SQL para obtener todas las categorías
            string query = "SELECT Id, Name, ParentId FROM MusicalCategories";

            // Crear una lista para almacenar las categorías
            var categories = new List<MusicalCategoryDto>();

            try
            {
                // Abrir la conexión a la base de datos
                await _conexion.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, _conexion))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new MusicalCategoryDto
                            {
                                Id = reader.GetInt32(0), // Id
                                Label = reader.GetString(1), // Name
                                ParentId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2), // ParentId
                                Children = new List<MusicalCategoryDto>() // Inicializar la lista de hijos
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores (log o lanzar la excepción)
                throw new Exception("Error al obtener las categorías musicales", ex);
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                await _conexion.CloseAsync();
            }

            // Crear un diccionario para organizar los datos en base a sus IDs
            var categoryMap = categories.ToDictionary(c => c.Id);

            // Organizar los datos en una estructura jerárquica
            var tree = new List<MusicalCategoryDto>();

            foreach (var category in categories)
            {
                if (category.ParentId.HasValue)
                {
                    // Si tiene un `ParentId`, agregarlo como hijo del padre correspondiente
                    categoryMap[category.ParentId.Value].Children.Add(category);
                }
                else
                {
                    // Si no tiene `ParentId`, es un nodo raíz, agregarlo al árbol
                    tree.Add(category);
                }
            }

            // Serializar el árbol a JSON, sin incluir la propiedad 'children' cuando está vacía
            var json = JsonConvert.SerializeObject(tree, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return json; // Devuelve el JSON en lugar de la lista
        }
        public async Task<List<Ejemplo>> ObtenerEjemplosPorCategorias(List<string> categoriaLabels)
        {
            string query;

            // Caso cuando no se seleccionan categorías o la lista está vacía
            if (categoriaLabels == null || !categoriaLabels.Any())
            {
                query = @"
            SELECT DISTINCT e.Nombre, e.RutaArchivo
            FROM Ejemplos e;";
            }
            else
            {
                // Generación dinámica de la cláusula WHERE
                var whereClause = string.Join(",", categoriaLabels.Select((_, index) => $"@CategoriaLabel{index}"));

                query = $@"
            WITH RecursiveCategories AS (
                SELECT Id FROM MusicalCategories WHERE Name IN ({whereClause})  
                UNION ALL
                SELECT mc.Id 
                FROM MusicalCategories mc
                INNER JOIN RecursiveCategories rc ON mc.ParentId = rc.Id
            )
            SELECT DISTINCT e.Nombre, e.RutaArchivo
            FROM Ejemplos e
            JOIN EjemploCategoria ec ON e.Id = ec.EjemploId
            WHERE ec.CategoriaId IN (SELECT Id FROM RecursiveCategories);";
            }

            var ejemplos = new List<Ejemplo>();

            try
            {
                await _conexion.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, _conexion))
                {
                    // Añadir parámetros para cada categoría
                    if (categoriaLabels != null && categoriaLabels.Any())
                    {
                        for (int i = 0; i < categoriaLabels.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@CategoriaLabel{i}", categoriaLabels[i]);
                        }
                    }

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ejemplos.Add(new Ejemplo
                            {
                                Nombre = reader.GetString(0),  // Nombre del ejemplo
                                RutaArchivo = reader.GetString(1)  // Ruta del archivo
                            });
                        }
                    }
                }
            }
            finally
            {
                await _conexion.CloseAsync();
            }

            return ejemplos;
        }



    }

}
