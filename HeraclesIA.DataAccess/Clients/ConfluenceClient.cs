// HeraclesIA.DataAccess.Clients/ConfluenceClient.cs
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HeraclesIA.DataBase.Abstractions;
using HeraclesIA.DataBase.Settings;
using HeraclesIA.Types.Dashboards;
using Markdig;
using Microsoft.Extensions.Options;

namespace HeraclesIA.DataAccess.Clients;

public sealed class ConfluenceClient : IConfluenceClient
{
    private readonly HttpClient _http;
    private readonly ConfluenceOptions _opt;

    public ConfluenceClient(HttpClient http, IOptions<ConfluenceOptions> opt)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _opt = opt?.Value ?? throw new ArgumentNullException(nameof(opt));

        if (string.IsNullOrWhiteSpace(_opt.ApiUrl)) throw new InvalidOperationException("Falta Confluence:ApiUrl (se deriva de ConfluenceUrl en PostConfigure).");
        if (string.IsNullOrWhiteSpace(_opt.BaseUrl)) throw new InvalidOperationException("Falta Confluence:BaseUrl.");
        if (string.IsNullOrWhiteSpace(_opt.SpaceKey)) throw new InvalidOperationException("Falta Confluence:SpaceKey.");
        if (string.IsNullOrWhiteSpace(_opt.Username)) throw new InvalidOperationException("Falta Confluence:Username.");
        if (string.IsNullOrWhiteSpace(_opt.ApiToken)) throw new InvalidOperationException("Falta Confluence:ApiToken.");

        // Configuración base del HttpClient (idempotente)
        var apiBase = _opt.ApiUrl.TrimEnd('/') + "/";
        if (_http.BaseAddress == null || !_http.BaseAddress.AbsoluteUri.Equals(apiBase, StringComparison.OrdinalIgnoreCase))
            _http.BaseAddress = new Uri(apiBase);

        var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.Username}:{_opt.ApiToken}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", raw);

        // Confluence REST usa JSON
        if (!_http.DefaultRequestHeaders.Accept.Any(h => h.MediaType?.Equals("application/json", StringComparison.OrdinalIgnoreCase) == true))
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Page> CreateOrUpdatePageAsync(
        string title,
        string markdownContent,
        string parentId,
        string snippetHtml,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("title requerido.", nameof(title));

        // 1) Buscar por title + spaceKey
        var existing = await FindByTitleAsync(_opt.SpaceKey, title.Trim(), ct).ConfigureAwait(false);

        if (existing is null)
        {
            // CREATE
            return await CreateAsync(title.Trim(), markdownContent, parentId, snippetHtml, ct).ConfigureAwait(false);
        }

        // UPDATE (append al body existente)
        return await UpdateAppendAsync(existing.Value.PageId, title.Trim(), markdownContent, snippetHtml, ct).ConfigureAwait(false);
    }

    // ---------------------------
    // FIND
    // ---------------------------
    private async Task<(string PageId, int Version)?> FindByTitleAsync(string spaceKey, string title, CancellationToken ct)
    {
        // GET content?spaceKey=PH&title=...&expand=version,_links&limit=1
        var url =
            $"content?type=page" +
            $"&spaceKey={Uri.EscapeDataString(spaceKey)}" +
            $"&title={Uri.EscapeDataString(title)}" +
            $"&expand=version,_links" +
            $"&limit=1";

        using var res = await _http.GetAsync(url, ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return null;

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("results", out var results) ||
            results.ValueKind != JsonValueKind.Array ||
            results.GetArrayLength() == 0)
            return null;

        var first = results[0];

        var id = first.GetProperty("id").GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id)) return null;

        var version = first.GetProperty("version").GetProperty("number").GetInt32();
        return (id, version);
    }

    // ---------------------------
    // CREATE
    // ---------------------------
    private async Task<Page> CreateAsync(string title, string markdownContent, string parentId, string snippetHtml, CancellationToken ct)
    {
        var xhtml = ToXhtml(markdownContent);
        var body = $"{xhtml}{snippetHtml ?? string.Empty}";

        object[] ancestors =
            string.IsNullOrWhiteSpace(parentId)
                ? Array.Empty<object>()
                : new object[] { new { id = parentId } };

        var payload = new
        {
            type = "page",
            title,
            space = new { key = _opt.SpaceKey },
            ancestors,
            body = new
            {
                storage = new
                {
                    value = body,
                    representation = "storage"
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        using var res = await _http.PostAsync(
            "content",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct).ConfigureAwait(false);

        res.EnsureSuccessStatusCode();

        var createdJson = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return ParsePageFromContentResponse(createdJson);
    }

    // ---------------------------
    // UPDATE (append)
    // ---------------------------
    private async Task<Page> UpdateAppendAsync(string pageId, string title, string markdownContent, string snippetHtml, CancellationToken ct)
    {
        // Traer body actual + version + links
        var getUrl = $"content/{pageId}?expand=body.storage,version,_links";
        using var getRes = await _http.GetAsync(getUrl, ct).ConfigureAwait(false);
        getRes.EnsureSuccessStatusCode();

        var existingJson = await getRes.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var existing = ParseExistingForUpdate(existingJson);

        var newXhtml = ToXhtml(markdownContent);
        var updatedBody = $"{existing.Body}{newXhtml}{snippetHtml ?? string.Empty}";

        var payload = new
        {
            id = pageId,
            type = "page",
            title,
            body = new
            {
                storage = new
                {
                    value = updatedBody,
                    representation = "storage"
                }
            },
            version = new { number = existing.Version + 1 }
        };

        var json = JsonSerializer.Serialize(payload);
        using var putRes = await _http.PutAsync(
            $"content/{pageId}",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct).ConfigureAwait(false);

        putRes.EnsureSuccessStatusCode();

        // URL final: BaseUrl + webui (ya sin hardcode)
        return new Page
        {
            PageId = pageId,
            Url = $"{_opt.BaseUrl.TrimEnd('/')}{existing.WebUi}"
        };
    }

    // ---------------------------
    // PARSERS
    // ---------------------------
    private Page ParsePageFromContentResponse(string jsonResult)
    {
        using var doc = JsonDocument.Parse(jsonResult);

        var id = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        var webui = "";
        if (doc.RootElement.TryGetProperty("_links", out var links) &&
            links.TryGetProperty("webui", out var w))
            webui = w.GetString() ?? "";

        return new Page
        {
            PageId = id,
            Url = string.IsNullOrWhiteSpace(webui)
                ? string.Empty
                : $"{_opt.BaseUrl.TrimEnd('/')}{webui}"
        };
    }

    private static (string Body, int Version, string WebUi) ParseExistingForUpdate(string existingJson)
    {
        using var doc = JsonDocument.Parse(existingJson);

        var body = doc.RootElement
            .GetProperty("body")
            .GetProperty("storage")
            .GetProperty("value")
            .GetString() ?? string.Empty;

        var version = doc.RootElement
            .GetProperty("version")
            .GetProperty("number")
            .GetInt32();

        var webui = doc.RootElement
            .GetProperty("_links")
            .GetProperty("webui")
            .GetString() ?? string.Empty;

        return (body, version, webui);
    }

    // ---------------------------
    // MARKDOWN -> XHTML storage
    // ---------------------------
    private static string ToXhtml(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var html = Markdown.ToHtml(markdown ?? string.Empty, pipeline);

        // Confluence storage prefiere XHTML self-closing en algunos tags
        html = html.Replace("<br>", "<br/>", StringComparison.OrdinalIgnoreCase)
                   .Replace("<hr>", "<hr/>", StringComparison.OrdinalIgnoreCase);

        return html;
    }
}