// HeraclesIA.Application.Services/DashboardPublisherConfluence.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.DataAccess.Abstractions;
using HeraclesIA.DataBase.Settings;
using HeraclesIA.Types.Dashboards;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace HeraclesIA.Application.Services;

public sealed class DashboardPublisherConfluence : IDashboardPublisher
{
    private readonly IConfluenceClient _confluence;
    private readonly ConfluenceSettings _settings;

    public DashboardPublisherConfluence(IConfluenceClient confluence, IOptions<ConfluenceSettings> options)
    {
        _confluence = confluence ?? throw new ArgumentNullException(nameof(confluence));
        _settings = options.Value ?? new ConfluenceSettings();

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl)) throw new InvalidOperationException("Falta Confluence:BaseUrl");
        if (string.IsNullOrWhiteSpace(_settings.SpaceKey)) throw new InvalidOperationException("Falta Confluence:SpaceKey");
        if (string.IsNullOrWhiteSpace(_settings.Username)) throw new InvalidOperationException("Falta Confluence:Username");
        if (string.IsNullOrWhiteSpace(_settings.ApiToken)) throw new InvalidOperationException("Falta Confluence:ApiToken");
    }

    public async Task<IReadOnlyList<Page>> PublishAsync(IReadOnlyList<Dashboard> dashboards, CancellationToken ct = default)
    {
        if (dashboards == null || dashboards.Count == 0)
            return Array.Empty<Page>();

        var pages = new List<Page>();

        foreach (var d in dashboards)
        {
            ct.ThrowIfCancellationRequested();

            var title = $"{d.DashboardNum} - {d.DashboardName}".Trim();
            var html = BuildHtml(d);

            var pageId = await _confluence.CreateOrUpdatePageAsync(
                _settings.SpaceKey,
                title,
                html,
                string.IsNullOrWhiteSpace(_settings.ParentPageId) ? null : _settings.ParentPageId,
                ct
            ).ConfigureAwait(false);

            pages.Add(new Page
            {
                PageId = pageId,
                Url = string.IsNullOrWhiteSpace(pageId) ? string.Empty : $"{_settings.BaseUrl.TrimEnd('/')}/pages/viewpage.action?pageId={pageId}"
            });
        }

        return pages;
    }

    private static string BuildHtml(Dashboard dashboard)
    {
        var sb = new StringBuilder();

        sb.Append("<h2>Descripción</h2>");
        sb.Append("<p>");
        sb.Append(System.Net.WebUtility.HtmlEncode(dashboard.Description ?? string.Empty));
        sb.Append("</p>");

        sb.Append("<h2>JSON</h2>");
        var json = JsonConvert.SerializeObject(dashboard, Formatting.Indented);
        sb.Append("<ac:structured-macro ac:name='code'>");
        sb.Append("<ac:parameter ac:name='language'>json</ac:parameter>");
        sb.Append("<ac:plain-text-body><![CDATA[");
        sb.Append(json);
        sb.Append("]]></ac:plain-text-body>");
        sb.Append("</ac:structured-macro>");

        return sb.ToString();
    }
}
