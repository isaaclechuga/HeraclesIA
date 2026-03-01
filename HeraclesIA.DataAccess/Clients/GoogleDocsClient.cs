using HeraclesIA.DataAccess.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.DataAccess.Clients
{
    public sealed class GoogleDocsClient : IGoogleDocsClient
    {
        private readonly HttpClient _http;

        public GoogleDocsClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<string> CreateDocumentAsync(string title, CancellationToken ct = default)
        {
            // Stub: tu implementación real depende de OAuth y Google APIs.
            // Dejo contrato y una excepción clara si no está configurado.
            throw new NotSupportedException("GoogleDocsClient requiere OAuth + Google Docs API. Implementación pendiente.");
        }

        public Task AppendHtmlAsync(string documentId, string html, CancellationToken ct = default)
        {
            throw new NotSupportedException("GoogleDocsClient requiere OAuth + Google Docs API. Implementación pendiente.");
        }
    }
}
