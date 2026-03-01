using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IFileStore
    {
        Task SaveTextAsync(string relativePath, string content, CancellationToken ct = default);
        Task SaveJsonAsync<T>(string relativePath, T data, CancellationToken ct = default);
    }
}
