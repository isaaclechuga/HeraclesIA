// HeraclesIA.Application.Services/FolderPublisherConfluence.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.DataAccess.Abstractions;
using HeraclesIA.DataBase.Settings;
using HeraclesIA.Types.Dashboards;
using Microsoft.Extensions.Options;
using System.Net;

namespace HeraclesIA.Application.Services;

public sealed class FolderPublisherConfluence : IFolderPublisher
{
    private readonly IConfluenceClient _confluence;
    private readonly ConfluenceSettings _settings;

    public FolderPublisherConfluence(IConfluenceClient confluence, IOptions<ConfluenceSettings> options)
    {
        _confluence = confluence ?? throw new ArgumentNullException(nameof(confluence));
        _settings = options.Value ?? new ConfluenceSettings();

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl)) throw new InvalidOperationException("Falta Confluence:BaseUrl");
        if (string.IsNullOrWhiteSpace(_settings.SpaceKey)) throw new InvalidOperationException("Falta Confluence:SpaceKey");
        if (string.IsNullOrWhiteSpace(_settings.Username)) throw new InvalidOperationException("Falta Confluence:Username");
        if (string.IsNullOrWhiteSpace(_settings.ApiToken)) throw new InvalidOperationException("Falta Confluence:ApiToken");
    }

    public async Task<Page> CreateFolderAsync(string folderName, string parentFolderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(folderName)) throw new ArgumentException("folderName requerido.", nameof(folderName));
        if (string.IsNullOrWhiteSpace(parentFolderId)) throw new ArgumentException("parentFolderId requerido.", nameof(parentFolderId));

        var title = folderName.Trim();
        var htmlBody = $"<h1>{WebUtility.HtmlEncode(title)}</h1>";

        var pageId = await _confluence.CreateOrUpdatePageAsync(
            _settings.SpaceKey,
            title,
            htmlBody,
            parentFolderId,
            ct
        ).ConfigureAwait(false);

        return new Page
        {
            PageId = pageId,
            Url = string.IsNullOrWhiteSpace(pageId) ? string.Empty : $"{_settings.BaseUrl.TrimEnd('/')}/pages/viewpage.action?pageId={pageId}"
        };
    }
}
