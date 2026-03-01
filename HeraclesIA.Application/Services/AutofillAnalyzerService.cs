// HeraclesIA.Infrastructure/Services/AutofillAnalyzerService.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Autofill;
using System.Text.Json;

namespace HeraclesIA.Application.Services;

public sealed class AutofillAnalyzerService : IAutofillAnalyzerService
{
    private readonly IGeminiClient _gemini;

    public AutofillAnalyzerService(IGeminiClient gemini)
    {
        _gemini = gemini;
    }

    public async Task<References> AnalyzeExternalReferencesAsync(string code, CancellationToken ct)
    {
        // El viejo mandaba una “estructura de clases” como data2
        var schema = """
            public class Referencia
            {
                public string Name { get; set; }
                public List<Clase> Clases { get; set; }
            }

            public class Clase
            {
                public string Nombre { get; set; }
                public List<Metodo> Metodos { get; set; }
            }

            public class Metodo
            {
                public string Nombre { get; set; }
            }
        """;

        var json = await _gemini.ExternalReferenceAsync(code, schema, ct);

        // viejo: Replace("```json","").Replace("```","").Trim()
        var cleaned = (json ?? "")
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Equals("nulo", StringComparison.OrdinalIgnoreCase) || cleaned.Equals("null", StringComparison.OrdinalIgnoreCase))
            return new References();

        try
        {
            var items = JsonSerializer.Deserialize<List<Referencia>>(cleaned, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Referencia>();

            return new References { Items = items };
        }
        catch
        {
            // Si Gemini devuelve basura, no tronamos el pipeline
            return new References();
        }
    }
}
