// HeraclesIA.Application.Abstractions/IFolderPublisher.cs
using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.Application.Abstractions;

public interface IFolderPublisher
{
    Task<Page> CreateFolderAsync(string folderName, string parentFolderId, CancellationToken ct = default);
}
