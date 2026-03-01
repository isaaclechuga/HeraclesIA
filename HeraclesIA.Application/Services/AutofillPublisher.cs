using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Autofill;
using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Services
{
    public sealed class AutofillPublisher : IAutofillPublisher
    {
        private readonly IConfluenceClient _confluence;
        private readonly PublisherOptions _opt;

        public sealed class PublisherOptions
        {
            public bool Enabled { get; set; } = true;
            public string DefaultParentId { get; set; } = ""; // si no tienes parent por jira, ponlo aquí
        }

        public AutofillPublisher(IConfluenceClient confluence, PublisherOptions opt)
        {
            _confluence = confluence;
            _opt = opt;
        }

        public async Task<IReadOnlyList<Page>> PublishAsync(AutoFill autofill, string code, References references, CancellationToken ct)
        {
            if (!_opt.Enabled)
                return  Array.Empty<Page>();

            var title = $"{autofill.AutofillId}. {autofill.Nombre}";

            // Si tú tienes parentId por Jira, aquí lo conectas. En el viejo no estaba implementado.
            var parentId = _opt.DefaultParentId;

            var md = BuildMarkdown(autofill, references);
            var snippet = BuildCodeSnippet(code);

            var page = await _confluence.CreateOrUpdatePageAsync(title, md, parentId, snippet, ct);
            return new[] { page };
        }

        private static string BuildMarkdown(AutoFill autofill, References references)
        {
            // equivalente al viejo: crear contenido + luego ir agregando
            // aquí lo centralizamos en una sola “primera sección”
            var refs = references.Items;
            var lines = new List<string>
        {
            $"## Autofill",
            $"- **Id:** {autofill.AutofillId}",
            $"- **Nombre:** {autofill.Nombre}",
            $"- **UnityScriptId:** {autofill.UnityScriptId}",
            "",
            "## Referencias externas detectadas",
            refs.Count == 0 ? "_No se detectaron referencias._" : ""
        };

            foreach (var proj in refs.GroupBy(r => r.Name))
            {
                lines.Add($"### {proj.Key}");
                foreach (var r in proj)
                {
                    foreach (var cls in r.Clases.GroupBy(c => c.Nombre))
                    {
                        lines.Add($"- **Clase:** {cls.Key}");
                        foreach (var c in cls)
                            foreach (var m in c.Metodos)
                                lines.Add($"  - {m.Nombre}");
                    }
                }
                lines.Add("");
            }

            return string.Join("\n", lines);
        }

        private static string BuildCodeSnippet(string code)
        {
            // Confluence storage macro (como tu viejo Message.CodeSnipet)
            return $@"
<p>Fragmento de Código Analizado:</p>
<ac:structured-macro ac:name='code'>
  <ac:parameter ac:name='language'>csharp</ac:parameter>
  <ac:plain-text-body><![CDATA[{code}]]></ac:plain-text-body>
</ac:structured-macro>";
        }
    }
}
