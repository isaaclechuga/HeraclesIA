using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.DataAccess.Abstractions
{
    public interface IGoogleDocsClient
    {
        Task<string> CreateDocumentAsync(string title, CancellationToken ct = default);
        Task AppendHtmlAsync(string documentId, string html, CancellationToken ct = default);
    }
}
