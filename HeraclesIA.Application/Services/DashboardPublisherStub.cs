using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.Application.Services;

public sealed class DashboardPublisherStub : IDashboardPublisher
{
    public Task<IReadOnlyList<Page>> PublishAsync(IReadOnlyList<Dashboard> dashboards, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Page>)Array.Empty<Page>());
}
