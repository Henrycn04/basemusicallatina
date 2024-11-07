using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Data.SqlClient;

public class MusicalCategoryDto
{
    [Newtonsoft.Json.JsonIgnore]
    public int Id { get; set; }

    // Serializar 'Label' como 'label' en el JSON resultante
    [JsonProperty("label")]
    public string Label { get; set; }

    // No se serializa 'ParentId'
    [Newtonsoft.Json.JsonIgnore]
    public int? ParentId { get; set; }

    [Newtonsoft.Json.JsonIgnore]

    public List<MusicalCategoryDto> Children { get; set; } = new List<MusicalCategoryDto>();

    // Para la serialización, crear una propiedad que solo incluya 'Children' si tiene hijos
    [JsonProperty("children")]
    public List<MusicalCategoryDto> ChildrenToSerialize => Children.Count > 0 ? Children : null;
}
