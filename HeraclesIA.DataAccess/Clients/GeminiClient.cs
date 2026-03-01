using Google.GenAI;
using HeraclesIA.Application.Abstractions;
using HeraclesIA.DataBase.Settings;
using HeraclesIA.Types.Prompts;
using Microsoft.Extensions.Options;

namespace HeraclesIA.DataAccess.Clients;

public sealed class GeminiClient : IGeminiClient
{
    private readonly GeminiSettings _settings;

    public GeminiClient(IOptions<GeminiSettings> options)
    {
        _settings = options.Value ?? new GeminiSettings();
        if (string.IsNullOrWhiteSpace(_settings.ApiKey)) throw new InvalidOperationException("Falta Gemini:ApiKey");
        if (string.IsNullOrWhiteSpace(_settings.ModelId)) throw new InvalidOperationException("Falta Gemini:ModelId");
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        var client = new Google.GenAI.Client(apiKey: _settings.ApiKey);

        var response = await client.Models.GenerateContentAsync(
            model: _settings.ModelId,
            contents: prompt
        ).ConfigureAwait(false);

        if (response?.Candidates == null || response.Candidates.Count == 0)
            return string.Empty;

        foreach (var candidate in response.Candidates)
        {
            var parts = candidate?.Content?.Parts;
            if (parts != null && parts.Count > 0 && parts[0]?.Text != null)
                return parts[0].Text;
        }

        return string.Empty;
    }

    public async Task<string> Analizar(PromptType type, string data, string data2 = "", CancellationToken ct = default)
    {
        var prompt = Message.Build(type, data, data2);

        var counter = 0;
        var result = string.Empty;

        while (string.IsNullOrWhiteSpace(result))
        {
            if (counter++ >= 15) break;
            await Task.Delay(2000, ct).ConfigureAwait(false);
            result = await CallGeminiAsync(prompt, ct).ConfigureAwait(false);
        }

        return result ?? string.Empty;
    }
}
