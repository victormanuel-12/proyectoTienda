using System.Text.Json;
using System.Text.Json.Serialization;
using proyectoTienda.Models.DTO;

namespace proyectoTienda.Servicios
{
  public class SubstackService
  {
    private readonly HttpClient _httpClient;
    private const string NewsletterUrl = "https://astyleset.substack.com";

    public SubstackService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<List<SubstackPostDto>> ObtenerPublicacionesAsync()
    {
      try
      {
        var url = $"{NewsletterUrl}/api/v1/posts?limit=4";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var posts = JsonSerializer.Deserialize<List<SubstackPostRaw>>(json, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        return posts?.Select(p => new SubstackPostDto
        {
          Title = p.Title ?? "",
          Url = p.CanonicalUrl ?? "",
          Summary = p.Description ?? "",
          ImageUrl = p.CoverImage ?? ""
        }).ToList() ?? new List<SubstackPostDto>();
      }
      catch (Exception)
      {
        return new List<SubstackPostDto>();
      }
    }

    private class SubstackPostRaw
    {
      [JsonPropertyName("title")]
      public string? Title { get; set; }

      [JsonPropertyName("canonical_url")]
      public string? CanonicalUrl { get; set; }

      [JsonPropertyName("description")]
      public string? Description { get; set; }

      [JsonPropertyName("cover_image")]
      public string? CoverImage { get; set; }
    }
  }
}