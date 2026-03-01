// HeraclesIA.DataBase.Abstractions/IConfluenceClient.cs
using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.DataBase.Abstractions;

public interface IConfluenceClient
{
    /// <summary>
    /// Crea o actualiza una página en Confluence por Title (en el Space configurado).
    /// - Si no existe, crea.
    /// - Si existe, hace update incrementando versión y appendea contenido nuevo al body actual.
    /// Retorna Page con PageId y Url.
    /// </summary>
    Task<Page> CreateOrUpdatePageAsync(
        string title,
        string markdownContent,
        string parentId,
        string snippetHtml,
        CancellationToken ct);
}