// HeraclesIA.Application.UseCases/CreateCategoryFolders.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Common;
using Microsoft.Extensions.Logging;

namespace HeraclesIA.Application.UseCases;

public sealed class CreateCategoryFolders
{
    private readonly IOnBaseRepository _onBase;
    private readonly IFolderPublisher _folderPublisher;
    private readonly ILogger<CreateCategoryFolders> _logger;

    public CreateCategoryFolders(
        IOnBaseRepository onBase,
        IFolderPublisher folderPublisher,
        ILogger<CreateCategoryFolders> logger)
    {
        _onBase = onBase ?? throw new ArgumentNullException(nameof(onBase));
        _folderPublisher = folderPublisher ?? throw new ArgumentNullException(nameof(folderPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateFoldersResult> ExecuteAsync(string parentFolderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(parentFolderId))
            throw new ArgumentException("ParentFolderId requerido.", nameof(parentFolderId));

        var result = new CreateFoldersResult();

        IReadOnlyList<HelperEntity> processData;
        try
        {
            processData = await _onBase.GetProcessDataAsync(0, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error GetProcessDataAsync");
            result.Errores++;
            return result;
        }

        var grouped = processData
            .Where(x => x != null)
            .GroupBy(x => new { x.CategoryNum, x.CategoryName })
            .OrderBy(g => g.Key.CategoryName);

        var updates = new List<HelperEntity>();

        foreach (var group in grouped)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var alreadyHasFolder = group.Any(x => !string.IsNullOrWhiteSpace(x.FolderId));
                if (alreadyHasFolder)
                {
                    result.YaExistian++;
                    continue;
                }

                var folderName = group.Key.CategoryName?.Trim();
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    result.Omitidos++;
                    continue;
                }

                var created = await _folderPublisher.CreateFolderAsync(folderName, parentFolderId, ct).ConfigureAwait(false);

                updates.Add(new HelperEntity
                {
                    CategoryNum = group.Key.CategoryNum,
                    CategoryName = group.Key.CategoryName,
                    FolderId = created.PageId
                });

                result.Creados++;
            }
            catch (Exception ex)
            {
                result.Errores++;
                _logger.LogError(ex, "Error creando folder para CategoryNum={CategoryNum} Name={CategoryName}",
                    group.Key.CategoryNum, group.Key.CategoryName);
            }
        }

        if (updates.Count > 0)
        {
            try
            {
                await _onBase.SaveFolderIdAsync(updates, ct).ConfigureAwait(false);
                result.ActualizadosEnDb = updates.Count;
            }
            catch (Exception ex)
            {
                result.Errores++;
                _logger.LogError(ex, "Error SaveFolderIdAsync (updates={Count})", updates.Count);
            }
        }

        return result;
    }
}
